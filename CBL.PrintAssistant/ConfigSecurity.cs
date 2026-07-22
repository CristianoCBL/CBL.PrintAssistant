using System.Security.Cryptography;
using System.Text;

namespace CBL.PrintAssistant
{
    public static class ConfigSecurity
    {
        private const string Prefix = "dpapi:";
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("CBL.PrintAssistant.AgentToken.v1");

        public static AppConfig CreateStorageCopy(AppConfig source)
        {
            ArgumentNullException.ThrowIfNull(source);

            return new AppConfig
            {
                ApiBaseUrl = source.ApiBaseUrl,
                UnitId = source.UnitId,
                KioskId = source.KioskId,
                StartWithWindows = source.StartWithWindows,
                AutoStartListening = source.AutoStartListening,
                EnableLocalIntegration = source.EnableLocalIntegration,
                AllowJobOverrides = source.AllowJobOverrides,
                RunMode = source.RunMode,
                NormalProfile = CreateStorageProfile(source.NormalProfile),
                StripProfile = CreateStorageProfile(source.StripProfile)
            };
        }

        public static bool UnprotectInPlace(AppConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            bool migrated = false;
            migrated |= UnprotectProfile(config.NormalProfile);
            migrated |= UnprotectProfile(config.StripProfile);
            return migrated;
        }

        private static PrintProfileConfig CreateStorageProfile(PrintProfileConfig source)
        {
            source ??= new PrintProfileConfig();

            return new PrintProfileConfig
            {
                AgentId = source.AgentId,
                AgentToken = Protect(source.AgentToken),
                PrinterName = source.PrinterName,
                PaperName = source.PaperName,
                Orientation = source.Orientation,
                RotationMode = source.RotationMode,
                FitMode = source.FitMode,
                Dpi = source.Dpi,
                Bleed = source.Bleed,
                MarginLeft = source.MarginLeft,
                MarginTop = source.MarginTop,
                MarginRight = source.MarginRight,
                MarginBottom = source.MarginBottom,
                OffsetX = source.OffsetX,
                OffsetY = source.OffsetY
            };
        }

        private static bool UnprotectProfile(PrintProfileConfig profile)
        {
            if (profile is null || string.IsNullOrWhiteSpace(profile.AgentToken))
                return false;

            if (!profile.AgentToken.StartsWith(Prefix, StringComparison.Ordinal))
                return true;

            profile.AgentToken = Unprotect(profile.AgentToken);
            return false;
        }

        private static string Protect(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.StartsWith(Prefix, StringComparison.Ordinal))
                return value ?? string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(value);
            byte[] protectedBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(protectedBytes);
        }

        private static string Unprotect(string value)
        {
            try
            {
                byte[] protectedBytes = Convert.FromBase64String(value[Prefix.Length..]);
                byte[] plainBytes = ProtectedData.Unprotect(protectedBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception ex) when (ex is FormatException or CryptographicException)
            {
                throw new InvalidOperationException(
                    "Não foi possível descriptografar o Agent Token deste usuário do Windows.",
                    ex);
            }
        }
    }
}
