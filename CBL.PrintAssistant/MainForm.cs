using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CBL.PrintAssistant
{
    public partial class MainForm : Form
    {
        private readonly string _configPath;
        private AppConfig? _currentConfig;
        private readonly PrintAgentService _printAgentService = new PrintAgentService();
        private readonly UpdateService _updateService = new UpdateService();

        private CancellationTokenSource? _listenerCts;
        private bool _isListening;
        private bool _allowClose;

        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;

        private const string StartupRegistryName = "CBL.PrintAssistant";
        private const string GitHubOwner = "CristianoCBL";
        private const string GitHubRepo = "CBL.PrintAssistant";

        public MainForm()
        {
            InitializeComponent();

            _configPath = Path.Combine(Application.StartupPath, "appconfig.json");

            ConfigureRotationCombo();
            UpdateStatusUi(false);
            UpdateStaticInfo();
            InitializeTray();

            AddLog("Aplicativo iniciado.");
            AddLog("Agent ID local: " + Environment.MachineName);
            AddLog("Versão atual: " + GetCurrentVersion());

            LoadInstalledPrinters();
            LoadConfig();

            Shown += MainForm_Shown;
            Resize += MainForm_Resize;
            FormClosing += MainForm_FormClosing;
        }

        private void ConfigureRotationCombo()
        {
            cmbRotation.Items.Clear();
            cmbRotation.Items.Add("Automático");
            cmbRotation.Items.Add("0°");
            cmbRotation.Items.Add("90°");
            cmbRotation.Items.Add("180°");
            cmbRotation.Items.Add("270°");
            cmbRotation.SelectedIndex = 0;
        }

        private void UpdateStaticInfo()
        {
            lblAgentIdValue.Text = Environment.MachineName;
            lblSystemPrinterValue.Text = "-";
            lblLastHeartbeatValue.Text = "-";
            lblLastJobValue.Text = "-";
        }

        private void UpdateStatusUi(bool active)
        {
            _isListening = active;
            lblStatusDot.ForeColor = active ? Color.ForestGreen : Color.Firebrick;
            lblStatusText.Text = active ? "Ativo" : "Inativo";
            btnStartListener.Text = active ? "Parar Escuta" : "Iniciar Escuta";

            if (_trayIcon != null)
            {
                _trayIcon.Text = active
                    ? "CBL Print Assistant - Ativo"
                    : "CBL Print Assistant - Inativo";
            }
        }

        private void UpdateHeartbeatUi()
        {
            lblLastHeartbeatValue.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void UpdateLastJobUi(string value)
        {
            lblLastJobValue.Text = value;
        }

        private void InitializeTray()
        {
            _trayMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Abrir");
            openItem.Click += (s, e) => ShowFromTray();

            var toggleListenItem = new ToolStripMenuItem("Iniciar/Parar Escuta");
            toggleListenItem.Click += async (s, e) => await ToggleListenerFromTrayAsync();

            var checkUpdatesItem = new ToolStripMenuItem("Verificar Atualização");
            checkUpdatesItem.Click += async (s, e) => await CheckForUpdatesAsync(true);

            var exitItem = new ToolStripMenuItem("Sair");
            exitItem.Click += (s, e) => ExitApplication();

            _trayMenu.Items.Add(openItem);
            _trayMenu.Items.Add(toggleListenItem);
            _trayMenu.Items.Add(checkUpdatesItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(exitItem);

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "CBL Print Assistant - Inativo",
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private async void MainForm_Shown(object? sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            foreach (string arg in args)
            {
                if (string.Equals(arg, "--tray", StringComparison.OrdinalIgnoreCase))
                {
                    HideToTray();
                    break;
                }
            }

            await CheckForUpdatesAsync(false);
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                HideToTray();
            }
        }

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowClose)
                return;

            e.Cancel = true;
            HideToTray();
        }

        private void HideToTray()
        {
            Hide();
            ShowInTaskbar = false;

            if (_trayIcon != null)
            {
                _trayIcon.Visible = true;
            }
        }

        private void ShowFromTray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private async Task ToggleListenerFromTrayAsync()
        {
            if (_isListening)
            {
                StopListener();
                return;
            }

            await StartListenerAsync();
        }

        private void ExitApplication()
        {
            try
            {
                _allowClose = true;
                _trayIcon?.Dispose();
                _trayIcon = null;
                _trayMenu?.Dispose();
                _trayMenu = null;

                _listenerCts?.Cancel();
                _listenerCts?.Dispose();
                _listenerCts = null;
            }
            catch
            {
            }

            Application.Exit();
        }

        private void ApplyStartupSetting(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                writable: true
            );

            if (key == null)
                throw new Exception("Não foi possível acessar o registro do Windows.");

            if (enabled)
            {
                string exePath = Application.ExecutablePath;
                string value = $"\"{exePath}\" --tray";
                key.SetValue(StartupRegistryName, value);
                AddLog("Inicialização com Windows ativada.");
            }
            else
            {
                if (key.GetValue(StartupRegistryName) != null)
                    key.DeleteValue(StartupRegistryName, false);

                AddLog("Inicialização com Windows desativada.");
            }
        }

        private string GetCurrentVersion()
        {
            var infoVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion;

            if (!string.IsNullOrWhiteSpace(infoVersion))
                return infoVersion;

            return Application.ProductVersion;
        }

        private Version GetCurrentVersionParsed()
        {
            string versionText = GetCurrentVersion();

            int plusIndex = versionText.IndexOf('+');
            if (plusIndex >= 0)
                versionText = versionText.Substring(0, plusIndex);

            if (Version.TryParse(versionText, out var version))
                return version;

            return new Version(1, 0, 0);
        }

        private async Task CheckForUpdatesAsync(bool manual)
        {
            try
            {
                AddLog("Verificando atualizações...");

                var result = await _updateService.CheckForUpdateAsync(
                    GitHubOwner,
                    GitHubRepo,
                    GetCurrentVersionParsed()
                );

                if (!result.HasUpdate)
                {
                    AddLog("Nenhuma atualização encontrada.");

                    if (manual)
                    {
                        MessageBox.Show(
                            "Você já está na versão mais recente.",
                            "Atualização",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }

                    return;
                }

                AddLog($"Nova versão encontrada: {result.LatestVersion}");

                var dialogResult = MessageBox.Show(
                    $"Nova versão disponível: {result.LatestVersion}\n\nDeseja baixar e atualizar agora?",
                    "Atualização disponível",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );

                if (dialogResult != DialogResult.Yes)
                    return;

                AddLog("Baixando atualização...");

                string scriptPath = await _updateService.DownloadAndPrepareUpdateAsync(
                    result.DownloadUrl ?? "",
                    Application.StartupPath,
                    Path.GetFileName(Application.ExecutablePath),
                    result.LatestVersion,
                    Environment.ProcessId
                );

                AddLog("Atualização preparada. Reiniciando aplicativo...");

                _updateService.LaunchUpdateScript(scriptPath);

                _allowClose = true;
                Application.Exit();
            }
            catch (Exception ex)
            {
                AddLog("Erro ao verificar atualização: " + ex.Message);

                if (manual)
                {
                    MessageBox.Show(
                        "Erro ao verificar atualização:\n" + ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            _ = CheckForUpdatesAsync(true);
        }

        private void btnLoadPrinters_Click(object sender, EventArgs e)
        {
            LoadInstalledPrinters();
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                cmbPrinters.Items.Clear();

                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    cmbPrinters.Items.Add(printerName);
                }

                AddLog($"Impressoras carregadas: {cmbPrinters.Items.Count}");

                if (cmbPrinters.Items.Count == 0)
                {
                    AddLog("Nenhuma impressora encontrada no Windows.");
                    return;
                }

                int askIndex = -1;

                for (int i = 0; i < cmbPrinters.Items.Count; i++)
                {
                    var itemText = cmbPrinters.Items[i]?.ToString() ?? "";

                    if (itemText.Contains("ASK-300", StringComparison.OrdinalIgnoreCase) ||
                        itemText.Contains("FUJIFILM", StringComparison.OrdinalIgnoreCase))
                    {
                        askIndex = i;
                        break;
                    }
                }

                if (askIndex >= 0)
                {
                    cmbPrinters.SelectedIndex = askIndex;
                    AddLog($"Impressora Fujifilm encontrada: {cmbPrinters.SelectedItem}");
                }
                else
                {
                    cmbPrinters.SelectedIndex = 0;
                    AddLog($"Impressora padrão carregada: {cmbPrinters.SelectedItem}");
                }

                TryRestoreSavedPrinter();
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar impressoras: " + ex.Message);
                MessageBox.Show(
                    "Erro ao carregar impressoras:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private AppConfig GetConfigFromForm()
        {
            return new AppConfig
            {
                ApiBaseUrl = txtSupabaseUrl.Text.Trim(),
                AgentId = Environment.MachineName,
                AgentToken = txtSupabaseKey.Text.Trim(),
                UnitId = txtUnitId.Text.Trim(),
                KioskId = txtKioskId.Text.Trim(),
                PrinterName = cmbPrinters.SelectedItem?.ToString() ?? "",
                StartWithWindows = chkStartWithWindows.Checked,
                RotationMode = cmbRotation.SelectedItem?.ToString() ?? "Automático",
                Bleed = (int)nudBleed.Value,
                OffsetX = (int)nudOffsetX.Value,
                OffsetY = (int)nudOffsetY.Value
            };
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var config = GetConfigFromForm();

                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configPath, json);

                _currentConfig = config;

                ApplyStartupSetting(config.StartWithWindows);

                AddLog("Configuração salva com sucesso.");
                MessageBox.Show(
                    "Configuração salva com sucesso.",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                AddLog("Erro ao salvar configuração: " + ex.Message);
                MessageBox.Show(
                    "Erro ao salvar configuração:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    AddLog("Nenhuma configuração salva encontrada.");
                    return;
                }

                string json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(json);

                if (config == null)
                {
                    AddLog("Arquivo de configuração vazio ou inválido.");
                    return;
                }

                _currentConfig = config;

                txtSupabaseUrl.Text = config.ApiBaseUrl;
                txtSupabaseKey.Text = config.AgentToken;
                txtUnitId.Text = config.UnitId;
                txtKioskId.Text = config.KioskId;
                chkStartWithWindows.Checked = config.StartWithWindows;

                if (!string.IsNullOrWhiteSpace(config.RotationMode) && cmbRotation.Items.Contains(config.RotationMode))
                    cmbRotation.SelectedItem = config.RotationMode;
                else
                    cmbRotation.SelectedIndex = 0;

                nudBleed.Value = Math.Max(nudBleed.Minimum, Math.Min(nudBleed.Maximum, config.Bleed));
                nudOffsetX.Value = Math.Max(nudOffsetX.Minimum, Math.Min(nudOffsetX.Maximum, config.OffsetX));
                nudOffsetY.Value = Math.Max(nudOffsetY.Minimum, Math.Min(nudOffsetY.Maximum, config.OffsetY));

                AddLog("Configuração carregada.");
                TryRestoreSavedPrinter();
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar configuração: " + ex.Message);
            }
        }

        private void TryRestoreSavedPrinter()
        {
            try
            {
                if (_currentConfig == null)
                    return;

                if (string.IsNullOrWhiteSpace(_currentConfig.PrinterName))
                    return;

                for (int i = 0; i < cmbPrinters.Items.Count; i++)
                {
                    var itemText = cmbPrinters.Items[i]?.ToString() ?? "";

                    if (string.Equals(itemText, _currentConfig.PrinterName, StringComparison.OrdinalIgnoreCase))
                    {
                        cmbPrinters.SelectedIndex = i;
                        AddLog($"Impressora restaurada da configuração: {itemText}");
                        return;
                    }
                }

                AddLog($"Impressora salva não encontrada neste computador: {_currentConfig.PrinterName}");
            }
            catch (Exception ex)
            {
                AddLog("Erro ao restaurar impressora salva: " + ex.Message);
            }
        }

        private string GetSelectedPrinterName()
        {
            return cmbPrinters.SelectedItem?.ToString() ?? "";
        }

        private void btnTestPrint_Click(object sender, EventArgs e)
        {
            try
            {
                string printerName = GetSelectedPrinterName();

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    MessageBox.Show(
                        "Selecione uma impressora antes de testar.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                PrintDocument printDocument = new PrintDocument();
                printDocument.PrinterSettings.PrinterName = printerName;

                if (!printDocument.PrinterSettings.IsValid)
                {
                    MessageBox.Show(
                        "A impressora selecionada não é válida no Windows.",
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                printDocument.PrintPage += (s, ev) =>
                {
                    using Font titleFont = new Font("Arial", 18, FontStyle.Bold);
                    using Font bodyFont = new Font("Arial", 11, FontStyle.Regular);

                    float y = 40;

                    ev.Graphics.DrawString("CBL Print Assistant", titleFont, Brushes.Black, 40, y);
                    y += 50;

                    ev.Graphics.DrawString("Teste de impressão executado com sucesso.", bodyFont, Brushes.Black, 40, y);
                    y += 30;

                    ev.Graphics.DrawString($"Impressora: {printerName}", bodyFont, Brushes.Black, 40, y);
                    y += 30;

                    ev.Graphics.DrawString($"Data/Hora: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", bodyFont, Brushes.Black, 40, y);
                    y += 30;

                    ev.Graphics.DrawString($"Unit ID: {txtUnitId.Text}", bodyFont, Brushes.Black, 40, y);
                    y += 30;

                    ev.Graphics.DrawString($"Kiosk ID: {txtKioskId.Text}", bodyFont, Brushes.Black, 40, y);

                    ev.HasMorePages = false;
                };

                printDocument.Print();

                AddLog($"Teste de impressão enviado para: {printerName}");
                MessageBox.Show(
                    "Teste de impressão enviado com sucesso.",
                    "Sucesso",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                AddLog("Erro no teste de impressão: " + ex.Message);
                MessageBox.Show(
                    "Erro no teste de impressão:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private async void btnStartListener_Click(object sender, EventArgs e)
        {
            if (_isListening)
            {
                StopListener();
                return;
            }

            await StartListenerAsync();
        }

        private async Task StartListenerAsync()
        {
            try
            {
                var config = GetConfigFromForm();
                ValidateConfig(config);

                AddLog("Registrando agente na API...");

                var registerResponse = await _printAgentService.SendAsync(
                    config.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "register",
                        AgentId = config.AgentId,
                        AgentToken = config.AgentToken,
                        MachineName = Environment.MachineName,
                        UnitId = config.UnitId,
                        KioskId = config.KioskId,
                        WindowsPrinterName = config.PrinterName,
                        AppVersion = Application.ProductVersion
                    });

                if (registerResponse == null || !registerResponse.Ok)
                    throw new Exception(registerResponse?.Error ?? "A API não confirmou o registro do agente.");

                if (registerResponse.Agent != null)
                {
                    lblSystemPrinterValue.Text = string.IsNullOrWhiteSpace(registerResponse.Agent.PrinterName)
                        ? "-"
                        : registerResponse.Agent.PrinterName;

                    if (!string.IsNullOrWhiteSpace(registerResponse.Agent.AgentId))
                        lblAgentIdValue.Text = registerResponse.Agent.AgentId;
                }

                _listenerCts = new CancellationTokenSource();
                UpdateStatusUi(true);

                AddLog("Agente registrado com sucesso.");
                AddLog("Escuta iniciada.");

                _ = ListenLoopAsync(_listenerCts.Token);
            }
            catch (Exception ex)
            {
                UpdateStatusUi(false);
                AddLog("Erro ao iniciar escuta: " + ex.Message);
                MessageBox.Show(
                    "Erro ao iniciar escuta:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void StopListener()
        {
            try
            {
                _listenerCts?.Cancel();
                _listenerCts?.Dispose();
                _listenerCts = null;
            }
            catch
            {
            }

            UpdateStatusUi(false);
            AddLog("Escuta parada.");
        }

        private void ValidateConfig(AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ApiBaseUrl))
                throw new Exception("Informe a API Base URL.");

            if (string.IsNullOrWhiteSpace(config.AgentToken))
                throw new Exception("Informe o Agent Token.");

            if (string.IsNullOrWhiteSpace(config.UnitId))
                throw new Exception("Informe o Unit ID.");

            if (string.IsNullOrWhiteSpace(config.KioskId))
                throw new Exception("Informe o Kiosk ID.");

            if (string.IsNullOrWhiteSpace(config.PrinterName))
                throw new Exception("Selecione uma impressora.");
        }

        private async Task ListenLoopAsync(CancellationToken cancellationToken)
        {
            int heartbeatCounter = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var config = GetConfigFromForm();
                    heartbeatCounter++;

                    if (heartbeatCounter >= 6)
                    {
                        heartbeatCounter = 0;

                        await _printAgentService.SendAsync(
                            config.ApiBaseUrl,
                            new PrintAgentRequest
                            {
                                Action = "heartbeat",
                                AgentId = config.AgentId,
                                AgentToken = config.AgentToken
                            });

                        UpdateHeartbeatUi();
                        AddLog("Heartbeat enviado.");
                    }

                    var nextJobResponse = await _printAgentService.SendAsync(
                        config.ApiBaseUrl,
                        new PrintAgentRequest
                        {
                            Action = "next-job",
                            AgentId = config.AgentId,
                            AgentToken = config.AgentToken
                        });

                    if (nextJobResponse?.Job != null)
                    {
                        UpdateLastJobUi(nextJobResponse.Job.PrintOrderId);
                        AddLog("Job encontrado: " + nextJobResponse.Job.PrintOrderId);
                        await ProcessJobAsync(config, nextJobResponse.Job, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddLog("Erro no loop de escuta: " + ex.Message);
                }

                try
                {
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            UpdateStatusUi(false);
        }

        private async Task ProcessJobAsync(AppConfig config, PrintJobDto job, CancellationToken cancellationToken)
        {
            try
            {
                var claimResponse = await _printAgentService.SendAsync(
                    config.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "claim-job",
                        AgentId = config.AgentId,
                        AgentToken = config.AgentToken,
                        PrintOrderId = job.PrintOrderId
                    });

                if (claimResponse == null || (claimResponse.Claimed != true && claimResponse.Status != "printing"))
                {
                    AddLog("Job não pôde ser assumido: " + job.PrintOrderId);
                    return;
                }

                AddLog("Job assumido: " + job.PrintOrderId);

                if (string.IsNullOrWhiteSpace(job.ImageUrl))
                    throw new Exception("O job não trouxe image_url.");

                string printerName = GetSelectedPrinterName();
                if (string.IsNullOrWhiteSpace(printerName))
                    printerName = config.PrinterName;

                for (int i = 0; i < Math.Max(1, job.Copies); i++)
                {
                    await PrintImageFromUrlInternalAsync(job.ImageUrl, printerName, cancellationToken);
                }

                await _printAgentService.SendAsync(
                    config.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "complete-job",
                        AgentId = config.AgentId,
                        AgentToken = config.AgentToken,
                        PrintOrderId = job.PrintOrderId,
                        CopiesPrinted = Math.Max(1, job.Copies)
                    });

                UpdateLastJobUi(job.PrintOrderId);
                AddLog("Job concluído: " + job.PrintOrderId);
            }
            catch (Exception ex)
            {
                AddLog("Falha no job " + job.PrintOrderId + ": " + ex.Message);

                try
                {
                    await _printAgentService.SendAsync(
                        config.ApiBaseUrl,
                        new PrintAgentRequest
                        {
                            Action = "fail-job",
                            AgentId = config.AgentId,
                            AgentToken = config.AgentToken,
                            PrintOrderId = job.PrintOrderId,
                            ErrorMessage = ex.Message,
                            Retryable = true
                        });
                }
                catch (Exception failEx)
                {
                    AddLog("Erro ao informar falha do job: " + failEx.Message);
                }
            }
        }

        private async Task PrintImageFromUrlInternalAsync(string imageUrl, string printerName, CancellationToken cancellationToken)
        {
            AddLog("Baixando imagem do job...");

            string tempFilePath;

            using (HttpClient client = new HttpClient())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl, cancellationToken);

                string extension = ".jpg";

                if (imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase))
                    extension = ".png";
                else if (imageUrl.Contains(".bmp", StringComparison.OrdinalIgnoreCase))
                    extension = ".bmp";
                else if (imageUrl.Contains(".jpeg", StringComparison.OrdinalIgnoreCase))
                    extension = ".jpeg";

                tempFilePath = Path.Combine(
                    Path.GetTempPath(),
                    $"cbl_print_{Guid.NewGuid()}{extension}"
                );

                await File.WriteAllBytesAsync(tempFilePath, imageBytes, cancellationToken);
            }

            try
            {
                PrintImageFromFile(tempFilePath, printerName);
            }
            finally
            {
                try
                {
                    if (File.Exists(tempFilePath))
                        File.Delete(tempFilePath);
                }
                catch
                {
                }
            }
        }

        private void PrintImageFromFile(string imagePath, string printerName)
        {
            using Image originalImage = Image.FromFile(imagePath);
            using Bitmap preparedImage = new Bitmap(originalImage);

            ApplyRotation(preparedImage, cmbRotation.SelectedItem?.ToString() ?? "Automático");
            preparedImage.SetResolution(300, 300);

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            if (!printDocument.PrinterSettings.IsValid)
                throw new Exception("A impressora selecionada não é válida no Windows.");

            PaperSize? selectedPaper = null;

            foreach (PaperSize ps in printDocument.PrinterSettings.PaperSizes)
            {
                bool is4x6 =
                    (ps.Width == 600 && ps.Height == 400) ||
                    (ps.Width == 400 && ps.Height == 600);

                if (is4x6)
                {
                    selectedPaper = ps;
                    break;
                }
            }

            if (selectedPaper == null)
            {
                selectedPaper = new PaperSize("4x6", 400, 600);
            }

            bool landscape = preparedImage.Width >= preparedImage.Height;

            printDocument.DefaultPageSettings.PaperSize = selectedPaper;
            printDocument.DefaultPageSettings.Landscape = landscape;
            printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDocument.OriginAtMargins = false;

            int bleed = (int)nudBleed.Value;
            int offsetX = (int)nudOffsetX.Value;
            int offsetY = (int)nudOffsetY.Value;

            printDocument.PrintPage += (s, ev) =>
            {
                ev.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                ev.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                ev.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                ev.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

                float hardX = ev.PageSettings.HardMarginX;
                float hardY = ev.PageSettings.HardMarginY;

                Rectangle drawRect = new Rectangle(
                    (int)Math.Round(-hardX) - bleed + offsetX,
                    (int)Math.Round(-hardY) - bleed + offsetY,
                    ev.PageBounds.Width + (int)Math.Round(hardX * 2) + (bleed * 2),
                    ev.PageBounds.Height + (int)Math.Round(hardY * 2) + (bleed * 2)
                );

                ev.Graphics.DrawImage(preparedImage, drawRect);
                ev.HasMorePages = false;
            };

            printDocument.Print();
        }

        private void ApplyRotation(Bitmap bitmap, string rotationMode)
        {
            switch (rotationMode)
            {
                case "90°":
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case "180°":
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case "270°":
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                case "Automático":
                    break;
                case "0°":
                default:
                    break;
            }
        }

        private void AddLog(string message)
        {
            string line = $"[{DateTime.Now:HH:mm:ss}] {message}";

            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action(() => listBox1.Items.Insert(0, line)));
            }
            else
            {
                listBox1.Items.Insert(0, line);
            }
        }

        private void label3_Click(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void lblSupabaseUrl_Click(object sender, EventArgs e) { }
        private void txtSupabaseUrl_TextChanged(object sender, EventArgs e) { }
    }
}