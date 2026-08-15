using System.Security.Cryptography;
using Newtonsoft.Json;

namespace CBL.PrintAssistant
{
    public sealed class LocalPrintCompanionConfig
    {
        public bool Enabled { get; set; }
        public int HttpsPort { get; set; } = 38452;
        public string PairingId { get; set; } = "";
        public string PairingSecret { get; set; } = "";
        public int MaxRequestBytes { get; set; } = 25 * 1024 * 1024;
        public List<string> AllowedOrigins { get; set; } = new()
        {
            "https://kbine.cblconnect.app",
            "https://event-snap-capture.lovable.app",
            "https://id-preview--b6b8f8eb-9e4f-4dee-9889-c887b9f5e482.lovable.app"
        };

        public static LocalPrintCompanionConfig LoadOrCreate(string path)
        {
            LocalPrintCompanionConfig config;
            try
            {
                config = File.Exists(path)
                    ? JsonConvert.DeserializeObject<LocalPrintCompanionConfig>(File.ReadAllText(path)) ?? new()
                    : new();
            }
            catch
            {
                config = new();
            }

            config.EnsureDefaults();
            config.Save(path);
            return config;
        }

        public void EnsureDefaults()
        {
            if (HttpsPort is < 1024 or > 65535) HttpsPort = 38452;
            if (MaxRequestBytes is < 1048576 or > 104857600) MaxRequestBytes = 25 * 1024 * 1024;
            if (string.IsNullOrWhiteSpace(PairingId))
                PairingId = $"{Environment.MachineName.ToLowerInvariant()}-{Guid.NewGuid():N}"[..Math.Min(Environment.MachineName.Length + 13, Environment.MachineName.Length + 13)];
            if (string.IsNullOrWhiteSpace(PairingSecret))
                PairingSecret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            AllowedOrigins ??= new();
            if (AllowedOrigins.Count == 0) AllowedOrigins.Add("https://kbine.cblconnect.app");
        }

        public bool IsOriginAllowed(string? origin)
        {
            if (string.IsNullOrWhiteSpace(origin)) return true;
            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? candidate)) return false;
            return AllowedOrigins.Any(value =>
                Uri.TryCreate(value, UriKind.Absolute, out Uri? allowed) &&
                string.Equals(candidate.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Host, allowed.Host, StringComparison.OrdinalIgnoreCase) &&
                candidate.Port == allowed.Port);
        }

        public void Save(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            string temp = path + ".tmp";
            File.WriteAllText(temp, JsonConvert.SerializeObject(this, Formatting.Indented));
            File.Move(temp, path, true);
        }
    }
}
