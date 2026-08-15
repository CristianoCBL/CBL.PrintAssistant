using System.Drawing.Printing;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private LocalPrintCompanion? _localPrintCompanion;
        private bool _localPrintShutdownHooked;

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (!_localPrintShutdownHooked)
            {
                _localPrintShutdownHooked = true;
                FormClosed += async (_, _) =>
                {
                    if (_localPrintCompanion != null)
                    {
                        await _localPrintCompanion.DisposeAsync();
                        _localPrintCompanion = null;
                    }
                };
            }

            await StartLocalPrintCompanionAsync();
        }

        private async Task StartLocalPrintCompanionAsync()
        {
            try
            {
                string configPath = Path.Combine(AppPaths.DataDirectory, "localprint.json");
                string queuePath = Path.Combine(AppPaths.DataDirectory, "localprint-data");
                LocalPrintCompanionConfig config = LocalPrintCompanionConfig.LoadOrCreate(configPath);

                if (!config.Enabled)
                {
                    AddLog("[LocalPrint] Impressão offline local desativada. Configure localprint.json para habilitar.");
                    return;
                }

                _localPrintCompanion = new LocalPrintCompanion(config, queuePath, PrintLocalFileAsync, AddLog);
                await _localPrintCompanion.StartAsync();
            }
            catch (Exception ex)
            {
                AddLog("[LocalPrint] Falha ao iniciar: " + ex.Message);
            }
        }

        private async Task PrintLocalFileAsync(string imagePath, int copies, CancellationToken token)
        {
            PrintProfileConfig profile = await ReadNormalProfileAsync();
            var settings = new EffectivePrintSettings
            {
                PrinterName = profile.PrinterName,
                PaperName = profile.PaperName,
                FallbackPaperName = profile.PaperName,
                Orientation = profile.Orientation,
                RotationMode = profile.RotationMode,
                FitMode = profile.FitMode,
                Dpi = profile.Dpi,
                Bleed = profile.Bleed,
                MarginLeft = profile.MarginLeft,
                MarginTop = profile.MarginTop,
                MarginRight = profile.MarginRight,
                MarginBottom = profile.MarginBottom,
                OffsetX = profile.OffsetX,
                OffsetY = profile.OffsetY,
                Copies = Math.Clamp(copies, 1, 10),
                IsTest = false,
                ContractVersion = 1
            };

            if (string.IsNullOrWhiteSpace(settings.PrinterName))
                throw new InvalidOperationException("Perfil Normal sem impressora configurada.");
            if (string.IsNullOrWhiteSpace(settings.PaperName))
                throw new InvalidOperationException("Perfil Normal sem papel configurado.");

            var probe = new PrinterSettings { PrinterName = settings.PrinterName };
            if (!probe.IsValid)
                throw new InvalidOperationException("A impressora do perfil Normal está indisponível no Windows.");

            for (int copy = 1; copy <= settings.Copies; copy++)
            {
                token.ThrowIfCancellationRequested();
                AddLog($"[LocalPrint] Imprimindo cópia {copy}/{settings.Copies} pelo perfil Normal.");
                ProfessionalPrintEngine.PrintFile(imagePath, settings, stripMode: false);
            }
        }

        private Task<PrintProfileConfig> ReadNormalProfileAsync()
        {
            var tcs = new TaskCompletionSource<PrintProfileConfig>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Capture()
            {
                try
                {
                    PrintProfileConfig source = GetConfigFromForm().NormalProfile;
                    tcs.TrySetResult(new PrintProfileConfig
                    {
                        AgentId = source.AgentId,
                        AgentToken = source.AgentToken,
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
                    });
                }
                catch (Exception ex) { tcs.TrySetException(ex); }
            }

            if (InvokeRequired) BeginInvoke((Action)Capture); else Capture();
            return tcs.Task;
        }
    }
}
