namespace CBL.PrintAssistant
{
    public sealed class JobPrintCoordinator
    {
        private readonly ProfessionalPrintEngine _engine;

        public JobPrintCoordinator(ProfessionalPrintEngine? engine = null)
        {
            _engine = engine ?? new ProfessionalPrintEngine();
        }

        public async Task<int> PrintAsync(
            string profileName,
            AppConfig appConfig,
            PrintProfileConfig localProfile,
            PrintJobDto job,
            Action<string>? log,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(appConfig);
            ArgumentNullException.ThrowIfNull(localProfile);
            ArgumentNullException.ThrowIfNull(job);

            if (string.IsNullOrWhiteSpace(job.ImageUrl))
                throw new InvalidOperationException("O job não trouxe image_url.");

            EffectivePrintSettings settings = PrintSettingsResolver.Resolve(
                appConfig,
                localProfile,
                job);

            bool stripMode = profileName.Equals(
                "Tirinha",
                StringComparison.OrdinalIgnoreCase);

            LogEffectiveSettings(profileName, settings, stripMode, log);

            for (int copy = 1; copy <= settings.Copies; copy++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                log?.Invoke(
                    $"[{profileName}] Imprimindo cópia {copy}/{settings.Copies}" +
                    (settings.IsTest ? " (teste)" : "") + ".");

                await _engine.PrintFromUrlAsync(
                    job.ImageUrl,
                    settings,
                    stripMode,
                    cancellationToken);
            }

            return settings.Copies;
        }

        private static void LogEffectiveSettings(
            string profileName,
            EffectivePrintSettings settings,
            bool stripMode,
            Action<string>? log)
        {
            if (log is null)
                return;

            string jobKind = settings.IsTest ? "teste/calibração" : "produção";
            string layout = stripMode ? "tirinha dupla" : "foto normal";

            log(
                $"[{profileName}] Job {jobKind}; contrato v{settings.ContractVersion}; " +
                $"layout={layout}; cópias={settings.Copies}; " +
                $"impressora=\"{settings.PrinterName}\"; papel=\"{settings.PaperName}\"; " +
                $"orientação={settings.Orientation}; rotação={settings.RotationMode}; " +
                $"ajuste={settings.FitMode}; DPI={settings.Dpi}; sangria={settings.Bleed}; " +
                $"margens={settings.MarginLeft}/{settings.MarginTop}/" +
                $"{settings.MarginRight}/{settings.MarginBottom}; " +
                $"offset={settings.OffsetX}/{settings.OffsetY}.");
        }
    }
}
