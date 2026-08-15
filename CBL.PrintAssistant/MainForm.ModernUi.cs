using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private static readonly Color UiBackground = Color.FromArgb(244, 247, 251);
        private static readonly Color UiSurface = Color.White;
        private static readonly Color UiSurfaceSoft = Color.FromArgb(248, 250, 252);
        private static readonly Color UiBorder = Color.FromArgb(226, 232, 240);
        private static readonly Color UiPrimary = Color.FromArgb(37, 99, 235);
        private static readonly Color UiPrimaryHover = Color.FromArgb(29, 78, 216);
        private static readonly Color UiPrimarySoft = Color.FromArgb(239, 246, 255);
        private static readonly Color UiHeader = Color.FromArgb(15, 23, 42);
        private static readonly Color UiHeaderEnd = Color.FromArgb(23, 37, 68);
        private static readonly Color UiText = Color.FromArgb(30, 41, 59);
        private static readonly Color UiMuted = Color.FromArgb(100, 116, 139);
        private static readonly Color UiSuccess = Color.FromArgb(22, 163, 74);
        private static readonly Color UiSuccessSoft = Color.FromArgb(240, 253, 244);
        private static readonly Color UiWarning = Color.FromArgb(217, 119, 6);
        private static readonly Color UiDanger = Color.FromArgb(220, 38, 38);
        private static readonly Color UiLog = Color.FromArgb(15, 23, 42);

        private TabControl? _modernTabs;
        private Button[]? _modernNavButtons;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplyModernUi();
        }

        private void ApplyModernUi()
        {
            SuspendLayout();
            try
            {
                BackColor = UiBackground;
                Font = new Font("Segoe UI", 9.25F, FontStyle.Regular, GraphicsUnit.Point);
                FormBorderStyle = FormBorderStyle.Sizable;
                MaximizeBox = true;
                MinimumSize = new Size(1180, 760);
                ClientSize = new Size(1240, 820);
                StartPosition = FormStartPosition.CenterScreen;
                Text = "CBL Print Assistant";

                Panel header = BuildHeader();
                Panel workspace = BuildWorkspaceShell();

                Controls.Clear();
                Controls.Add(workspace);
                Controls.Add(header);

                ApplyModernStyleRecursive(this);
                ConfigureProfileGroup(groupNormal, false);
                ConfigureProfileGroup(groupStrip, true);
                ConfigureLogList();
                RefreshOperationButtonLabels();
                UpdateNavigationState();

                cmbRunMode.SelectedIndexChanged += (_, _) => RefreshOperationButtonLabels();
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private Panel BuildHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 96,
                BackColor = UiHeader,
                Padding = new Padding(28, 16, 28, 14)
            };
            header.Paint += (_, e) =>
            {
                using var brush = new LinearGradientBrush(header.ClientRectangle, UiHeader, UiHeaderEnd, 0F);
                e.Graphics.FillRectangle(brush, header.ClientRectangle);
            };

            var logo = new Panel
            {
                Size = new Size(48, 48),
                Location = new Point(28, 22),
                BackColor = UiPrimary
            };
            RoundControl(logo, 14);
            logo.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = "CBL",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
            });

            var title = new Label
            {
                AutoSize = true,
                Text = "Print Assistant",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 19F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(92, 19)
            };

            var subtitle = new Label
            {
                AutoSize = true,
                Text = "Central de impressão para eventos e totens",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(95, 57)
            };

            var versionBadge = new Panel
            {
                Size = new Size(112, 34),
                BackColor = Color.FromArgb(30, 41, 59),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(header.Width - 140, 31)
            };
            RoundControl(versionBadge, 17);
            versionBadge.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = $"v{Application.ProductVersion}",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(226, 232, 240),
                Font = new Font("Segoe UI Semibold", 9F)
            });
            header.Resize += (_, _) => versionBadge.Left = header.ClientSize.Width - versionBadge.Width - 28;

            header.Controls.Add(logo);
            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(versionBadge);
            return header;
        }

        private Panel BuildWorkspaceShell()
        {
            var shell = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiBackground
            };

            var navHost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = UiSurface,
                Padding = new Padding(24, 10, 24, 10)
            };

            var navFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiSurface,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            string[] names = { "Operação", "Impressoras", "Sistema", "Logs" };
            _modernNavButtons = new Button[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                var button = new Button
                {
                    Text = names[i],
                    Name = $"nav{names[i]}",
                    Tag = "nav",
                    Size = new Size(i == 1 ? 154 : 132, 42),
                    Margin = new Padding(0, 0, 8, 0),
                    FlatStyle = FlatStyle.Flat,
                    FlatAppearance = { BorderSize = 0 },
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI Semibold", 9.5F),
                    BackColor = UiSurface,
                    ForeColor = UiMuted
                };
                RoundControl(button, 10);
                button.Click += (_, _) => SelectWorkspace(index);
                _modernNavButtons[i] = button;
                navFlow.Controls.Add(button);
            }

            var divider = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = UiBorder
            };
            navHost.Controls.Add(navFlow);
            navHost.Controls.Add(divider);

            _modernTabs = BuildWorkspaceTabs();
            _modernTabs.SelectedIndexChanged += (_, _) => UpdateNavigationState();

            shell.Controls.Add(_modernTabs);
            shell.Controls.Add(navHost);
            return shell;
        }

        private TabControl BuildWorkspaceTabs()
        {
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.FlatButtons,
                ItemSize = new Size(1, 1),
                SizeMode = TabSizeMode.Fixed,
                Padding = new Point(0, 0),
                BackColor = UiBackground
            };

            var operation = CreateTabPage("Operação");
            var printers = CreateTabPage("Impressoras");
            var system = CreateTabPage("Sistema");
            var logs = CreateTabPage("Logs");

            BuildOperationPage(operation);
            BuildPrintersPage(printers);
            BuildSystemPage(system);
            BuildLogsPage(logs);

            tabs.TabPages.Add(operation);
            tabs.TabPages.Add(printers);
            tabs.TabPages.Add(system);
            tabs.TabPages.Add(logs);
            return tabs;
        }

        private static TabPage CreateTabPage(string text)
        {
            return new TabPage
            {
                Text = text,
                BackColor = UiBackground,
                Padding = new Padding(24, 22, 24, 24),
                AutoScroll = true
            };
        }

        private void SelectWorkspace(int index)
        {
            if (_modernTabs == null || index < 0 || index >= _modernTabs.TabPages.Count)
                return;

            _modernTabs.SelectedIndex = index;
            UpdateNavigationState();
        }

        private void UpdateNavigationState()
        {
            if (_modernTabs == null || _modernNavButtons == null)
                return;

            for (int i = 0; i < _modernNavButtons.Length; i++)
            {
                bool active = i == _modernTabs.SelectedIndex;
                Button button = _modernNavButtons[i];
                button.BackColor = active ? UiPrimarySoft : UiSurface;
                button.ForeColor = active ? UiPrimary : UiMuted;
                button.FlatAppearance.MouseOverBackColor = active ? UiPrimarySoft : UiSurfaceSoft;
            }
        }

        private void BuildOperationPage(TabPage page)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = UiBackground,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Panel commandCard = CreateCard(164);
            commandCard.Dock = DockStyle.Fill;
            commandCard.Margin = new Padding(0, 0, 0, 18);
            AddSectionTitle(commandCard, "Operação", "Controle rápido da escuta, sincronização e modo ativo", 24, 20);

            lblRunMode.Text = "Modo de execução";
            lblRunMode.Location = new Point(26, 78);
            lblRunMode.AutoSize = true;
            cmbRunMode.Location = new Point(26, 101);
            cmbRunMode.Size = new Size(270, 30);

            btnStartAll.Size = new Size(184, 44);
            btnStopAll.Size = new Size(128, 44);
            btnSyncFromSite.Size = new Size(188, 44);
            btnCheckUpdates.Size = new Size(176, 44);
            btnStartAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnStopAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSyncFromSite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckUpdates.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            void LayoutCommands()
            {
                int right = commandCard.ClientSize.Width - 24;
                Button[] buttons = { btnCheckUpdates, btnSyncFromSite, btnStopAll, btnStartAll };
                foreach (Button button in buttons)
                {
                    right -= button.Width;
                    button.Location = new Point(right, 94);
                    right -= 10;
                }
            }
            commandCard.Resize += (_, _) => LayoutCommands();

            commandCard.Controls.Add(lblRunMode);
            commandCard.Controls.Add(cmbRunMode);
            commandCard.Controls.Add(btnStartAll);
            commandCard.Controls.Add(btnStopAll);
            commandCard.Controls.Add(btnSyncFromSite);
            commandCard.Controls.Add(btnCheckUpdates);
            LayoutCommands();

            var summaries = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 226,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiBackground,
                Margin = new Padding(0, 0, 0, 18)
            };
            summaries.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            summaries.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            summaries.Controls.Add(BuildProfileSummaryCard("Foto Normal", lblNormalStatusDot, lblNormalStatusText, cmbNormalPrinter, cmbNormalPaper, lblNormalSystemPrinterValue), 0, 0);
            summaries.Controls.Add(BuildProfileSummaryCard("Tirinha", lblStripStatusDot, lblStripStatusText, cmbStripPrinter, cmbStripPaper, lblStripSystemPrinterValue), 1, 0);

            Panel localCard = CreateCard(144);
            localCard.Dock = DockStyle.Fill;
            localCard.Margin = new Padding(0);
            AddSectionTitle(localCard, "Impressão local offline", "Canal dedicado para imprimir mesmo sem acesso à internet", 24, 18);

            var protocolBadge = CreatePill("HTTPS  •  porta 38452", UiPrimarySoft, UiPrimary, 180);
            protocolBadge.Location = new Point(24, 83);
            localCard.Controls.Add(protocolBadge);

            var queueBadge = CreatePill("Fila persistente", UiSuccessSoft, UiSuccess, 138);
            queueBadge.Location = new Point(216, 83);
            localCard.Controls.Add(queueBadge);

            var dedupeBadge = CreatePill("Anti-duplicação por jobId", Color.FromArgb(255, 247, 237), UiWarning, 188);
            dedupeBadge.Location = new Point(366, 83);
            localCard.Controls.Add(dedupeBadge);

            var goPrinters = new Button
            {
                Text = "Configurar impressoras",
                Size = new Size(190, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(localCard.Width - 214, 78)
            };
            goPrinters.Click += (_, _) => SelectWorkspace(1);
            localCard.Controls.Add(goPrinters);
            localCard.Resize += (_, _) => goPrinters.Left = localCard.ClientSize.Width - goPrinters.Width - 24;

            root.Controls.Add(commandCard, 0, 0);
            root.Controls.Add(summaries, 0, 1);
            root.Controls.Add(localCard, 0, 2);
            page.Controls.Add(root);
        }

        private void BuildPrintersPage(TabPage page)
        {
            Panel toolbar = CreateCard(96);
            toolbar.Dock = DockStyle.Top;
            toolbar.Margin = new Padding(0, 0, 0, 18);
            AddSectionTitle(toolbar, "Impressoras", "Perfis físicos usados pelo motor profissional de impressão", 24, 18);

            btnLoadPrinters.Text = "Atualizar impressoras";
            btnLoadPrinters.Size = new Size(184, 40);
            btnLoadPrinters.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveConfig.Text = "Salvar configuração";
            btnSaveConfig.Size = new Size(184, 40);
            btnSaveConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolbar.Controls.Add(btnLoadPrinters);
            toolbar.Controls.Add(btnSaveConfig);
            toolbar.Resize += (_, _) =>
            {
                btnSaveConfig.Location = new Point(toolbar.ClientSize.Width - 208, 28);
                btnLoadPrinters.Location = new Point(toolbar.ClientSize.Width - 402, 28);
            };

            var profiles = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiBackground,
                Padding = new Padding(0, 18, 0, 0)
            };
            profiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            profiles.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            profiles.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            profiles.Controls.Add(BuildPrinterProfileCard("Foto Normal", "Perfil principal para totens e impressão offline", groupNormal, false), 0, 0);
            profiles.Controls.Add(BuildPrinterProfileCard("Tirinha", "Perfil dedicado para experiências em formato tirinha", groupStrip, true), 1, 0);

            page.Controls.Add(profiles);
            page.Controls.Add(toolbar);
        }

        private Panel BuildPrinterProfileCard(string title, string subtitle, GroupBox group, bool strip)
        {
            Panel card = CreateCard(470);
            card.Dock = DockStyle.Fill;
            card.Margin = strip ? new Padding(10, 0, 0, 0) : new Padding(0, 0, 10, 0);
            card.Padding = new Padding(20, 66, 20, 18);

            card.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = UiText,
                Location = new Point(22, 17)
            });
            card.Controls.Add(new Label
            {
                Text = subtitle,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.75F),
                ForeColor = UiMuted,
                Location = new Point(24, 44)
            });

            group.Text = "";
            group.Dock = DockStyle.Fill;
            group.Margin = new Padding(0);
            group.Padding = new Padding(0);
            group.FlatStyle = FlatStyle.Flat;
            group.Paint += (_, e) =>
            {
                using var brush = new SolidBrush(UiSurface);
                e.Graphics.FillRectangle(brush, group.ClientRectangle);
            };
            card.Controls.Add(group);
            group.BringToFront();
            return card;
        }

        private void BuildSystemPage(TabPage page)
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 430,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiBackground,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

            Panel connectionCard = CreateCard(410);
            connectionCard.Dock = DockStyle.Fill;
            connectionCard.Margin = new Padding(0, 0, 10, 0);
            AddSectionTitle(connectionCard, "Conexão e identificação", "Dados usados para conectar este equipamento ao ambiente KBINE", 24, 20);

            label1.Text = "Endpoint da API";
            label1.Location = new Point(26, 86);
            txtSupabaseUrl.Location = new Point(26, 109);
            txtSupabaseUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtSupabaseUrl.Size = new Size(connectionCard.Width - 52, 30);

            label2.Text = "Unit ID";
            label2.Location = new Point(26, 164);
            txtUnitId.Location = new Point(26, 187);
            txtUnitId.Size = new Size(290, 30);

            label3.Text = "Kiosk ID";
            label3.Location = new Point(342, 164);
            txtKioskId.Location = new Point(342, 187);
            txtKioskId.Size = new Size(290, 30);

            connectionCard.Resize += (_, _) =>
            {
                txtSupabaseUrl.Width = connectionCard.ClientSize.Width - 52;
                int half = Math.Max((connectionCard.ClientSize.Width - 78) / 2, 220);
                txtUnitId.Width = half;
                label3.Left = 26 + half + 26;
                txtKioskId.Left = label3.Left;
                txtKioskId.Width = half;
            };

            var securityNote = new Label
            {
                AutoSize = false,
                Text = "Credenciais de agente permanecem protegidas e são configuradas na área Impressoras.",
                ForeColor = UiMuted,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(26, 252),
                Height = 44,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            connectionCard.Resize += (_, _) => securityNote.Width = connectionCard.ClientSize.Width - 52;

            connectionCard.Controls.Add(label1);
            connectionCard.Controls.Add(txtSupabaseUrl);
            connectionCard.Controls.Add(label2);
            connectionCard.Controls.Add(txtUnitId);
            connectionCard.Controls.Add(label3);
            connectionCard.Controls.Add(txtKioskId);
            connectionCard.Controls.Add(securityNote);

            Panel behaviorCard = CreateCard(410);
            behaviorCard.Dock = DockStyle.Fill;
            behaviorCard.Margin = new Padding(10, 0, 0, 0);
            AddSectionTitle(behaviorCard, "Inicialização e integração", "Comportamento deste computador durante os eventos", 24, 20);

            chkStartWithWindows.Location = new Point(26, 92);
            chkAutoStartListening.Location = new Point(26, 132);
            chkEnableLocalIntegration.Location = new Point(26, 172);

            var localTitle = new Label
            {
                Text = "Companion offline",
                AutoSize = true,
                ForeColor = UiText,
                Font = new Font("Segoe UI Semibold", 10F),
                Location = new Point(26, 228)
            };
            var localInfo = new Label
            {
                AutoSize = false,
                Text = "Configuração local: 127.0.0.1:38451\nImpressão offline HTTPS: porta 38452",
                ForeColor = UiMuted,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(26, 254),
                Height = 48,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            behaviorCard.Resize += (_, _) => localInfo.Width = behaviorCard.ClientSize.Width - 52;

            var saveSystem = new Button
            {
                Name = "btnSaveSystemUi",
                Text = "Salvar configuração",
                Size = new Size(186, 40),
                Location = new Point(26, 330)
            };
            saveSystem.Click += (sender, e) => btnSaveConfig_Click(sender ?? this, e);

            behaviorCard.Controls.Add(chkStartWithWindows);
            behaviorCard.Controls.Add(chkAutoStartListening);
            behaviorCard.Controls.Add(chkEnableLocalIntegration);
            behaviorCard.Controls.Add(localTitle);
            behaviorCard.Controls.Add(localInfo);
            behaviorCard.Controls.Add(saveSystem);

            root.Controls.Add(connectionCard, 0, 0);
            root.Controls.Add(behaviorCard, 1, 0);
            page.Controls.Add(root);
        }

        private void BuildLogsPage(TabPage page)
        {
            var root = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiBackground
            };

            Panel header = CreateCard(96);
            header.Dock = DockStyle.Top;
            AddSectionTitle(header, "Logs e diagnóstico", "Acompanhe conexões, jobs, impressão local e falhas do agente", 24, 18);

            var copy = new Button
            {
                Text = "Copiar logs",
                Size = new Size(132, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            copy.Click += (_, _) =>
            {
                if (listBox1.Items.Count == 0)
                    return;
                string text = string.Join(Environment.NewLine, listBox1.Items.Cast<object>().Select(x => x?.ToString() ?? ""));
                Clipboard.SetText(text);
            };

            var clear = new Button
            {
                Text = "Limpar visualização",
                Size = new Size(166, 38),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            clear.Click += (_, _) => listBox1.Items.Clear();
            header.Controls.Add(copy);
            header.Controls.Add(clear);
            header.Resize += (_, _) =>
            {
                clear.Location = new Point(header.ClientSize.Width - clear.Width - 24, 29);
                copy.Location = new Point(clear.Left - copy.Width - 10, 29);
            };

            Panel logCard = CreateCard(420);
            logCard.Dock = DockStyle.Fill;
            logCard.Padding = new Padding(12);
            logCard.Margin = new Padding(0, 18, 0, 0);
            logCard.BackColor = UiLog;
            listBox1.Dock = DockStyle.Fill;
            listBox1.Margin = new Padding(0);
            logCard.Controls.Add(listBox1);

            root.Controls.Add(logCard);
            root.Controls.Add(header);
            page.Controls.Add(root);
        }

        private Panel BuildProfileSummaryCard(string title, Label sourceDot, Label sourceStatus, ComboBox sourcePrinter, ComboBox sourcePaper, Label sourceSystem)
        {
            Panel card = CreateCard(216);
            card.Dock = DockStyle.Fill;
            card.Margin = title == "Foto Normal" ? new Padding(0, 0, 10, 0) : new Padding(10, 0, 0, 0);

            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13.5F),
                ForeColor = UiText,
                Location = new Point(22, 20)
            };

            var statusBadge = new Label
            {
                AutoSize = false,
                Size = new Size(86, 28),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Semibold", 8.75F),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(card.Width - 108, 18)
            };
            RoundControl(statusBadge, 14);
            card.Resize += (_, _) => statusBadge.Left = card.ClientSize.Width - statusBadge.Width - 22;

            var grid = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 2,
                BackColor = UiSurface,
                Location = new Point(18, 72),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(card.Width - 36, 124),
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            card.Resize += (_, _) => grid.Width = card.ClientSize.Width - 36;

            Panel printerField = BuildInfoField("Impressora", out Label printerValue);
            Panel paperField = BuildInfoField("Papel", out Label paperValue);
            Panel systemField = BuildInfoField("Totem / sistema", out Label systemValue);
            printerField.Margin = new Padding(0, 0, 8, 8);
            paperField.Margin = new Padding(8, 0, 0, 8);
            systemField.Margin = new Padding(0, 0, 0, 0);
            grid.Controls.Add(printerField, 0, 0);
            grid.Controls.Add(paperField, 1, 0);
            grid.Controls.Add(systemField, 0, 1);
            grid.SetColumnSpan(systemField, 2);

            card.Controls.Add(titleLabel);
            card.Controls.Add(statusBadge);
            card.Controls.Add(grid);

            void Refresh()
            {
                bool active = sourceStatus.Text.Contains("Ativ", StringComparison.OrdinalIgnoreCase);
                statusBadge.Text = active ? "●  Ativo" : "●  Inativo";
                statusBadge.ForeColor = active ? UiSuccess : UiMuted;
                statusBadge.BackColor = active ? UiSuccessSoft : UiSurfaceSoft;
                printerValue.Text = string.IsNullOrWhiteSpace(sourcePrinter.Text) ? "Não configurada" : sourcePrinter.Text;
                paperValue.Text = string.IsNullOrWhiteSpace(sourcePaper.Text) ? "Não configurado" : sourcePaper.Text;
                systemValue.Text = string.IsNullOrWhiteSpace(sourceSystem.Text) ? "—" : sourceSystem.Text;
            }

            sourceStatus.TextChanged += (_, _) => Refresh();
            sourceDot.ForeColorChanged += (_, _) => Refresh();
            sourcePrinter.SelectedIndexChanged += (_, _) => Refresh();
            sourcePrinter.TextChanged += (_, _) => Refresh();
            sourcePaper.SelectedIndexChanged += (_, _) => Refresh();
            sourcePaper.TextChanged += (_, _) => Refresh();
            sourceSystem.TextChanged += (_, _) => Refresh();
            Refresh();
            return card;
        }

        private static Panel BuildInfoField(string caption, out Label value)
        {
            var field = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiSurfaceSoft,
                Padding = new Padding(14, 10, 14, 8)
            };
            RoundControl(field, 10);
            field.Controls.Add(new Label
            {
                Text = caption.ToUpperInvariant(),
                AutoSize = true,
                ForeColor = UiMuted,
                Font = new Font("Segoe UI Semibold", 7.6F),
                Location = new Point(13, 8)
            });
            value = new Label
            {
                Text = "—",
                AutoEllipsis = true,
                ForeColor = UiText,
                Font = new Font("Segoe UI Semibold", 9.25F),
                Location = new Point(13, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(190, 24)
            };
            field.Controls.Add(value);
            field.Resize += (_, _) => value.Width = field.ClientSize.Width - 26;
            return field;
        }

        private static Label CreatePill(string text, Color background, Color foreground, int width)
        {
            var label = new Label
            {
                Text = text,
                AutoSize = false,
                Size = new Size(width, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = background,
                ForeColor = foreground,
                Font = new Font("Segoe UI Semibold", 8.5F)
            };
            RoundControl(label, 15);
            return label;
        }

        private static Panel CreateCard(int height)
        {
            var panel = new Panel
            {
                Height = height,
                BackColor = UiSurface,
                Padding = new Padding(0)
            };
            StyleCard(panel);
            return panel;
        }

        private static void StyleCard(Panel panel)
        {
            void RefreshRegion() => SetRoundedRegion(panel, 16);
            panel.Resize += (_, _) => RefreshRegion();
            panel.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Rectangle rect = new Rectangle(0, 0, Math.Max(panel.Width - 1, 1), Math.Max(panel.Height - 1, 1));
                using GraphicsPath path = RoundedRect(rect, 16);
                using var pen = new Pen(UiBorder, 1F);
                e.Graphics.DrawPath(pen, path);
            };
            RefreshRegion();
        }

        private static void AddSectionTitle(Control parent, string title, string subtitle, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = UiText,
                Location = new Point(x, y)
            });
            parent.Controls.Add(new Label
            {
                Text = subtitle,
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = UiMuted,
                Location = new Point(x, y + 31)
            });
        }

        private void ConfigureProfileGroup(GroupBox group, bool strip)
        {
            group.BackColor = UiSurface;
            group.ForeColor = UiText;
            group.Font = new Font("Segoe UI", 9.25F);
            group.Padding = new Padding(0);
            group.Resize += (_, _) => LayoutProfileGroup(group, strip);
            LayoutProfileGroup(group, strip);
        }

        private void LayoutProfileGroup(GroupBox group, bool strip)
        {
            int width = Math.Max(group.ClientSize.Width, 430);
            int fieldWidth = Math.Max(width - 18, 360);
            int tokenWidth = Math.Max(fieldWidth - 168, 210);

            Label agentLabel = strip ? label26 : label9;
            Label tokenLabel = strip ? label25 : label10;
            Label printerLabel = strip ? label24 : label11;
            Label paperLabel = strip ? label30 : label28;
            Label rotationLabel = strip ? label23 : label12;
            Label dpiLabel = strip ? label29 : label27;
            Label bleedLabel = strip ? label22 : label13;
            Label offsetXLabel = strip ? label21 : label14;
            Label offsetYLabel = strip ? label20 : label15;
            Label statusCaption = strip ? label19 : label16;
            Label systemCaption = strip ? label18 : label17;
            Label statusDot = strip ? lblStripStatusDot : lblNormalStatusDot;
            Label statusText = strip ? lblStripStatusText : lblNormalStatusText;
            Label systemValue = strip ? lblStripSystemPrinterValue : lblNormalSystemPrinterValue;
            TextBox agent = strip ? txtStripAgentId : txtNormalAgentId;
            TextBox token = strip ? txtStripToken : txtNormalToken;
            ComboBox printer = strip ? cmbStripPrinter : cmbNormalPrinter;
            ComboBox paper = strip ? cmbStripPaper : cmbNormalPaper;
            ComboBox rotation = strip ? cmbStripRotation : cmbNormalRotation;
            NumericUpDown dpi = strip ? nudStripDpi : nudNormalDpi;
            NumericUpDown bleed = strip ? nudStripBleed : nudNormalBleed;
            NumericUpDown offsetX = strip ? nudStripOffsetX : nudNormalOffsetX;
            NumericUpDown offsetY = strip ? nudStripOffsetY : nudNormalOffsetY;

            statusCaption.Visible = false;
            statusDot.Location = new Point(width - 108, 5);
            statusText.Location = new Point(width - 84, 9);

            agentLabel.Text = "Agent ID";
            tokenLabel.Text = "Agent Token";
            agentLabel.Location = new Point(8, 14);
            agent.Location = new Point(8, 36);
            agent.Size = new Size(145, 29);
            tokenLabel.Location = new Point(176, 14);
            token.Location = new Point(176, 36);
            token.Size = new Size(tokenWidth, 29);

            printerLabel.Location = new Point(8, 82);
            printer.Location = new Point(8, 104);
            printer.Size = new Size(fieldWidth, 30);

            paperLabel.Text = "Papel";
            paperLabel.Location = new Point(8, 147);
            paper.Location = new Point(8, 169);
            paper.Size = new Size(fieldWidth, 30);

            rotationLabel.Location = new Point(8, 212);
            rotation.Location = new Point(8, 234);
            rotation.Size = new Size(fieldWidth, 30);

            int numericWidth = Math.Max((fieldWidth - 42) / 4, 78);
            int x1 = 8;
            int x2 = x1 + numericWidth + 14;
            int x3 = x2 + numericWidth + 14;
            int x4 = x3 + numericWidth + 14;
            dpiLabel.Location = new Point(x1, 278);
            bleedLabel.Location = new Point(x2, 278);
            offsetXLabel.Location = new Point(x3, 278);
            offsetYLabel.Location = new Point(x4, 278);
            dpi.Location = new Point(x1, 300);
            bleed.Location = new Point(x2, 300);
            offsetX.Location = new Point(x3, 300);
            offsetY.Location = new Point(x4, 300);
            dpi.Size = new Size(numericWidth, 29);
            bleed.Size = new Size(numericWidth, 29);
            offsetX.Size = new Size(numericWidth, 29);
            offsetY.Size = new Size(numericWidth, 29);

            systemCaption.Text = "Totem / Sistema";
            systemCaption.Location = new Point(8, 347);
            systemValue.Location = new Point(116, 347);
            systemValue.Size = new Size(Math.Max(width - 126, 240), 24);
        }

        private void ConfigureLogList()
        {
            listBox1.BackColor = UiLog;
            listBox1.ForeColor = Color.FromArgb(226, 232, 240);
            listBox1.BorderStyle = BorderStyle.None;
            listBox1.Font = new Font("Cascadia Mono", 9F, FontStyle.Regular, GraphicsUnit.Point);
            listBox1.IntegralHeight = false;
        }

        private void RefreshOperationButtonLabels()
        {
            btnStartAll.Text = GetSelectedRunMode() switch
            {
                ProfileStrip => "▶  Iniciar Tirinha",
                ModeBoth => "▶  Iniciar Ambos",
                _ => "▶  Iniciar Normal"
            };
            btnStopAll.Text = "■  Parar";
            btnSyncFromSite.Text = "Sincronizar do site";
            btnCheckUpdates.Text = "Verificar atualização";
        }

        private void ApplyModernStyleRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                switch (control)
                {
                    case Button button when Equals(button.Tag, "nav"):
                        break;

                    case Button button:
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderSize = 0;
                        button.BackColor = IsPrimaryAction(button) ? UiPrimary : Color.FromArgb(241, 245, 249);
                        button.ForeColor = IsPrimaryAction(button) ? Color.White : UiText;
                        button.Cursor = Cursors.Hand;
                        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point);
                        button.FlatAppearance.MouseOverBackColor = IsPrimaryAction(button) ? UiPrimaryHover : Color.FromArgb(226, 232, 240);
                        RoundControl(button, 10);
                        break;

                    case TextBox textBox:
                        textBox.BackColor = UiSurfaceSoft;
                        textBox.ForeColor = UiText;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case ComboBox combo:
                        combo.BackColor = UiSurfaceSoft;
                        combo.ForeColor = UiText;
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case NumericUpDown numeric:
                        numeric.BackColor = UiSurfaceSoft;
                        numeric.ForeColor = UiText;
                        numeric.BorderStyle = BorderStyle.FixedSingle;
                        numeric.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case CheckBox check:
                        check.ForeColor = UiText;
                        check.UseVisualStyleBackColor = false;
                        check.BackColor = UiSurface;
                        check.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case Label label:
                        if (label.BackColor == SystemColors.Control)
                            label.BackColor = Color.Transparent;
                        break;
                }

                if (control.HasChildren)
                    ApplyModernStyleRecursive(control);
            }
        }

        private static bool IsPrimaryAction(Button button)
        {
            return button.Name is "btnStartAll" or "btnSaveConfig" or "btnSyncFromSite" or "btnSaveSystemUi";
        }

        private static void RoundControl(Control control, int radius)
        {
            void Refresh() => SetRoundedRegion(control, radius);
            control.Resize += (_, _) => Refresh();
            Refresh();
        }

        private static void SetRoundedRegion(Control control, int radius)
        {
            if (control.Width <= 0 || control.Height <= 0)
                return;
            Rectangle rect = new Rectangle(0, 0, control.Width, control.Height);
            using GraphicsPath path = RoundedRect(rect, radius);
            control.Region?.Dispose();
            control.Region = new Region(path);
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = Math.Max(radius * 2, 2);
            var path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
