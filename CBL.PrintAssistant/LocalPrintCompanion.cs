using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace CBL.PrintAssistant
{
    public sealed class LocalPrintRequest
    {
        [JsonProperty("protocolVersion")] public int ProtocolVersion { get; set; }
        [JsonProperty("jobId")] public string JobId { get; set; } = "";
        [JsonProperty("profile")] public string Profile { get; set; } = "normal";
        [JsonProperty("copies")] public int Copies { get; set; } = 1;
        [JsonProperty("imageVariant")] public string ImageVariant { get; set; } = "composed";
        [JsonProperty("image")] public LocalPrintImage Image { get; set; } = new();
        [JsonProperty("context")] public LocalPrintContext Context { get; set; } = new();
        [JsonProperty("createdAt")] public string CreatedAt { get; set; } = "";
        [JsonProperty("auth")] public LocalPrintAuth Auth { get; set; } = new();
    }

    public sealed class LocalPrintImage
    {
        [JsonProperty("encoding")] public string Encoding { get; set; } = "base64";
        [JsonProperty("contentType")] public string ContentType { get; set; } = "image/jpeg";
        [JsonProperty("sha256")] public string Sha256 { get; set; } = "";
        [JsonProperty("data")] public string Data { get; set; } = "";
    }

    public sealed class LocalPrintContext
    {
        [JsonProperty("stationCode")] public string? StationCode { get; set; }
        [JsonProperty("stationId")] public string? StationId { get; set; }
        [JsonProperty("eventId")] public string? EventId { get; set; }
        [JsonProperty("creationKey")] public string? CreationKey { get; set; }
        [JsonProperty("captureId")] public string? CaptureId { get; set; }
    }

    public sealed class LocalPrintAuth
    {
        [JsonProperty("pairingId")] public string PairingId { get; set; } = "";
        [JsonProperty("nonce")] public string Nonce { get; set; } = "";
        [JsonProperty("ts")] public long Timestamp { get; set; }
        [JsonProperty("signature")] public string Signature { get; set; } = "";
    }

    public sealed class LocalPrintJobRecord
    {
        public string JobId { get; set; } = "";
        public int Copies { get; set; }
        public string ImageVariant { get; set; } = "composed";
        public string ImageSha256 { get; set; } = "";
        public string ImageFileName { get; set; } = "";
        public string Status { get; set; } = "queued";
        public int CopiesPrinted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
        public LocalPrintContext Context { get; set; } = new();
    }

    public sealed class LocalPrintJobStore
    {
        private readonly object _gate = new();
        private readonly string _statePath;
        private readonly string _imagesDirectory;
        private Dictionary<string, LocalPrintJobRecord> _jobs = new(StringComparer.OrdinalIgnoreCase);

        public LocalPrintJobStore(string root)
        {
            Directory.CreateDirectory(root);
            _statePath = Path.Combine(root, "jobs.json");
            _imagesDirectory = Path.Combine(root, "images");
            Directory.CreateDirectory(_imagesDirectory);
            Load();
        }

        public LocalPrintJobRecord? Get(string jobId)
        {
            lock (_gate) return _jobs.TryGetValue(jobId, out var job) ? Clone(job) : null;
        }

        public IEnumerable<LocalPrintJobRecord> RecoverQueuedJobs()
        {
            lock (_gate)
            {
                bool changed = false;
                foreach (var job in _jobs.Values.Where(x => x.Status == "printing"))
                {
                    job.Status = "failed";
                    job.ErrorCode = "interrupted";
                    job.ErrorMessage = "Impressão interrompida; reimpressão automática bloqueada por segurança.";
                    job.UpdatedAt = DateTimeOffset.UtcNow;
                    changed = true;
                }
                if (changed) SaveUnsafe();
                return _jobs.Values.Where(x => x.Status == "queued").Select(Clone).ToArray();
            }
        }

        public (LocalPrintJobRecord Job, bool Duplicate, bool Conflict) Enqueue(LocalPrintRequest request, byte[] bytes)
        {
            lock (_gate)
            {
                if (_jobs.TryGetValue(request.JobId, out var existing))
                {
                    bool conflict = existing.Copies != request.Copies ||
                        !string.Equals(existing.ImageSha256, request.Image.Sha256, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(existing.ImageVariant, request.ImageVariant, StringComparison.OrdinalIgnoreCase);
                    return (Clone(existing), true, conflict);
                }

                string safeId = new(request.JobId.Where(c => char.IsLetterOrDigit(c) || c is '-' or '_').ToArray());
                if (string.IsNullOrWhiteSpace(safeId)) safeId = Guid.NewGuid().ToString("N");
                string ext = request.Image.ContentType.ToLowerInvariant() switch { "image/png" => ".png", "image/bmp" => ".bmp", _ => ".jpg" };
                string file = safeId + ext;
                File.WriteAllBytes(Path.Combine(_imagesDirectory, file), bytes);

                var now = DateTimeOffset.UtcNow;
                var job = new LocalPrintJobRecord
                {
                    JobId = request.JobId,
                    Copies = request.Copies,
                    ImageVariant = request.ImageVariant,
                    ImageSha256 = request.Image.Sha256,
                    ImageFileName = file,
                    Status = "queued",
                    CreatedAt = now,
                    UpdatedAt = now,
                    Context = request.Context ?? new()
                };
                _jobs[job.JobId] = job;
                SaveUnsafe();
                return (Clone(job), false, false);
            }
        }

        public string GetImagePath(LocalPrintJobRecord job) => Path.Combine(_imagesDirectory, job.ImageFileName);

        public void Update(string jobId, string status, int copiesPrinted = 0, string? errorCode = null, string? errorMessage = null)
        {
            lock (_gate)
            {
                if (!_jobs.TryGetValue(jobId, out var job)) return;
                job.Status = status;
                job.CopiesPrinted = copiesPrinted;
                job.ErrorCode = errorCode;
                job.ErrorMessage = errorMessage;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                SaveUnsafe();
            }
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_statePath)) return;
                var list = JsonConvert.DeserializeObject<List<LocalPrintJobRecord>>(File.ReadAllText(_statePath)) ?? new();
                _jobs = list.Where(x => !string.IsNullOrWhiteSpace(x.JobId)).ToDictionary(x => x.JobId, StringComparer.OrdinalIgnoreCase);
            }
            catch { _jobs = new(StringComparer.OrdinalIgnoreCase); }
        }

        private void SaveUnsafe()
        {
            string tmp = _statePath + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(_jobs.Values.OrderBy(x => x.CreatedAt), Formatting.Indented));
            File.Move(tmp, _statePath, true);
        }

        private static LocalPrintJobRecord Clone(LocalPrintJobRecord value) =>
            JsonConvert.DeserializeObject<LocalPrintJobRecord>(JsonConvert.SerializeObject(value))!;
    }

    public sealed class LocalPrintCompanion : IAsyncDisposable
    {
        private readonly LocalPrintCompanionConfig _config;
        private readonly LocalPrintJobStore _store;
        private readonly Func<string, int, CancellationToken, Task> _printAsync;
        private readonly Action<string> _log;
        private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(new() { SingleReader = true });
        private readonly object _nonceGate = new();
        private readonly Dictionary<string, DateTimeOffset> _nonces = new(StringComparer.Ordinal);
        private CancellationTokenSource? _cts;
        private WebApplication? _app;
        private Task? _worker;

        public LocalPrintCompanion(LocalPrintCompanionConfig config, string dataRoot, Func<string, int, CancellationToken, Task> printAsync, Action<string> log)
        {
            _config = config;
            _store = new LocalPrintJobStore(dataRoot);
            _printAsync = printAsync;
            _log = log;
        }

        public async Task StartAsync()
        {
            if (!_config.Enabled || _app != null) return;
            string pfx = Path.Combine(AppPaths.DataDirectory, "localprint.pfx");
            string pwdPath = Path.Combine(AppPaths.DataDirectory, "localprint.pfx.password");
            if (!File.Exists(pfx))
            {
                _log("[LocalPrint] Desativado: certificado HTTPS local não configurado.");
                return;
            }

            string? pwd = File.Exists(pwdPath) ? File.ReadAllText(pwdPath).Trim() : null;
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(o =>
            {
                o.Limits.MaxRequestBodySize = _config.MaxRequestBytes;
                o.ListenAnyIP(_config.HttpsPort, l => l.UseHttps(pfx, pwd));
            });
            var app = builder.Build();

            app.Use(async (ctx, next) =>
            {
                string origin = ctx.Request.Headers.Origin.ToString();
                if (!_config.IsOriginAllowed(origin))
                {
                    ctx.Response.StatusCode = 403;
                    await ctx.Response.WriteAsJsonAsync(new { ok = false, error = "origin_not_allowed" });
                    return;
                }
                if (!string.IsNullOrWhiteSpace(origin))
                {
                    ctx.Response.Headers.AccessControlAllowOrigin = origin;
                    ctx.Response.Headers.Vary = "Origin";
                    ctx.Response.Headers.AccessControlAllowMethods = "GET, POST, OPTIONS";
                    ctx.Response.Headers.AccessControlAllowHeaders = "Content-Type";
                }
                if (HttpMethods.IsOptions(ctx.Request.Method)) { ctx.Response.StatusCode = 204; return; }
                await next();
            });

            app.MapGet("/ping", () => Results.Json(new
            {
                ok = true,
                app = "CBL.PrintAssistant",
                protocolVersions = new[] { 1 },
                profiles = new[] { "normal" },
                pairingRequired = true,
                pairingId = _config.PairingId
            }));

            app.MapGet("/job-status/{jobId}", (string jobId) =>
            {
                var job = _store.Get(jobId);
                return job == null
                    ? Results.NotFound(new { ok = false, error = "not_found" })
                    : Results.Json(new { ok = true, jobId = job.JobId, status = job.Status, copiesPrinted = job.CopiesPrinted, updatedAt = job.UpdatedAt, errorCode = job.ErrorCode });
            });

            app.MapPost("/print-job", async (HttpContext ctx) =>
            {
                LocalPrintRequest? request;
                try
                {
                    using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8);
                    request = JsonConvert.DeserializeObject<LocalPrintRequest>(await reader.ReadToEndAsync());
                }
                catch { return Results.BadRequest(new { ok = false, error = "invalid_payload" }); }

                string? validation = Validate(request);
                if (validation != null)
                    return Results.Json(new { ok = false, error = validation }, statusCode: validation is "unauthorized" or "replay" ? 401 : 400);

                byte[] bytes;
                try { bytes = Convert.FromBase64String(request!.Image.Data); }
                catch { return Results.BadRequest(new { ok = false, error = "invalid_image" }); }
                string hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
                if (!string.Equals(hash, request!.Image.Sha256, StringComparison.OrdinalIgnoreCase))
                    return Results.BadRequest(new { ok = false, error = "image_hash_mismatch" });

                var result = _store.Enqueue(request, bytes);
                if (result.Conflict) return Results.Conflict(new { ok = false, error = "job_conflict" });
                if (!result.Duplicate) _queue.Writer.TryWrite(result.Job.JobId);
                return Results.Json(new { ok = true, jobId = result.Job.JobId, status = result.Job.Status, duplicate = result.Duplicate });
            });

            _cts = new CancellationTokenSource();
            _app = app;
            _worker = Task.Run(() => WorkerAsync(_cts.Token));
            foreach (var job in _store.RecoverQueuedJobs()) _queue.Writer.TryWrite(job.JobId);
            await app.StartAsync(_cts.Token);
            _log($"[LocalPrint] HTTPS ativo na porta {_config.HttpsPort}; pairing {_config.PairingId}.");
        }

        private string? Validate(LocalPrintRequest? request)
        {
            if (request == null || request.ProtocolVersion != 1 || string.IsNullOrWhiteSpace(request.JobId)) return "invalid_payload";
            if (!string.Equals(request.Profile, "normal", StringComparison.OrdinalIgnoreCase)) return "unsupported_profile";
            if (request.Copies is < 1 or > 10) return "invalid_payload";
            if (request.Image == null || request.Image.Encoding != "base64" || string.IsNullOrWhiteSpace(request.Image.Data) || string.IsNullOrWhiteSpace(request.Image.Sha256)) return "invalid_payload";
            if (request.Auth == null || request.Auth.PairingId != _config.PairingId || string.IsNullOrWhiteSpace(request.Auth.Nonce) || string.IsNullOrWhiteSpace(request.Auth.Signature)) return "unauthorized";
            if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - request.Auth.Timestamp) > 300) return "unauthorized";

            string text = $"{request.JobId}\n{request.Copies}\n{request.ImageVariant}\n{request.Image.Sha256.ToLowerInvariant()}\n{request.Auth.Nonce}\n{request.Auth.Timestamp}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.PairingSecret));
            byte[] expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            byte[] received;
            try { received = Convert.FromHexString(request.Auth.Signature); } catch { return "unauthorized"; }
            if (received.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(received, expected)) return "unauthorized";

            lock (_nonceGate)
            {
                DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddMinutes(-10);
                foreach (string key in _nonces.Where(x => x.Value < cutoff).Select(x => x.Key).ToArray()) _nonces.Remove(key);
                if (_nonces.ContainsKey(request.Auth.Nonce)) return "replay";
                _nonces[request.Auth.Nonce] = DateTimeOffset.UtcNow;
            }
            return null;
        }

        private async Task WorkerAsync(CancellationToken token)
        {
            try
            {
                await foreach (string id in _queue.Reader.ReadAllAsync(token))
                {
                    var job = _store.Get(id);
                    if (job == null || job.Status != "queued") continue;
                    try
                    {
                        _store.Update(id, "printing");
                        await _printAsync(_store.GetImagePath(job), job.Copies, token);
                        _store.Update(id, "printed", job.Copies);
                        _log($"[LocalPrint] Job concluído: {id}");
                    }
                    catch (Exception ex)
                    {
                        _store.Update(id, "failed", 0, token.IsCancellationRequested ? "interrupted" : "print_failed", ex.Message);
                        _log($"[LocalPrint] Falha no job {id}: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        public async ValueTask DisposeAsync()
        {
            _cts?.Cancel();
            _queue.Writer.TryComplete();
            if (_app != null)
            {
                try { await _app.StopAsync(TimeSpan.FromSeconds(3)); } catch { }
                await _app.DisposeAsync();
            }
            _cts?.Dispose();
        }
    }
}
