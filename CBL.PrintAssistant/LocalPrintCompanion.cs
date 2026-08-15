using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace CBL.PrintAssistant
{
    public sealed class LocalPrintRequest
    {
        [JsonProperty("protocolVersion")]
        public int ProtocolVersion { get; set; }

        [JsonProperty("jobId")]
        public string JobId { get; set; } = "";

        [JsonProperty("profile")]
        public string Profile { get; set; } = "normal";

        [JsonProperty("copies")]
        public int Copies { get; set; } = 1;

        [JsonProperty("imageVariant")]
        public string ImageVariant { get; set; } = "composed";

        [JsonProperty("image")]
        public LocalPrintImage Image { get; set; } = new LocalPrintImage();

        [JsonProperty("context")]
        public LocalPrintContext Context { get; set; } = new LocalPrintContext();

        [JsonProperty("createdAt")]
        public string CreatedAt { get; set; } = "";

        [JsonProperty("auth")]
        public LocalPrintAuth Auth { get; set; } = new LocalPrintAuth();
    }

    public sealed class LocalPrintImage
    {
        [JsonProperty("encoding")]
        public string Encoding { get; set; } = "base64";

        [JsonProperty("contentType")]
        public string ContentType { get; set; } = "image/jpeg";

        [JsonProperty("sha256")]
        public string Sha256 { get; set; } = "";

        [JsonProperty("data")]
        public string Data { get; set; } = "";
    }

    public sealed class LocalPrintContext
    {
        [JsonProperty("stationCode")]
        public string? StationCode { get; set; }

        [JsonProperty("stationId")]
        public string? StationId { get; set; }

        [JsonProperty("eventId")]
        public string? EventId { get; set; }

        [JsonProperty("creationKey")]
        public string? CreationKey { get; set; }

        [JsonProperty("captureId")]
        public string? CaptureId { get; set; }
    }

    public sealed class LocalPrintAuth
    {
        [JsonProperty("pairingId")]
        public string PairingId { get; set; } = "";

        [JsonProperty("nonce")]
        public string Nonce { get; set; } = "";

        [JsonProperty("ts")]
        public long Timestamp { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; } = "";
    }

    public sealed class LocalPrintJobRecord
    {
        [JsonProperty("jobId")]
        public string JobId { get; set; } = "";

        [JsonProperty("profile")]
        public string Profile { get; set; } = "normal";

        [JsonProperty("copies")]
        public int Copies { get; set; }

        [JsonProperty("imageVariant")]
        public string ImageVariant { get; set; } = "composed";

        [JsonProperty("imageSha256")]
        public string ImageSha256 { get; set; } = "";

        [JsonProperty("imageContentType")]
        public string ImageContentType { get; set; } = "image/jpeg";

        [JsonProperty("imageFileName")]
        public string ImageFileName { get; set; } = "";

        [JsonProperty("status")]
        public string Status { get; set; } = "queued";

        [JsonProperty("copiesPrinted")]
        public int CopiesPrinted { get; set; }

        [JsonProperty("createdAt")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTimeOffset UpdatedAt { get; set; }

        [JsonProperty("errorCode")]
        public string? ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonProperty("context")]
        public LocalPrintContext Context { get; set; } = new LocalPrintContext();
    }

    public sealed class LocalPrintJobStore
    {
        private readonly object _gate = new object();
        private readonly string _statePath;
        private readonly string _imagesDirectory;
        private Dictionary<string, LocalPrintJobRecord> _jobs = new Dictionary<string, LocalPrintJobRecord>(StringComparer.OrdinalIgnoreCase);

        public LocalPrintJobStore(string rootDirectory)
        {
            Directory.CreateDirectory(rootDirectory);
            _statePath = Path.Combine(rootDirectory, "jobs.json");
            _imagesDirectory = Path.Combine(rootDirectory, "images");
            Directory.CreateDirectory(_imagesDirectory);
            Load();
        }

        public IEnumerable<LocalPrintJobRecord> RecoverQueuedJobs()
        {
            lock (_gate)
            {
                bool changed = false;
                foreach (LocalPrintJobRecord job in _jobs.Values)
                {
                    if (job.Status == "printing")
                    {
                        job.Status = "failed";
                        job.ErrorCode = "interrupted";
                        job.ErrorMessage = "O Print Assistant foi interrompido durante a impressão. Reenvio automático bloqueado por segurança.";
                        job.UpdatedAt = DateTimeOffset.UtcNow;
                        changed = true;
                    }
                }

                if (changed)
                    SaveUnsafe();

                return _jobs.Values.Where(x => x.Status == "queued").Select(Clone).ToArray();
            }
        }

        public LocalPrintJobRecord? Get(string jobId)
        {
            lock (_gate)
                return _jobs.TryGetValue(jobId, out LocalPrintJobRecord? job) ? Clone(job) : null;
        }

        public (LocalPrintJobRecord job, bool duplicate, bool conflict) Enqueue(LocalPrintRequest request, byte[] imageBytes)
        {
            lock (_gate)
            {
                if (_jobs.TryGetValue(request.JobId, out LocalPrintJobRecord? existing))
                {
                    bool conflict =
                        existing.Copies != request.Copies ||
                        !string.Equals(existing.ImageSha256, request.Image.Sha256, StringComparison.OrdinalIgnoreCase) ||
                        !string.Equals(existing.ImageVariant, request.ImageVariant, StringComparison.OrdinalIgnoreCase);
                    return (Clone(existing), true, conflict);
                }

                string extension = request.Image.ContentType.ToLowerInvariant() switch
                {
                    "image/png" => ".png",
                    "image/bmp" => ".bmp",
                    _ => ".jpg"
                };
                string safeJobId = new string(request.JobId.Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());
                if (string.IsNullOrWhiteSpace(safeJobId))
                    safeJobId = Guid.NewGuid().ToString("N");
                string imageFileName = safeJobId + extension;
                string imagePath = Path.Combine(_imagesDirectory, imageFileName);
                File.WriteAllBytes(imagePath, imageBytes);

                DateTimeOffset now = DateTimeOffset.UtcNow;
                var job = new LocalPrintJobRecord
                {
                    JobId = request.JobId,
                    Profile = request.Profile,
                    Copies = request.Copies,
                    ImageVariant = request.ImageVariant,
                    ImageSha256 = request.Image.Sha256,
                    ImageContentType = request.Image.ContentType,
                    ImageFileName = imageFileName,
                    Status = "queued",
                    CreatedAt = now,
                    UpdatedAt = now,
                    Context = request.Context ?? new LocalPrintContext()
                };
                _jobs[job.JobId] = job;
                SaveUnsafe();
                return (Clone(job), false, false);
            }
        }

        public string GetImagePath(LocalPrintJobRecord job) => Path.Combine(_imagesDirectory, job.ImageFileName);

        public LocalPrintJobRecord Update(string jobId, string status, int copiesPrinted = 0, string? errorCode = null, string? errorMessage = null)
        {
            lock (_gate)
            {
                if (!_jobs.TryGetValue(jobId, out LocalPrintJobRecord? job))
                    throw new InvalidOperationException("Job local não encontrado.");

                job.Status = status;
                job.CopiesPrinted = copiesPrinted;
                job.ErrorCode = errorCode;
                job.ErrorMessage = errorMessage;
                job.UpdatedAt = DateTimeOffset.UtcNow;
                SaveUnsafe();
                return Clone(job);
            }
        }

        private void Load()
        {
            lock (_gate)
            {
                try
                {
                    if (!File.Exists(_statePath))
                        return;
                    string json = File.ReadAllText(_statePath);
                    var list = JsonConvert.DeserializeObject<List<LocalPrintJobRecord>>(json) ?? new List<LocalPrintJobRecord>();
                    _jobs = list.Where(x => !string.IsNullOrWhiteSpace(x.JobId))
                        .GroupBy(x => x.JobId, StringComparer.OrdinalIgnoreCase)
                        .ToDictionary(x => x.Key, x => x.Last(), StringComparer.OrdinalIgnoreCase);
                }
                catch
                {
                    _jobs = new Dictionary<string, LocalPrintJobRecord>(StringComparer.OrdinalIgnoreCase);
                }
            }
        }

        private void SaveUnsafe()
        {
            string tmp = _statePath + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(_jobs.Values.OrderBy(x => x.CreatedAt), Formatting.Indented));
            File.Move(tmp, _statePath, true);
        }

        private static LocalPrintJobRecord Clone(LocalPrintJobRecord job) =>
            JsonConvert.DeserializeObject<LocalPrintJobRecord>(JsonConvert.SerializeObject(job))!;
    }

    public sealed class LocalPrintCompanion : IAsyncDisposable
    {
        private readonly LocalPrintCompanionConfig _config;
        private readonly LocalPrintJobStore _store;
        private readonly Func<string, int, CancellationToken, Task> _printFileAsync;
        private readonly Action<string> _log;
        private readonly Channel<string> _queue = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        private readonly object _nonceGate = new object();
        private readonly Dictionary<string, DateTimeOffset> _nonces = new Dictionary<string, DateTimeOffset>(StringComparer.Ordinal);
        private CancellationTokenSource? _cts;
        private WebApplication? _app;
        private Task? _worker;

        public LocalPrintCompanion(LocalPrintCompanionConfig config, string dataDirectory, Func<string, int, CancellationToken, Task> printFileAsync, Action<string> log)
        {
            _config = config;
            _store = new LocalPrintJobStore(dataDirectory);
            _printFileAsync = printFileAsync;
            _log = log;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (!_config.Enabled || _app != null)
                return;

            string certPath = Path.Combine(AppContext.BaseDirectory, "localprint.pfx");
            if (!File.Exists(certPath))
            {
                _log("[LocalPrint] HTTPS não iniciado: localprint.pfx não encontrado. Gere/instale o certificado confiável antes do teste LAN.");
                return;
            }

            string passwordPath = Path.Combine(AppContext.BaseDirectory, "localprint.pfx.password");
            string? certPassword = File.Exists(passwordPath) ? File.ReadAllText(passwordPath).Trim() : null;

            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = _config.MaxRequestBytes;
                options.ListenAnyIP(_config.HttpsPort, listen => listen.UseHttps(certPath, certPassword));
            });
            builder.Services.ConfigureHttpJsonOptions(options => { });

            WebApplication app = builder.Build();
            MapRoutes(app);

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _app = app;
            _worker = Task.Run(() => WorkerLoopAsync(_cts.Token), _cts.Token);

            foreach (LocalPrintJobRecord queued in _store.RecoverQueuedJobs())
                _queue.Writer.TryWrite(queued.JobId);

            await app.StartAsync(_cts.Token);
            _log($"[LocalPrint] HTTPS ativo na porta {_config.HttpsPort}. Pairing ID: {_config.PairingId}");
        }

        private void MapRoutes(WebApplication app)
        {
            app.Use(async (ctx, next) =>
            {
                string origin = ctx.Request.Headers.Origin.ToString();
                if (!string.IsNullOrWhiteSpace(origin) && !_config.IsOriginAllowed(origin))
                {
                    ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
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

                if (HttpMethods.IsOptions(ctx.Request.Method))
                {
                    ctx.Response.StatusCode = StatusCodes.Status204NoContent;
                    return;
                }

                await next();
            });

            app.MapGet("/ping", () => Results.Json(new
            {
                ok = true,
                app = "CBL.PrintAssistant",
                protocolVersions = new[] { 1 },
                printerReady = IsNormalProfileReady(),
                profiles = new[] { "normal" },
                pairingRequired = true,
                pairingId = _config.PairingId
            }));

            app.MapGet("/job-status/{jobId}", (string jobId) =>
            {
                LocalPrintJobRecord? job = _store.Get(jobId);
                if (job == null)
                    return Results.NotFound(new { ok = false, error = "not_found" });
                return Results.Json(new
                {
                    ok = true,
                    jobId = job.JobId,
                    status = job.Status,
                    copiesPrinted = job.CopiesPrinted,
                    updatedAt = job.UpdatedAt,
                    errorCode = job.ErrorCode
                });
            });

            app.MapPost("/print-job", async (HttpContext ctx) =>
            {
                LocalPrintRequest? request;
                try
                {
                    using var reader = new StreamReader(ctx.Request.Body, Encoding.UTF8);
                    string body = await reader.ReadToEndAsync();
                    request = JsonConvert.DeserializeObject<LocalPrintRequest>(body);
                }
                catch
                {
                    return Results.BadRequest(new { ok = false, error = "invalid_payload" });
                }

                string? validation = ValidateRequest(request);
                if (validation != null)
                {
                    int status = validation == "unauthorized" || validation == "replay" ? 401 : 400;
                    return Results.Json(new { ok = false, error = validation }, statusCode: status);
                }

                byte[] imageBytes;
                try
                {
                    imageBytes = Convert.FromBase64String(request!.Image.Data);
                }
                catch
                {
                    return Results.BadRequest(new { ok = false, error = "invalid_image" });
                }

                string computedHash = Convert.ToHexString(SHA256.HashData(imageBytes)).ToLowerInvariant();
                if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(computedHash), Encoding.ASCII.GetBytes(request!.Image.Sha256.ToLowerInvariant())))
                    return Results.BadRequest(new { ok = false, error = "image_hash_mismatch" });

                var result = _store.Enqueue(request, imageBytes);
                if (result.conflict)
                    return Results.Conflict(new { ok = false, error = "job_conflict", jobId = result.job.JobId, status = result.job.Status });

                if (!result.duplicate && result.job.Status == "queued")
                    _queue.Writer.TryWrite(result.job.JobId);

                return Results.Json(new
                {
                    ok = true,
                    jobId = result.job.JobId,
                    status = result.job.Status,
                    duplicate = result.duplicate
                });
            });
        }

        private string? ValidateRequest(LocalPrintRequest? request)
        {
            if (request == null || request.ProtocolVersion != 1 || string.IsNullOrWhiteSpace(request.JobId))
                return "invalid_payload";
            if (!string.Equals(request.Profile, "normal", StringComparison.OrdinalIgnoreCase))
                return "unsupported_profile";
            if (request.Copies < 1 || request.Copies > 10)
                return "invalid_payload";
            if (request.Image == null || !string.Equals(request.Image.Encoding, "base64", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(request.Image.Data) || string.IsNullOrWhiteSpace(request.Image.Sha256))
                return "invalid_payload";
            if (request.Auth == null || !string.Equals(request.Auth.PairingId, _config.PairingId, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(request.Auth.Nonce) || string.IsNullOrWhiteSpace(request.Auth.Signature))
                return "unauthorized";

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (Math.Abs(now - request.Auth.Timestamp) > 300)
                return "unauthorized";

            string signingText = $"{request.JobId}\n{request.Copies}\n{request.ImageVariant}\n{request.Image.Sha256.ToLowerInvariant()}\n{request.Auth.Nonce}\n{request.Auth.Timestamp}";
            byte[] expected;
            try
            {
                using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config.PairingSecret));
                expected = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingText));
            }
            catch
            {
                return "unauthorized";
            }

            byte[] received;
            try
            {
                received = Convert.FromHexString(request.Auth.Signature);
            }
            catch
            {
                return "unauthorized";
            }

            if (received.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(received, expected))
                return "unauthorized";

            lock (_nonceGate)
            {
                DateTimeOffset cutoff = DateTimeOffset.UtcNow.AddMinutes(-10);
                foreach (string old in _nonces.Where(x => x.Value < cutoff).Select(x => x.Key).ToArray())
                    _nonces.Remove(old);
                if (_nonces.ContainsKey(request.Auth.Nonce))
                    return "replay";
                _nonces[request.Auth.Nonce] = DateTimeOffset.UtcNow;
            }

            return null;
        }

        private bool IsNormalProfileReady()
        {
            try
            {
                string appConfigPath = Path.Combine(AppContext.BaseDirectory, "appconfig.json");
                if (!File.Exists(appConfigPath))
                    return false;
                AppConfig? config = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(appConfigPath));
                return config != null &&
                    !string.IsNullOrWhiteSpace(config.NormalProfile.PrinterName) &&
                    !string.IsNullOrWhiteSpace(config.NormalProfile.PaperName);
            }
            catch
            {
                return false;
            }
        }

        private async Task WorkerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (string jobId in _queue.Reader.ReadAllAsync(cancellationToken))
                {
                    LocalPrintJobRecord? job = _store.Get(jobId);
                    if (job == null || job.Status != "queued")
                        continue;

                    try
                    {
                        _store.Update(jobId, "printing");
                        string imagePath = _store.GetImagePath(job);
                        await _printFileAsync(imagePath, job.Copies, cancellationToken);
                        _store.Update(jobId, "printed", job.Copies);
                        _log($"[LocalPrint] Job concluído: {jobId}");
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        try { _store.Update(jobId, "failed", 0, "interrupted", "Processamento interrompido."); } catch { }
                        break;
                    }
                    catch (Exception ex)
                    {
                        try { _store.Update(jobId, "failed", 0, "print_failed", ex.Message); } catch { }
                        _log($"[LocalPrint] Falha no job {jobId}: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_cts != null)
                _cts.Cancel();
            _queue.Writer.TryComplete();
            if (_app != null)
            {
                try { await _app.StopAsync(TimeSpan.FromSeconds(3)); } catch { }
                await _app.DisposeAsync();
            }
            _cts?.Dispose();
            _app = null;
            _cts = null;
        }
    }
}
