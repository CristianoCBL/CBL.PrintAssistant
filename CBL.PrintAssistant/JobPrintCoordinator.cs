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

            bool