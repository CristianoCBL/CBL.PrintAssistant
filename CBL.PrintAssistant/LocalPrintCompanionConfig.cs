using System;
using System.Collections.Generic;
using System.IO;
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
        public List<string> AllowedOrigins { get; set; } = new List<string>
        {
            "https://kbine.cblconnect.app",
            "https://event-snap-capture.lovable.app",
            "https://id-preview--b6b8f8eb-9e4f-4dee-9889-c887b9f5e482.lovable.app"
        };

        public static LocalPrintCompanionConfig LoadOrCreate(string path)
        {
            LocalPrintCompanionConfig? config = null;

            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    config = JsonConvert.DeserializeObject<LocalPrintCompanionConfig>(json);
                }
            }
            catch
            {
                config = null;
            }

            config ??= new LocalPrintCompanionConfig();
            config.EnsureDefaults();
            config.Save(path);
            return config;
        }

        public void EnsureDefaults()
        {
            if (HttpsPort < 1024 || HttpsPort > 65535)
                HttpsPort = 38452;

            if (MaxRequestBytes < 1024 * 1024 || MaxRequestBytes > 100 * 1024 * 1024)
                MaxRequestBytes = 25 * 1024 * 1024;

            if (string.IsNullOrWhiteSpace(PairingId))
            {
                string machine = Environment.MachineName
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(" ", "-");
                string suffix = Guid.NewGuid().ToString("N").Substring(0, 12);
                PairingId = string.IsNullOrWhiteSpace(machine) ? suffix : $"{machine}-{suffix}";
            }

            if (string.IsNullOrWhiteSpace(PairingSecret))
                PairingSecret = CreateBase64UrlSecret(32);

            AllowedOrigins ??= new List<string>();
            if (AllowedOrigins.Count == 0)
                AllowedOrigins.Add("https://kbine.cblconnect.app");
        }

        public bool IsOriginAllowed(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
                return true;

            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? candidate))
                return false;

            foreach (string configured in AllowedOrigins)
            {
                if (string.IsNullOrWhiteSpace(configured))
                    continue;

                if (Uri.TryCreate(configured, UriKind.Absolute, out Uri? allowed) &&
                    string.Equals(candidate.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(candidate.Host, allowed.Host, StringComparison.OrdinalIgnoreCase) &&
                    candidate.Port == allowed.Port)
                {
                    return true;
                }
            }

            return false;
        }

        public void Save(string path)
        {
            EnsureDefaults();
            string directory = Path.GetDirectoryName(path) ?? ".";
            Directory.CreateDirectory(directory);

            string tmp = path + ".tmp";
            File.WriteAllText(tmp, JsonConvert.SerializeObject(this, Formatting.Indented));
            File.Move(tmp, path, true);
        }

        private static string CreateBase64UrlSecret(int byteCount)
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(byteCount);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
