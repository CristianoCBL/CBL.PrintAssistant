using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private LocalPrintCompanion? _localPrintCompanion;

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);
            await StartLocalPrintCompanionAsync();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try
            {
                if (_localPrintCompanion != null)
                    _localPrintCompanion.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            catch
            {
            }
            finally
            {
                _localPrintCompanion = null;
            }

            base.OnFormClosed(e);
        }

        private async Task StartLocalPrintCompanionAsync()
        {
            try
            {
                string configPath = Path.Combine(Application.StartupPath, "localprint.json");
                string dataPath = Path.Combine(Application.StartupPath, "localprint-data");
                LocalPrintCompanionConfig config = LocalPrintCompanionConfig.LoadOrCreate(configPath);

                if (!config.Enabled)
                {
                    AddLog("[LocalPrint] Companion offline desativado em localprint.json.");
                    return;
                }

                _localPrintCompanion = new LocalPrintCompanion(
                    config,
                    dataPath,
                    PrintLocalFileAsync,
                    AddLog);

                await _localPrintCompanion.StartAsync();
            }
            catch (Exception ex)
            {
                AddLog("[LocalPrint] Falha ao iniciar companion: " + ex.Message);
            }
        }

        private async Task PrintLocalFileAsync(string imagePath, int copies, CancellationToken cancellationToken)
        {
            PrintProfileConfig profile = await GetNormalProfileSnapshotAsync();

            if (string.IsNullOrWhiteSpace(profile.PrinterName))
                throw new InvalidOperationException("Perfil Normal sem impressora configurada.");
            if (string.IsNullOrWhiteSpace(profile.PaperName))
                throw new InvalidOperationException("Perfil Normal sem PaperSize configurado.");

            int safeCopies = Math.Clamp(copies, 1, 10);
            for (int i = 0; i < safeCopies; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                PrintImageFromFile(imagePath, profile, false);
            }
        }

        private Task<PrintProfileConfig> GetNormalProfileSnapshotAsync()
        {
            var tcs = new TaskCompletionSource<PrintProfileConfig>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Capture()
            {
                try
                {
                    AppConfig config = GetConfigFromForm();
                    PrintProfileConfig source = config.NormalProfile;
                    tcs.TrySetResult(new PrintProfileConfig
                    {
                        AgentId = source.AgentId,
                        AgentToken = source.AgentToken,
                        PrinterName = source.PrinterName,
                        PaperName = source.PaperName,
                        RotationMode = source.RotationMode,
                        Dpi = source.Dpi,
                        Bleed = source.Bleed,
                        OffsetX = source.OffsetX,
                        OffsetY = source.OffsetY
                    });
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }

            if (InvokeRequired)
                BeginInvoke((Action)Capture);
            else
                Capture();

            return tcs.Task;
        }
    }
}
