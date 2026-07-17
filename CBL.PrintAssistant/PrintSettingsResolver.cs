namespace CBL.PrintAssistant
{
    public static class PrintSettingsResolver
    {
        public const int CurrentContractVersion = 2;

        public static EffectivePrintSettings Resolve(
            AppConfig appConfig,
            PrintProfileConfig localProfile,
            PrintJobDto job)
        {
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(localProfile);
            ArgumentNullException.ThrowIfNull(job);

            int contractVersion = job.ContractVersion.GetValueOrDefault(1);
            if (contractVersion < 1 || contractVersion > CurrentContractVersion)
                throw new InvalidOperationException(
                    $"Versão de contrato de impressão não suportada: {contractVersion}. " +
                    $"Versão máxima suportada: {CurrentContractVersion}.");

            bool allowOverrides = appConfig.AllowJobOverrides;

            string printerName = FirstNonEmpty(
                allowOverrides ? job.WindowsPrinterName : null,
                allowOverrides ? job.PrinterName : null,
                localProfile.PrinterName);

            string paperName = FirstNonEmpty(
                allowOverrides ? job.PaperSize : null,
                localProfile.PaperName);

            string orientation = FirstNonEmpty(
                allowOverrides ? job.Orientation : null,
                localProfile.Orientation,
                "Automático");

            string rotation = FirstNonEmpty(
                allowOverrides ? job.Rotation : null,
                localProfile.RotationMode,
                "Automático");

            string fit = FirstNonEmpty(
                allowOverrides ? job.Fit : null,
                localProfile.FitMode,
                "Cover");

            int copies = Math.Clamp(job.Copies <= 0 ? 1 : job.Copies, 1, 5);

            return new EffectivePrintSettings
            {
                PrinterName = printerName,
                PaperName = paperName,
                Orientation = NormalizeOrientation(orientation),
                RotationMode = NormalizeRotation(rotation),
                FitMode = NormalizeFit(fit),
                Dpi = Clamp(allowOverrides ? job.Dpi : null, localProfile.Dpi, 72, 1200),
                Bleed = Clamp(allowOverrides ? job.Bleed : null, localProfile.Bleed, -500, 500),
                MarginLeft = Clamp(allowOverrides ? job.MarginLeft : null, localProfile.MarginLeft, 0, 1000),
                MarginTop = Clamp(allowOverrides ? job.MarginTop : null, localProfile.MarginTop, 0, 1000),
                MarginRight = Clamp(allowOverrides ? job.MarginRight : null, localProfile.MarginRight, 0, 1000),
                MarginBottom = Clamp(allowOverrides ? job.MarginBottom : null, localProfile.MarginBottom, 0, 1000),
                OffsetX = Clamp(allowOverrides ? job.OffsetX : null, localProfile.OffsetX, -1000, 1000),
                OffsetY = Clamp(allowOverrides ? job.OffsetY : null, localProfile.OffsetY, -1000, 1000),
                Copies = copies,
                IsTest = job.IsTest == true,
                ContractVersion = contractVersion
            };
        }

        private static int Clamp(int? overrideValue, int fallback, int min, int max)
        {
            int value = overrideValue ?? fallback;
            return Math.Clamp(value, min, max);
        }

        private static string FirstNonEmpty(params string?[] values)
        {
            foreach (string? value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static string NormalizeOrientation(string value)
        {
            if (value.Equals("portrait", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("retrato", StringComparison.OrdinalIgnoreCase))
                return "Retrato";

            if (value.Equals("landscape", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("paisagem", StringComparison.OrdinalIgnoreCase))
                return "Paisagem";

            return "Automático";
        }

        private static string NormalizeRotation(string value)
        {
            string normalized = value.Trim().Replace("graus", "", StringComparison.OrdinalIgnoreCase).Trim();
            return normalized switch
            {
                "0" or "0°" => "0°",
                "90" or "90°" => "90°",
                "180" or "180°" => "180°",
                "270" or "270°" => "270°",
                _ => "Automático"
            };
        }

        private static string NormalizeFit(string value)
        {
            if (value.Equals("contain", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("conter", StringComparison.OrdinalIgnoreCase))
                return "Contain";

            if (value.Equals("stretch", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("esticar", StringComparison.OrdinalIgnoreCase))
                return "Stretch";

            return "Cover";
        }
    }
}
