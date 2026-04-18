using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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

        private System.Windows.Forms.Timer? _autoRetryTimer;
        private bool _autoStartAttemptInProgress;

        private HttpListener? _localConfigListener;
        private CancellationTokenSource? _localConfigServerCts;

        private const string StartupRegistryName = "CBL.PrintAssistant";
        private const string GitHubOwner = "CristianoCBL";
        private const string GitHubRepo = "CBL.PrintAssistant";
        private const string ProfileNormal = "Normal";
        private const string ProfileStrip = "Tirinha";
        private const string ModeBoth = "Ambos";
        private const string LocalListenerPrefix = "http://127.0.0.1:38451/";

        public MainForm()
        {
            InitializeComponent();

            _configPath = Path.Combine(Application.StartupPath, "appconfig.json");

            ConfigureRotationCombos();
            ConfigureRunModeSelector();
            InitializeTray();
            InitializeAutoRetryTimer();

            SetStatus(lblNormalStatusDot, lblNormalStatusText, false);
            SetStatus(lblStripStatusDot, lblStripStatusText, false);

            txtNormalAgentId.Text = Environment.MachineName + "-normal";
            txtStripAgentId.Text = Environment.MachineName + "-strip";

            AddLog("Aplicativo iniciado.");
            AddLog("Modos de uso: Só Normal, Só Tirinha ou Ambos.");

            LoadInstalledPrinters();
            LoadConfig();
            UpdateRunModeUi();

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

        private void ConfigureRunModeSelector()
        {
            cmbRunMode.Items.Clear();
            cmbRunMode.Items.Add(ProfileNormal);
            cmbRunMode.Items.Add(ProfileStrip);
            cmbRunMode.Items.Add(ModeBoth);
            cmbRunMode.SelectedIndex = 0;
        }

        private void InitializeAutoRetryTimer()
        {
            _autoRetryTimer = new System.Windows.Forms.Timer();
            _autoRetryTimer.Interval = 30000;
            _autoRetryTimer.Tick += async (s, e) => await AutoEnsureListeningAsync();
        }

        private string GetSelectedRunMode()
        {
            string? value = cmbRunMode.SelectedItem?.ToString();
            if (string.Equals(value, ProfileStrip, StringComparison.OrdinalIgnoreCase))
                return ProfileStrip;
            if (string.Equals(value, ModeBoth, StringComparison.OrdinalIgnoreCase))
                return ModeBoth;
            return ProfileNormal;
        }

        private void cmbRunMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateRunModeUi();
        }

        private void UpdateRunModeUi()
        {
            string mode = GetSelectedRunMode();

            bool normalEnabled = mode == ProfileNormal || mode == ModeBoth;
            bool stripEnabled = mode == ProfileStrip || mode == ModeBoth;

            groupNormal.Enabled = normalEnabled;
            groupStrip.Enabled = stripEnabled;

            btnStartAll.Text = mode switch
            {
                ProfileStrip => "Iniciar Tirinha",
                ModeBoth => "Iniciar Ambos",
                _ => "Iniciar Normal"
            };
        }

        private void InitializeTray()
        {
            _trayMenu = new ContextMenuStrip();

            var openItem = new ToolStripMenuItem("Abrir");
            openItem.Click += (s, e) => ShowFromTray();

            var startItem = new ToolStripMenuItem("Iniciar modo selecionado");
            startItem.Click += async (s, e) => await StartSelectedModeAsync();

            var syncItem = new ToolStripMenuItem("Sincronizar do Site");
            syncItem.Click += async (s, e) => await SyncFromSiteForCurrentModeAsync();

            var stopItem = new ToolStripMenuItem("Parar Escuta");
            stopItem.Click += (s, e) => StopAll();

            var updateItem = new ToolStripMenuItem("Verificar Atualização");
            updateItem.Click += async (s, e) => await CheckForUpdatesAsync(true);

            var exitItem = new ToolStripMenuItem("Sair");
            exitItem.Click += (s, e) => ExitApplication();

            _trayMenu.Items.Add(openItem);
            _trayMenu.Items.Add(startItem);
            _trayMenu.Items.Add(syncItem);
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

            StartLocalConfigServerIfEnabled();
            StartAutoRetryIfEnabled();

            await AutoEnsureListeningAsync();
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

                StopAutoRetry();
                StopLocalConfigServer();

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

                RefreshPaperLists();
                TryRestoreSavedPrinters();
                TryRestoreSavedPaperNames();

                AddLog("Impressoras carregadas.");
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar impressoras: " + ex.Message);
            }
        }

        private void cmbNormalPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPaperSizesForPrinter(cmbNormalPrinter, cmbNormalPaper, false);
        }

        private void cmbStripPrinter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPaperSizesForPrinter(cmbStripPrinter, cmbStripPaper, true);
        }

        private void RefreshPaperLists()
        {
            LoadPaperSizesForPrinter(cmbNormalPrinter, cmbNormalPaper, false);
            LoadPaperSizesForPrinter(cmbStripPrinter, cmbStripPaper, true);
        }

        private void LoadPaperSizesForPrinter(ComboBox printerCombo, ComboBox paperCombo, bool stripMode)
        {
            paperCombo.Items.Clear();

            string? printerName = printerCombo.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(printerName))
                return;

            try
            {
                using PrintDocument pd = new PrintDocument();
                pd.PrinterSettings.PrinterName = printerName;

                if (!pd.PrinterSettings.IsValid)
                    return;

                foreach (PaperSize ps in pd.PrinterSettings.PaperSizes)
                {
                    paperCombo.Items.Add(ps.PaperName);
                }

                if (paperCombo.Items.Count == 0)
                    return;

                if (stripMode)
                    SelectPaperByPreferredNames(paperCombo, "6x2 inchx2 Type1(152x51 mmx2)", "6x2", "Type1");
                else
                    SelectPaperByPreferredNames(paperCombo, "6x4 inch(152x102 mm)", "6x4", "4x6");

                if (paperCombo.SelectedIndex < 0)
                    paperCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                AddLog("Erro ao carregar papersize da impressora: " + ex.Message);
            }
        }

        private void SelectPaperByPreferredNames(ComboBox combo, params string[] candidates)
        {
            foreach (string candidate in candidates)
            {
                for (int i = 0; i < combo.Items.Count; i++)
                {
                    string itemText = combo.Items[i]?.ToString() ?? "";
                    if (itemText.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
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
                AutoStartListening = chkAutoStartListening.Checked,
                EnableLocalIntegration = chkEnableLocalIntegration.Checked,
                RunMode = GetSelectedRunMode(),
                NormalProfile = new PrintProfileConfig
                {
                    AgentId = txtNormalAgentId.Text.Trim(),
                    AgentToken = txtNormalToken.Text.Trim(),
                    PrinterName = cmbNormalPrinter.SelectedItem?.ToString() ?? "",
                    PaperName = cmbNormalPaper.SelectedItem?.ToString() ?? "",
                    RotationMode = cmbNormalRotation.SelectedItem?.ToString() ?? "Automático",
                    Dpi = (int)nudNormalDpi.Value,
                    Bleed = (int)nudNormalBleed.Value,
                    OffsetX = (int)nudNormalOffsetX.Value,
                    OffsetY = (int)nudNormalOffsetY.Value
                },
                StripProfile = new PrintProfileConfig
                {
                    AgentId = txtStripAgentId.Text.Trim(),
                    AgentToken = txtStripToken.Text.Trim(),
                    PrinterName = cmbStripPrinter.SelectedItem?.ToString() ?? "",
                    PaperName = cmbStripPaper.SelectedItem?.ToString() ?? "",
                    RotationMode = cmbStripRotation.SelectedItem?.ToString() ?? "Automático",
                    Dpi = (int)nudStripDpi.Value,
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
                SaveCurrentConfigToDisk();
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

        private void SaveCurrentConfigToDisk()
        {
            var config = GetConfigFromForm();
            File.WriteAllText(_configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            _currentConfig = config;
            ApplyStartupSetting(config.StartWithWindows);

            if (config.EnableLocalIntegration)
                StartLocalConfigServerIfEnabled();
            else
                StopLocalConfigServer();

            if (config.AutoStartListening)
                StartAutoRetryIfEnabled();
            else
                StopAutoRetry();
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
                chkAutoStartListening.Checked = config.AutoStartListening;
                chkEnableLocalIntegration.Checked = config.EnableLocalIntegration;

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

                nudNormalDpi.Value = ClampNumeric(nudNormalDpi, config.NormalProfile.Dpi);
                nudNormalBleed.Value = ClampNumeric(nudNormalBleed, config.NormalProfile.Bleed);
                nudNormalOffsetX.Value = ClampNumeric(nudNormalOffsetX, config.NormalProfile.OffsetX);
                nudNormalOffsetY.Value = ClampNumeric(nudNormalOffsetY, config.NormalProfile.OffsetY);

                nudStripDpi.Value = ClampNumeric(nudStripDpi, config.StripProfile.Dpi);
                nudStripBleed.Value = ClampNumeric(nudStripBleed, config.StripProfile.Bleed);
                nudStripOffsetX.Value = ClampNumeric(nudStripOffsetX, config.StripProfile.OffsetX);
                nudStripOffsetY.Value = ClampNumeric(nudStripOffsetY, config.StripProfile.OffsetY);

                if (cmbRunMode.Items.Contains(config.RunMode))
                    cmbRunMode.SelectedItem = config.RunMode;
                else
                    cmbRunMode.SelectedItem = ProfileNormal;

                TryRestoreSavedPrinters();
                RefreshPaperLists();
                TryRestoreSavedPaperNames();
                UpdateRunModeUi();

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

            SelectComboIfExists(cmbNormalPrinter, _currentConfig.NormalProfile.PrinterName);
            SelectComboIfExists(cmbStripPrinter, _currentConfig.StripProfile.PrinterName);
        }

        private void TryRestoreSavedPaperNames()
        {
            if (_currentConfig == null)
                return;

            SelectComboIfExists(cmbNormalPaper, _currentConfig.NormalProfile.PaperName);
            SelectComboIfExists(cmbStripPaper, _currentConfig.StripProfile.PaperName);

            if (cmbNormalPaper.SelectedIndex < 0)
                SelectPaperByPreferredNames(cmbNormalPaper, "6x4 inch(152x102 mm)", "6x4", "4x6");

            if (cmbStripPaper.SelectedIndex < 0)
                SelectPaperByPreferredNames(cmbStripPaper, "6x2 inchx2 Type1(152x51 mmx2)", "6x2", "Type1");

            if (cmbNormalPaper.Items.Count > 0 && cmbNormalPaper.SelectedIndex < 0)
                cmbNormalPaper.SelectedIndex = 0;

            if (cmbStripPaper.Items.Count > 0 && cmbStripPaper.SelectedIndex < 0)
                cmbStripPaper.SelectedIndex = 0;
        }

        private void SelectComboIfExists(ComboBox combo, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(combo.Items[i]?.ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private async void btnStartAll_Click(object sender, EventArgs e)
        {
            await StartSelectedModeAsync();
        }

        private void btnStopAll_Click(object sender, EventArgs e)
        {
            StopAll();
        }

        private async void btnSyncFromSite_Click(object sender, EventArgs e)
        {
            await SyncFromSiteForCurrentModeAsync();
        }

        private async Task SyncFromSiteForCurrentModeAsync()
        {
            try
            {
                string mode = GetSelectedRunMode();

                if (mode == ModeBoth)
                {
                    await SyncProfileFromSiteAsync(true);
                    await SyncProfileFromSiteAsync(false);
                    MessageBox.Show(
                        "Configuração dos perfis Normal e Tirinha sincronizada com sucesso.",
                        "Sincronização concluída",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                bool normalProfile = mode == ProfileNormal;
                await SyncProfileFromSiteAsync(normalProfile);

                MessageBox.Show(
                    $"Configuração do perfil {(normalProfile ? ProfileNormal : ProfileStrip)} sincronizada com sucesso.",
                    "Sincronização concluída",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLog("Erro ao sincronizar do site: " + ex.Message);
                MessageBox.Show(
                    "Erro ao sincronizar do site:\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private async Task SyncProfileFromSiteAsync(bool normalProfile)
        {
            var config = GetConfigFromForm();
            ValidateGeneralConfig(config);

            var profile = normalProfile ? config.NormalProfile : config.StripProfile;
            string profileType = normalProfile ? "normal" : "strip";
            string profileLabel = normalProfile ? ProfileNormal : ProfileStrip;

            if (string.IsNullOrWhiteSpace(profile.AgentId))
                throw new Exception($"Informe o Agent ID do perfil {profileLabel}.");

            if (string.IsNullOrWhiteSpace(profile.AgentToken))
                throw new Exception($"Informe o Agent Token do perfil {profileLabel}.");

            AddLog($"[{profileLabel}] Sincronizando configuração do site...");

            using var client = new HttpClient();

            var payload = new
            {
                action = "get-config",
                agent_id = profile.AgentId,
                agent_token = profile.AgentToken,
                profile_type = profileType
            };

            string json = JsonConvert.SerializeObject(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(config.ApiBaseUrl, content);
            string body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {body}");

            var apiResponse = JsonConvert.DeserializeObject<RemoteConfigResponse>(body);
            if (apiResponse == null || !apiResponse.Ok || apiResponse.Config == null)
                throw new Exception(apiResponse?.Error ?? "A API não retornou uma configuração válida.");

            ApplyRemoteConfigToUi(normalProfile, apiResponse.Config);
            SaveCurrentConfigToDisk();

            AddLog($"[{profileLabel}] Configuração sincronizada com sucesso.");
        }

        private void ApplyRemoteConfigToUi(bool normalProfile, RemoteProfileConfig config)
        {
            ComboBox printerCombo = normalProfile ? cmbNormalPrinter : cmbStripPrinter;
            ComboBox paperCombo = normalProfile ? cmbNormalPaper : cmbStripPaper;
            ComboBox rotationCombo = normalProfile ? cmbNormalRotation : cmbStripRotation;
            NumericUpDown dpiControl = normalProfile ? nudNormalDpi : nudStripDpi;
            NumericUpDown bleedControl = normalProfile ? nudNormalBleed : nudStripBleed;
            NumericUpDown offsetXControl = normalProfile ? nudNormalOffsetX : nudStripOffsetX;
            NumericUpDown offsetYControl = normalProfile ? nudNormalOffsetY : nudStripOffsetY;

            if (!string.IsNullOrWhiteSpace(config.WindowsPrinterName))
                SelectComboIfExists(printerCombo, config.WindowsPrinterName);

            RefreshPaperLists();

            if (!string.IsNullOrWhiteSpace(config.PaperName))
                SelectComboIfExists(paperCombo, config.PaperName);

            if (!string.IsNullOrWhiteSpace(config.RotationMode) && rotationCombo.Items.Contains(config.RotationMode))
                rotationCombo.SelectedItem = config.RotationMode;

            dpiControl.Value = ClampNumeric(dpiControl, config.Dpi ?? 300);
            bleedControl.Value = ClampNumeric(bleedControl, config.Bleed ?? (normalProfile ? 8 : 0));
            offsetXControl.Value = ClampNumeric(offsetXControl, config.OffsetX ?? 0);
            offsetYControl.Value = ClampNumeric(offsetYControl, config.OffsetY ?? 0);
        }

        private async Task StartSelectedModeAsync()
        {
            var config = GetConfigFromForm();

            ValidateGeneralConfig(config);

            string mode = GetSelectedRunMode();

            if (mode == ProfileNormal)
            {
                ValidateProfile(ProfileNormal, config.NormalProfile);
                StopStrip();
                await StartNormalAsync(config);
                return;
            }

            if (mode == ProfileStrip)
            {
                ValidateProfile(ProfileStrip, config.StripProfile);
                StopNormal();
                await StartStripAsync(config);
                return;
            }

            ValidateProfile(ProfileNormal, config.NormalProfile);
            ValidateProfile(ProfileStrip, config.StripProfile);

            await StartNormalAsync(config);
            await StartStripAsync(config);
        }

        private bool IsSelectedModeRunning()
        {
            string mode = GetSelectedRunMode();
            if (mode == ProfileNormal)
                return _normalListening;
            if (mode == ProfileStrip)
                return _stripListening;
            return _normalListening && _stripListening;
        }

        private void StartAutoRetryIfEnabled()
        {
            if (chkAutoStartListening.Checked)
                _autoRetryTimer?.Start();
        }

        private void StopAutoRetry()
        {
            _autoRetryTimer?.Stop();
        }

        private async Task AutoEnsureListeningAsync()
        {
            if (!chkAutoStartListening.Checked)
                return;

            if (_autoStartAttemptInProgress)
                return;

            if (IsSelectedModeRunning())
                return;

            _autoStartAttemptInProgress = true;
            try
            {
                AddLog("Auto conexão: verificando listeners...");
                await StartSelectedModeAsync();

                if (!IsSelectedModeRunning())
                    AddLog("Sem conexão com a API/rede. Nova tentativa em 30 segundos.");
                else
                    AddLog("Conexão restabelecida. Listener ativo.");
            }
            catch (Exception ex)
            {
                AddLog("Falha na auto conexão: " + ex.Message);
            }
            finally
            {
                _autoStartAttemptInProgress = false;
            }
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

            if (string.IsNullOrWhiteSpace(profile.PaperName))
                throw new Exception($"Selecione o PaperSize do perfil {profileName}.");
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
            preparedImage.SetResolution(profile.Dpi, profile.Dpi);

            using Bitmap finalImage = isStripProfile
                ? CreateStripSheet(preparedImage, profile.Dpi)
                : new Bitmap(preparedImage);

            finalImage.SetResolution(profile.Dpi, profile.Dpi);

            using PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = profile.PrinterName;

            if (!printDocument.PrinterSettings.IsValid)
                throw new Exception("A impressora selecionada não é válida no Windows.");

            PaperSize? selectedPaper = FindPaperSize(printDocument, profile.PaperName);

            if (selectedPaper == null)
                throw new Exception($"PaperSize não encontrado na impressora: {profile.PaperName}");

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

        private PaperSize? FindPaperSize(PrintDocument printDocument, string savedPaperName)
        {
            foreach (PaperSize ps in printDocument.PrinterSettings.PaperSizes)
            {
                if (!string.IsNullOrWhiteSpace(savedPaperName) &&
                    string.Equals(ps.PaperName, savedPaperName, StringComparison.OrdinalIgnoreCase))
                {
                    return ps;
                }
            }

            foreach (PaperSize ps in printDocument.PrinterSettings.PaperSizes)
            {
                bool is4x6 =
                    (ps.Width == 600 && ps.Height == 400) ||
                    (ps.Width == 400 && ps.Height == 600);

                if (is4x6)
                    return ps;
            }

            return null;
        }

        private Bitmap CreateStripSheet(Bitmap stripImage, int dpi)
        {
            const int canvasWidth = 1800;
            const int canvasHeight = 1200;
            const int gap = 12;

            Bitmap canvas = new Bitmap(canvasWidth, canvasHeight);
            canvas.SetResolution(dpi, dpi);

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
                var currentVersion = new Version(Application.ProductVersion.Split('+')[0]);
                var result = await _updateService.CheckForUpdateAsync(GitHubOwner, GitHubRepo, currentVersion);

                if (!result.HasUpdate)
                {
                    if (manual)
                    {
                        MessageBox.Show("Você já está na versão mais recente.", "Atualização",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return;
                }

                if (!manual)
                    return;

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

        private void StartLocalConfigServerIfEnabled()
        {
            if (!chkEnableLocalIntegration.Checked)
                return;

            if (_localConfigListener != null)
                return;

            try
            {
                _localConfigServerCts = new CancellationTokenSource();
                _localConfigListener = new HttpListener();
                _localConfigListener.Prefixes.Add(LocalListenerPrefix);
                _localConfigListener.Start();

                AddLog("Integração local ativa em " + LocalListenerPrefix);
                _ = Task.Run(() => LocalConfigServerLoopAsync(_localConfigServerCts.Token));
            }
            catch (Exception ex)
            {
                AddLog("Não foi possível iniciar integração local: " + ex.Message);
            }
        }

        private void StopLocalConfigServer()
        {
            try
            {
                _localConfigServerCts?.Cancel();
                _localConfigListener?.Stop();
                _localConfigListener?.Close();
            }
            catch
            {
            }
            finally
            {
                _localConfigListener = null;
                _localConfigServerCts?.Dispose();
                _localConfigServerCts = null;
            }
        }

        private async Task LocalConfigServerLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _localConfigListener != null)
            {
                HttpListenerContext? ctx = null;
                try
                {
                    ctx = await _localConfigListener.GetContextAsync();
                    _ = Task.Run(() => HandleLocalRequestAsync(ctx), cancellationToken);
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    AddLog("Erro na integração local: " + ex.Message);
                }
            }
        }

        private async Task HandleLocalRequestAsync(HttpListenerContext context)
        {
            try
            {
                string path = context.Request.Url?.AbsolutePath?.Trim('/').ToLowerInvariant() ?? "";

                ApplyCorsHeaders(context.Response);

                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    context.Response.Close();
                    return;
                }

                if (context.Request.HttpMethod == "GET" && path == "ping")
                {
                    await WriteJsonResponseAsync(context.Response, 200, new { ok = true, app = "CBL.PrintAssistant" });
                    return;
                }

                if (context.Request.HttpMethod == "POST" && path == "apply-config")
                {
                    using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
                    string body = await reader.ReadToEndAsync();

                    var payload = JsonConvert.DeserializeObject<LocalApplyConfigRequest>(body);
                    if (payload == null)
                    {
                        await WriteJsonResponseAsync(context.Response, 400, new { ok = false, error = "Payload inválido." });
                        return;
                    }

                    await ApplyLocalConfigPayloadAsync(payload);
                    await WriteJsonResponseAsync(context.Response, 200, new { ok = true });
                    return;
                }

                await WriteJsonResponseAsync(context.Response, 404, new { ok = false, error = "Rota não encontrada." });
            }
            catch (Exception ex)
            {
                try
                {
                    ApplyCorsHeaders(context.Response);
                    await WriteJsonResponseAsync(context.Response, 500, new { ok = false, error = ex.Message });
                }
                catch
                {
                }
            }
        }

        private void ApplyCorsHeaders(HttpListenerResponse response)
        {
            response.Headers["Access-Control-Allow-Origin"] = "*";
            response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
            response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
        }

        private async Task WriteJsonResponseAsync(HttpListenerResponse response, int statusCode, object payload)
        {
            ApplyCorsHeaders(response);
            response.StatusCode = statusCode;
            response.ContentType = "application/json; charset=utf-8";
            string json = JsonConvert.SerializeObject(payload);
            byte[] data = Encoding.UTF8.GetBytes(json);
            await response.OutputStream.WriteAsync(data, 0, data.Length);
            response.OutputStream.Close();
        }

        private async Task ApplyLocalConfigPayloadAsync(LocalApplyConfigRequest payload)
        {
            await RunOnUiThreadAsync(async () =>
            {
                txtSupabaseUrl.Text = payload.ApiBaseUrl ?? txtSupabaseUrl.Text;
                txtUnitId.Text = payload.UnitId ?? txtUnitId.Text;
                txtKioskId.Text = payload.KioskId ?? txtKioskId.Text;

                if (!string.IsNullOrWhiteSpace(payload.RunMode) && cmbRunMode.Items.Contains(payload.RunMode))
                    cmbRunMode.SelectedItem = payload.RunMode;

                ApplyLocalProfileToUi(true, payload.NormalProfile);
                ApplyLocalProfileToUi(false, payload.StripProfile);

                RefreshPaperLists();
                SaveCurrentConfigToDisk();
                AddLog("Configuração recebida do site/localhost.");

                if (chkAutoStartListening.Checked)
                    await StartSelectedModeAsync();
            });
        }

        private void ApplyLocalProfileToUi(bool normalProfile, LocalProfileDto? profile)
        {
            if (profile == null)
                return;

            TextBox agentIdText = normalProfile ? txtNormalAgentId : txtStripAgentId;
            TextBox agentTokenText = normalProfile ? txtNormalToken : txtStripToken;
            ComboBox printerCombo = normalProfile ? cmbNormalPrinter : cmbStripPrinter;
            ComboBox paperCombo = normalProfile ? cmbNormalPaper : cmbStripPaper;
            ComboBox rotationCombo = normalProfile ? cmbNormalRotation : cmbStripRotation;
            NumericUpDown dpiControl = normalProfile ? nudNormalDpi : nudStripDpi;
            NumericUpDown bleedControl = normalProfile ? nudNormalBleed : nudStripBleed;
            NumericUpDown offsetXControl = normalProfile ? nudNormalOffsetX : nudStripOffsetX;
            NumericUpDown offsetYControl = normalProfile ? nudNormalOffsetY : nudStripOffsetY;

            if (!string.IsNullOrWhiteSpace(profile.AgentId))
                agentIdText.Text = profile.AgentId;
            if (!string.IsNullOrWhiteSpace(profile.AgentToken))
                agentTokenText.Text = profile.AgentToken;
            if (!string.IsNullOrWhiteSpace(profile.PrinterName))
                SelectComboIfExists(printerCombo, profile.PrinterName);

            RefreshPaperLists();

            if (!string.IsNullOrWhiteSpace(profile.PaperName))
                SelectComboIfExists(paperCombo, profile.PaperName);

            if (!string.IsNullOrWhiteSpace(profile.RotationMode) && rotationCombo.Items.Contains(profile.RotationMode))
                rotationCombo.SelectedItem = profile.RotationMode;

            dpiControl.Value = ClampNumeric(dpiControl, profile.Dpi ?? 300);
            bleedControl.Value = ClampNumeric(bleedControl, profile.Bleed ?? (normalProfile ? 8 : 0));
            offsetXControl.Value = ClampNumeric(offsetXControl, profile.OffsetX ?? 0);
            offsetYControl.Value = ClampNumeric(offsetYControl, profile.OffsetY ?? 0);
        }

        private Task RunOnUiThreadAsync(Func<Task> action)
        {
            var tcs = new TaskCompletionSource<bool>();

            void Execute()
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await action();
                        tcs.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.TrySetException(ex);
                    }
                });
            }

            if (InvokeRequired)
                BeginInvoke((Action)Execute);
            else
                Execute();

            return tcs.Task;
        }

        private class RemoteConfigResponse
        {
            [JsonProperty("ok")]
            public bool Ok { get; set; }

            [JsonProperty("error")]
            public string? Error { get; set; }

            [JsonProperty("config")]
            public RemoteProfileConfig? Config { get; set; }
        }

        private class RemoteProfileConfig
        {
            [JsonProperty("profile_type")]
            public string? ProfileType { get; set; }

            [JsonProperty("windows_printer_name")]
            public string? WindowsPrinterName { get; set; }

            [JsonProperty("paper_name")]
            public string? PaperName { get; set; }

            [JsonProperty("dpi")]
            public int? Dpi { get; set; }

            [JsonProperty("rotation_mode")]
            public string? RotationMode { get; set; }

            [JsonProperty("bleed")]
            public int? Bleed { get; set; }

            [JsonProperty("offset_x")]
            public int? OffsetX { get; set; }

            [JsonProperty("offset_y")]
            public int? OffsetY { get; set; }
        }

        private class LocalApplyConfigRequest
        {
            [JsonProperty("api_base_url")]
            public string? ApiBaseUrl { get; set; }

            [JsonProperty("unit_id")]
            public string? UnitId { get; set; }

            [JsonProperty("kiosk_id")]
            public string? KioskId { get; set; }

            [JsonProperty("run_mode")]
            public string? RunMode { get; set; }

            [JsonProperty("normal_profile")]
            public LocalProfileDto? NormalProfile { get; set; }

            [JsonProperty("strip_profile")]
            public LocalProfileDto? StripProfile { get; set; }
        }

        private class LocalProfileDto
        {
            [JsonProperty("agent_id")]
            public string? AgentId { get; set; }

            [JsonProperty("agent_token")]
            public string? AgentToken { get; set; }

            [JsonProperty("printer_name")]
            public string? PrinterName { get; set; }

            [JsonProperty("paper_name")]
            public string? PaperName { get; set; }

            [JsonProperty("dpi")]
            public int? Dpi { get; set; }

            [JsonProperty("rotation_mode")]
            public string? RotationMode { get; set; }

            [JsonProperty("bleed")]
            public int? Bleed { get; set; }

            [JsonProperty("offset_x")]
            public int? OffsetX { get; set; }

            [JsonProperty("offset_y")]
            public int? OffsetY { get; set; }
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
