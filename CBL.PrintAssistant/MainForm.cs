using System.Drawing.Printing;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace CBL.PrintAssistant
{
    public partial class MainForm : Form
    {
        private readonly string _configPath;
        private readonly PrintAgentService _printAgentService = new PrintAgentService();
        private readonly UpdateService _updateService = new UpdateService();

        private AppConfig? _currentConfig;

        private CancellationTokenSource? _normalCts;
        private CancellationTokenSource? _stripCts;

        private bool _normalListening;
        private bool _stripListening;

        private bool _allowClose;
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;

        private const string StartupRegistryName = "CBL.PrintAssistant";
        private const string GitHubOwner = "CristianoCBL";
        private const string GitHubRepo = "CBL.PrintAssistant";
        private const string ProfileNormal = "Normal";
        private const string ProfileStrip = "Tirinha";

        public MainForm()
        {
            InitializeComponent();

            _configPath = Path.Combine(Application.StartupPath, "appconfig.json");

            ConfigureRotationCombos();
            ConfigureProfileSelector();
            InitializeTray();
            SetStatus(lblNormalStatusDot, lblNormalStatusText, false);
            SetStatus(lblStripStatusDot, lblStripStatusText, false);

            txtNormalAgentId.Text = Environment.MachineName + "-normal";
            txtStripAgentId.Text = Environment.MachineName + "-strip";

            AddLog("Aplicativo iniciado.");
            AddLog("Modo de uso: Normal ou Tirinha (um perfil por vez).");

            LoadInstalledPrinters();
            LoadConfig();
            UpdateActiveProfileUi();

            Shown += MainForm_Shown;
            Resize += MainForm_Resize;
            FormClosing += MainForm_FormClosing;
        }

        private void ConfigureRotationCombos()
        {
            string[] rotations = { "Automático", "0°", "90°", "180°", "270°" };

            cmbNormalRotation.Items.Clear();
            cmbStripRotation.Items.Clear();

            cmbNormalRotation.Items.AddRange(rotations);
            cmbStripRotation.Items.AddRange(rotations);

            cmbNormalRotation.SelectedIndex = 0;
            cmbStripRotation.SelectedIndex = 0;
        }

        private void ConfigureProfileSelector()
        {
            cmbActiveProfile.Items.Clear();
            cmbActiveProfile.Items.Add(ProfileNormal);
            cmbActiveProfile.Items.Add(ProfileStrip);
            cmbActiveProfile.SelectedIndex = 0;
        }

        private string GetSelectedProfileMode()
        {
            return string.Equals(cmbActiveProfile.SelectedItem?.ToString(), ProfileStrip, StringComparison.OrdinalIgnoreCase)
                ? ProfileStrip
                : ProfileNormal;
        }

        private void cmbActiveProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateActiveProfileUi();
        }

        private void UpdateActiveProfileUi()
        {
            bool normalSelected = GetSelectedProfileMode() == ProfileNormal;

            groupNormal.Enabled = normalSelected;
            groupStrip.Enabled = !normalSelected;
            btnStartAll.Text = normalSelected ? "Iniciar Normal" : "Iniciar Tirinha";
        }

        private void InitializeTray()
        {
            _trayMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Abrir");
            openItem.Click += (s, e) => ShowFromTray();

            var startItem = new ToolStripMenuItem("Iniciar Perfil Selecionado");
            startItem.Click += async (s, e) => await StartSelectedProfileAsync();

            var stopItem = new ToolStripMenuItem("Parar Escuta");
            stopItem.Click += (s, e) => StopAll();

            var updateItem = new ToolStripMenuItem("Verificar Atualização");
            updateItem.Click += async (s, e) => await CheckForUpdatesAsync(true);

            var exitItem = new ToolStripMenuItem("Sair");
            exitItem.Click += (s, e) => ExitApplication();

            _trayMenu.Items.Add(openItem);
            _trayMenu.Items.Add(startItem);
            _trayMenu.Items.Add(stopItem);
            _trayMenu.Items.Add(updateItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add(exitItem);

            _trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "CBL Print Assistant",
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private async void MainForm_Shown(object? sender, EventArgs e)
        {
            foreach (string arg in Environment.GetCommandLineArgs())
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
                HideToTray();
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
        }

        private void ShowFromTray()
        {
            Show();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void ExitApplication()
        {
            try
            {
                _allowClose = true;

                _normalCts?.Cancel();
                _normalCts?.Dispose();
                _stripCts?.Cancel();
                _stripCts?.Dispose();

                _trayIcon?.Dispose();
                _trayMenu?.Dispose();
            }
            catch
            {
            }

            Application.Exit();
        }

        private void SetStatus(Label dot, Label text, bool active)
        {
            dot.ForeColor = active ? Color.ForestGreen : Color.Firebrick;
            text.Text = active ? "Ativo" : "Inativo";
        }

        private void ApplyStartupSetting(bool enabled)
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Run",
                writable: true);

            if (key == null)
                throw new Exception("Não foi possível acessar o registro do Windows.");

            if (enabled)
            {
                string value = $"\"{Application.ExecutablePath}\" --tray";
                key.SetValue(StartupRegistryName, value);
            }
            else
            {
                if (key.GetValue(StartupRegistryName) != null)
                    key.DeleteValue(StartupRegistryName, false);
            }
        }

        private void btnLoadPrinters_Click(object sender, EventArgs e)
        {
            LoadInstalledPrinters();
        }

        private void LoadInstalledPrinters()
        {
            try
            {
                cmbNormalPrinter.Items.Clear();
                cmbStripPrinter.Items.Clear();

                foreach (string printerName in PrinterSettings.InstalledPrinters)
                {
                    cmbNormalPrinter.Items.Add(printerName);
                    cmbStripPrinter.Items.Add(printerName);
                }

                if (cmbNormalPrinter.Items.Count == 0)
                {
                    AddLog("Nenhuma impressora encontrada no Windows.");
                    return;
                }

                int askIndex = -1;

                for (int i = 0; i < cmbNormalPrinter.Items.Count; i++)
                {
                    string itemText = cmbNormalPrinter.Items[i]?.ToString() ?? "";

                    if (itemText.Contains("ASK-300", StringComparison.OrdinalIgnoreCase) ||
                        itemText.Contains("FUJIFILM", StringComparison.OrdinalIgnoreCase))
                    {
                        askIndex = i;
                        break;
                    }
                }

                if (askIndex >= 0)
                {
                    cmbNormalPrinter.SelectedIndex = askIndex;
                    cmbStripPrinter.SelectedIndex = askIndex;
                }
                else
                {
                    cmbNormalPrinter.SelectedIndex = 0;
                    cmbStripPrinter.SelectedIndex = 0;
                }

                TryRestoreSavedPrinters();

                AddLog("Impressoras carregadas.");
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar impressoras: " + ex.Message);
            }
        }

        private AppConfig GetConfigFromForm()
        {
            return new AppConfig
            {
                ApiBaseUrl = txtSupabaseUrl.Text.Trim(),
                UnitId = txtUnitId.Text.Trim(),
                KioskId = txtKioskId.Text.Trim(),
                StartWithWindows = chkStartWithWindows.Checked,
                ActiveProfile = GetSelectedProfileMode(),
                NormalProfile = new PrintProfileConfig
                {
                    AgentId = txtNormalAgentId.Text.Trim(),
                    AgentToken = txtNormalToken.Text.Trim(),
                    PrinterName = cmbNormalPrinter.SelectedItem?.ToString() ?? "",
                    RotationMode = cmbNormalRotation.SelectedItem?.ToString() ?? "Automático",
                    Bleed = (int)nudNormalBleed.Value,
                    OffsetX = (int)nudNormalOffsetX.Value,
                    OffsetY = (int)nudNormalOffsetY.Value
                },
                StripProfile = new PrintProfileConfig
                {
                    AgentId = txtStripAgentId.Text.Trim(),
                    AgentToken = txtStripToken.Text.Trim(),
                    PrinterName = cmbStripPrinter.SelectedItem?.ToString() ?? "",
                    RotationMode = cmbStripRotation.SelectedItem?.ToString() ?? "Automático",
                    Bleed = (int)nudStripBleed.Value,
                    OffsetX = (int)nudStripOffsetX.Value,
                    OffsetY = (int)nudStripOffsetY.Value
                }
            };
        }

        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            try
            {
                var config = GetConfigFromForm();

                File.WriteAllText(_configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
                _currentConfig = config;

                ApplyStartupSetting(config.StartWithWindows);

                AddLog("Configuração salva com sucesso.");
                MessageBox.Show("Configuração salva com sucesso.", "Sucesso",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog("Erro ao salvar configuração: " + ex.Message);
                MessageBox.Show("Erro ao salvar configuração:\n" + ex.Message, "Erro",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                    return;

                string json = File.ReadAllText(_configPath);
                var config = JsonConvert.DeserializeObject<AppConfig>(json);

                if (config == null)
                    return;

                _currentConfig = config;

                txtSupabaseUrl.Text = config.ApiBaseUrl;
                txtUnitId.Text = config.UnitId;
                txtKioskId.Text = config.KioskId;
                chkStartWithWindows.Checked = config.StartWithWindows;

                txtNormalAgentId.Text = string.IsNullOrWhiteSpace(config.NormalProfile.AgentId)
                    ? Environment.MachineName + "-normal"
                    : config.NormalProfile.AgentId;
                txtNormalToken.Text = config.NormalProfile.AgentToken;

                txtStripAgentId.Text = string.IsNullOrWhiteSpace(config.StripProfile.AgentId)
                    ? Environment.MachineName + "-strip"
                    : config.StripProfile.AgentId;
                txtStripToken.Text = config.StripProfile.AgentToken;

                if (cmbNormalRotation.Items.Contains(config.NormalProfile.RotationMode))
                    cmbNormalRotation.SelectedItem = config.NormalProfile.RotationMode;

                if (cmbStripRotation.Items.Contains(config.StripProfile.RotationMode))
                    cmbStripRotation.SelectedItem = config.StripProfile.RotationMode;

                nudNormalBleed.Value = ClampNumeric(nudNormalBleed, config.NormalProfile.Bleed);
                nudNormalOffsetX.Value = ClampNumeric(nudNormalOffsetX, config.NormalProfile.OffsetX);
                nudNormalOffsetY.Value = ClampNumeric(nudNormalOffsetY, config.NormalProfile.OffsetY);

                nudStripBleed.Value = ClampNumeric(nudStripBleed, config.StripProfile.Bleed);
                nudStripOffsetX.Value = ClampNumeric(nudStripOffsetX, config.StripProfile.OffsetX);
                nudStripOffsetY.Value = ClampNumeric(nudStripOffsetY, config.StripProfile.OffsetY);

                if (cmbActiveProfile.Items.Contains(config.ActiveProfile))
                    cmbActiveProfile.SelectedItem = config.ActiveProfile;
                else
                    cmbActiveProfile.SelectedItem = ProfileNormal;

                TryRestoreSavedPrinters();
                UpdateActiveProfileUi();

                AddLog("Configuração carregada.");
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar configuração: " + ex.Message);
            }
        }

        private decimal ClampNumeric(NumericUpDown control, int value)
        {
            if (value < control.Minimum) return control.Minimum;
            if (value > control.Maximum) return control.Maximum;
            return value;
        }

        private void TryRestoreSavedPrinters()
        {
            if (_currentConfig == null)
                return;

            SelectPrinterIfExists(cmbNormalPrinter, _currentConfig.NormalProfile.PrinterName);
            SelectPrinterIfExists(cmbStripPrinter, _currentConfig.StripProfile.PrinterName);
        }

        private void SelectPrinterIfExists(ComboBox combo, string printerName)
        {
            if (string.IsNullOrWhiteSpace(printerName))
                return;

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(combo.Items[i]?.ToString(), printerName, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void btnStartAll_Click(object sender, EventArgs e)
        {
            await StartSelectedProfileAsync();
        }

        private void btnStopAll_Click(object sender, EventArgs e)
        {
            StopAll();
        }

        private async Task StartSelectedProfileAsync()
        {
            var config = GetConfigFromForm();

            ValidateGeneralConfig(config);

            if (GetSelectedProfileMode() == ProfileNormal)
            {
                ValidateProfile(ProfileNormal, config.NormalProfile);
                StopStrip();
                await StartNormalAsync(config);
                return;
            }

            ValidateProfile(ProfileStrip, config.StripProfile);
            StopNormal();
            await StartStripAsync(config);
        }

        private void StopAll()
        {
            StopNormal();
            StopStrip();
        }

        private async Task StartNormalAsync(AppConfig config)
        {
            if (_normalListening)
                return;

            try
            {
                AddLog("[Normal] Registrando agente...");

                var profile = config.NormalProfile;

                var registerResponse = await _printAgentService.SendAsync(
                    config.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "register",
                        AgentId = profile.AgentId,
                        AgentToken = profile.AgentToken,
                        MachineName = Environment.MachineName,
                        UnitId = config.UnitId,
                        KioskId = config.KioskId,
                        WindowsPrinterName = profile.PrinterName,
                        AppVersion = Application.ProductVersion
                    });

                if (registerResponse == null || !registerResponse.Ok)
                    throw new Exception(registerResponse?.Error ?? "A API não confirmou o registro.");

                if (registerResponse.Agent != null)
                {
                    lblNormalSystemPrinterValue.Text =
                        string.IsNullOrWhiteSpace(registerResponse.Agent.PrinterName)
                        ? "-"
                        : registerResponse.Agent.PrinterName;
                }

                _normalCts = new CancellationTokenSource();
                _normalListening = true;
                SetStatus(lblNormalStatusDot, lblNormalStatusText, true);

                AddLog($"[Normal] Escuta iniciada. Agent: {profile.AgentId}");

                _ = ListenLoopAsync(
                    ProfileNormal,
                    config,
                    profile,
                    _normalCts.Token,
                    () =>
                    {
                        _normalListening = false;
                        SetStatus(lblNormalStatusDot, lblNormalStatusText, false);
                    });
            }
            catch (Exception ex)
            {
                _normalListening = false;
                SetStatus(lblNormalStatusDot, lblNormalStatusText, false);
                AddLog("[Normal] Erro ao iniciar: " + ex.Message);
            }
        }

        private async Task StartStripAsync(AppConfig config)
        {
            if (_stripListening)
                return;

            try
            {
                AddLog("[Tirinha] Registrando agente...");

                var profile = config.StripProfile;

                var registerResponse = await _printAgentService.SendAsync(
                    config.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "register",
                        AgentId = profile.AgentId,
                        AgentToken = profile.AgentToken,
                        MachineName = Environment.MachineName,
                        UnitId = config.UnitId,
                        KioskId = config.KioskId,
                        WindowsPrinterName = profile.PrinterName,
                        AppVersion = Application.ProductVersion
                    });

                if (registerResponse == null || !registerResponse.Ok)
                    throw new Exception(registerResponse?.Error ?? "A API não confirmou o registro.");

                if (registerResponse.Agent != null)
                {
                    lblStripSystemPrinterValue.Text =
                        string.IsNullOrWhiteSpace(registerResponse.Agent.PrinterName)
                        ? "-"
                        : registerResponse.Agent.PrinterName;
                }

                _stripCts = new CancellationTokenSource();
                _stripListening = true;
                SetStatus(lblStripStatusDot, lblStripStatusText, true);

                AddLog($"[Tirinha] Escuta iniciada. Agent: {profile.AgentId}");

                _ = ListenLoopAsync(
                    ProfileStrip,
                    config,
                    profile,
                    _stripCts.Token,
                    () =>
                    {
                        _stripListening = false;
                        SetStatus(lblStripStatusDot, lblStripStatusText, false);
                    });
            }
            catch (Exception ex)
            {
                _stripListening = false;
                SetStatus(lblStripStatusDot, lblStripStatusText, false);
                AddLog("[Tirinha] Erro ao iniciar: " + ex.Message);
            }
        }

        private void StopNormal()
        {
            try
            {
                _normalCts?.Cancel();
                _normalCts?.Dispose();
                _normalCts = null;
            }
            catch
            {
            }

            _normalListening = false;
            SetStatus(lblNormalStatusDot, lblNormalStatusText, false);
            AddLog("[Normal] Escuta parada.");
        }

        private void StopStrip()
        {
            try
            {
                _stripCts?.Cancel();
                _stripCts?.Dispose();
                _stripCts = null;
            }
            catch
            {
            }

            _stripListening = false;
            SetStatus(lblStripStatusDot, lblStripStatusText, false);
            AddLog("[Tirinha] Escuta parada.");
        }

        private void ValidateGeneralConfig(AppConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.ApiBaseUrl))
                throw new Exception("Informe a API Base URL.");

            if (string.IsNullOrWhiteSpace(config.UnitId))
                throw new Exception("Informe o Unit ID.");

            if (string.IsNullOrWhiteSpace(config.KioskId))
                throw new Exception("Informe o Kiosk ID.");
        }

        private void ValidateProfile(string profileName, PrintProfileConfig profile)
        {
            if (string.IsNullOrWhiteSpace(profile.AgentId))
                throw new Exception($"Informe o Agent ID do perfil {profileName}.");

            if (string.IsNullOrWhiteSpace(profile.AgentToken))
                throw new Exception($"Informe o Agent Token do perfil {profileName}.");

            if (string.IsNullOrWhiteSpace(profile.PrinterName))
                throw new Exception($"Selecione a impressora Windows do perfil {profileName}.");
        }

        private async Task ListenLoopAsync(
            string profileName,
            AppConfig generalConfig,
            PrintProfileConfig profile,
            CancellationToken cancellationToken,
            Action onStopped)
        {
            int heartbeatCounter = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    heartbeatCounter++;

                    if (heartbeatCounter >= 6)
                    {
                        heartbeatCounter = 0;

                        await _printAgentService.SendAsync(
                            generalConfig.ApiBaseUrl,
                            new PrintAgentRequest
                            {
                                Action = "heartbeat",
                                AgentId = profile.AgentId,
                                AgentToken = profile.AgentToken
                            });

                        AddLog($"[{profileName}] Heartbeat enviado.");
                    }

                    var nextJobResponse = await _printAgentService.SendAsync(
                        generalConfig.ApiBaseUrl,
                        new PrintAgentRequest
                        {
                            Action = "next-job",
                            AgentId = profile.AgentId,
                            AgentToken = profile.AgentToken
                        });

                    if (nextJobResponse?.Job != null)
                    {
                        AddLog($"[{profileName}] Job encontrado: {nextJobResponse.Job.PrintOrderId}");
                        await ProcessJobAsync(profileName, generalConfig, profile, nextJobResponse.Job, cancellationToken);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddLog($"[{profileName}] Erro no loop: {ex.Message}");
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

            onStopped();
        }

        private async Task ProcessJobAsync(
            string profileName,
            AppConfig generalConfig,
            PrintProfileConfig profile,
            PrintJobDto job,
            CancellationToken cancellationToken)
        {
            try
            {
                var claimResponse = await _printAgentService.SendAsync(
                    generalConfig.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "claim-job",
                        AgentId = profile.AgentId,
                        AgentToken = profile.AgentToken,
                        PrintOrderId = job.PrintOrderId
                    });

                if (claimResponse == null || (claimResponse.Claimed != true && claimResponse.Status != "printing"))
                {
                    AddLog($"[{profileName}] Job não pôde ser assumido: {job.PrintOrderId}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(job.ImageUrl))
                    throw new Exception("O job não trouxe image_url.");

                bool isStripProfile = string.Equals(profileName, ProfileStrip, StringComparison.OrdinalIgnoreCase);

                for (int i = 0; i < Math.Max(1, job.Copies); i++)
                {
                    await PrintImageFromUrlInternalAsync(job.ImageUrl, profile, isStripProfile, cancellationToken);
                }

                await _printAgentService.SendAsync(
                    generalConfig.ApiBaseUrl,
                    new PrintAgentRequest
                    {
                        Action = "complete-job",
                        AgentId = profile.AgentId,
                        AgentToken = profile.AgentToken,
                        PrintOrderId = job.PrintOrderId,
                        CopiesPrinted = Math.Max(1, job.Copies)
                    });

                AddLog($"[{profileName}] Job concluído: {job.PrintOrderId}");
            }
            catch (Exception ex)
            {
                AddLog($"[{profileName}] Falha no job {job.PrintOrderId}: {ex.Message}");

                try
                {
                    await _printAgentService.SendAsync(
                        generalConfig.ApiBaseUrl,
                        new PrintAgentRequest
                        {
                            Action = "fail-job",
                            AgentId = profile.AgentId,
                            AgentToken = profile.AgentToken,
                            PrintOrderId = job.PrintOrderId,
                            ErrorMessage = ex.Message,
                            Retryable = true
                        });
                }
                catch (Exception failEx)
                {
                    AddLog($"[{profileName}] Erro ao informar falha: {failEx.Message}");
                }
            }
        }

        private async Task PrintImageFromUrlInternalAsync(
            string imageUrl,
            PrintProfileConfig profile,
            bool isStripProfile,
            CancellationToken cancellationToken)
        {
            string tempFilePath;

            using (HttpClient client = new HttpClient())
            {
                byte[] imageBytes = await client.GetByteArrayAsync(imageUrl, cancellationToken);

                string extension = ".jpg";
                if (imageUrl.Contains(".png", StringComparison.OrdinalIgnoreCase)) extension = ".png";
                else if (imageUrl.Contains(".bmp", StringComparison.OrdinalIgnoreCase)) extension = ".bmp";
                else if (imageUrl.Contains(".jpeg", StringComparison.OrdinalIgnoreCase)) extension = ".jpeg";

                tempFilePath = Path.Combine(Path.GetTempPath(), $"cbl_print_{Guid.NewGuid()}{extension}");
                await File.WriteAllBytesAsync(tempFilePath, imageBytes, cancellationToken);
            }

            try
            {
                PrintImageFromFile(tempFilePath, profile, isStripProfile);
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

        private void PrintImageFromFile(string imagePath, PrintProfileConfig profile, bool isStripProfile)
        {
            using Image originalImage = Image.FromFile(imagePath);
            using Bitmap preparedImage = new Bitmap(originalImage);

            ApplyRotation(preparedImage, profile.RotationMode);
            preparedImage.SetResolution(300, 300);

            using Bitmap finalImage = isStripProfile
                ? CreateStripSheet(preparedImage)
                : new Bitmap(preparedImage);

            finalImage.SetResolution(300, 300);

            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = profile.PrinterName;

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
                selectedPaper = new PaperSize("4x6", 400, 600);

            bool landscape = finalImage.Width >= finalImage.Height;

            printDocument.DefaultPageSettings.PaperSize = selectedPaper;
            printDocument.DefaultPageSettings.Landscape = landscape;
            printDocument.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
            printDocument.OriginAtMargins = false;

            int bleed = profile.Bleed;
            int offsetX = profile.OffsetX;
            int offsetY = profile.OffsetY;

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

                ev.Graphics.DrawImage(finalImage, drawRect);
                ev.HasMorePages = false;
            };

            printDocument.Print();
        }

        private Bitmap CreateStripSheet(Bitmap stripImage)
        {
            const int canvasWidth = 1800;
            const int canvasHeight = 1200;
            const int gap = 12;

            Bitmap canvas = new Bitmap(canvasWidth, canvasHeight);
            canvas.SetResolution(300, 300);

            using Graphics graphics = Graphics.FromImage(canvas);
            graphics.Clear(Color.White);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            using Bitmap stripCopy = new Bitmap(stripImage);
            if (stripCopy.Height >= stripCopy.Width)
                stripCopy.RotateFlip(RotateFlipType.Rotate90FlipNone);

            Rectangle topSlot = new Rectangle(0, 0, canvasWidth, (canvasHeight / 2) - (gap / 2));
            Rectangle bottomSlot = new Rectangle(0, (canvasHeight / 2) + (gap / 2), canvasWidth, (canvasHeight / 2) - (gap / 2));

            DrawImageContain(graphics, stripCopy, topSlot);
            DrawImageContain(graphics, stripCopy, bottomSlot);

            return canvas;
        }

        private void DrawImageContain(Graphics graphics, Image image, Rectangle bounds)
        {
            float imageRatio = (float)image.Width / image.Height;
            float boundsRatio = (float)bounds.Width / bounds.Height;

            int drawWidth;
            int drawHeight;

            if (imageRatio > boundsRatio)
            {
                drawWidth = bounds.Width;
                drawHeight = (int)Math.Round(bounds.Width / imageRatio);
            }
            else
            {
                drawHeight = bounds.Height;
                drawWidth = (int)Math.Round(bounds.Height * imageRatio);
            }

            Rectangle target = new Rectangle(
                bounds.X + ((bounds.Width - drawWidth) / 2),
                bounds.Y + ((bounds.Height - drawHeight) / 2),
                drawWidth,
                drawHeight);

            graphics.DrawImage(image, target);
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
                default:
                    break;
            }
        }

        private async Task CheckForUpdatesAsync(bool manual)
        {
            try
            {
                AddLog("Verificando atualizações...");

                var currentVersion = new Version(Application.ProductVersion.Split('+')[0]);
                var result = await _updateService.CheckForUpdateAsync(GitHubOwner, GitHubRepo, currentVersion);

                if (!result.HasUpdate)
                {
                    AddLog("Nenhuma atualização encontrada.");

                    if (manual)
                    {
                        MessageBox.Show("Você já está na versão mais recente.", "Atualização",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return;
                }

                AddLog($"Nova versão encontrada: {result.LatestVersion}");

                var ask = MessageBox.Show(
                    $"Nova versão disponível: {result.LatestVersion}\n\nDeseja baixar e atualizar agora?",
                    "Atualização disponível",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (ask != DialogResult.Yes)
                    return;

                string scriptPath = await _updateService.DownloadAndPrepareUpdateAsync(
                    result.DownloadUrl ?? "",
                    Application.StartupPath,
                    Path.GetFileName(Application.ExecutablePath),
                    result.LatestVersion,
                    Environment.ProcessId);

                _updateService.LaunchUpdateScript(scriptPath);

                _allowClose = true;
                Application.Exit();
            }
            catch (Exception ex)
            {
                AddLog("Erro ao verificar atualização: " + ex.Message);

                if (manual)
                {
                    MessageBox.Show("Erro ao verificar atualização:\n" + ex.Message, "Erro",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCheckUpdates_Click(object sender, EventArgs e)
        {
            _ = CheckForUpdatesAsync(true);
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
    }
}
