using System.Drawing;
using System.Windows.Forms;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private static readonly Color UiBackground = Color.FromArgb(244, 247, 251);
        private static readonly Color UiSurface = Color.White;
        private static readonly Color UiBorder = Color.FromArgb(221, 227, 234);
        private static readonly Color UiPrimary = Color.FromArgb(30, 64, 175);
        private static readonly Color UiPrimaryHover = Color.FromArgb(30, 58, 138);
        private static readonly Color UiHeader = Color.FromArgb(15, 23, 42);
        private static readonly Color UiText = Color.FromArgb(30, 41, 59);
        private static readonly Color UiMuted = Color.FromArgb(100, 116, 139);
        private static readonly Color UiSuccess = Color.FromArgb(22, 163, 74);
        private static readonly Color UiDanger = Color.FromArgb(220, 38, 38);

        private TabControl? _modernTabs;

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
                MinimumSize = new Size(1080, 740);
                ClientSize = new Size(1180, 800);
                StartPosition = FormStartPosition.CenterScreen;
                Text = "CBL Print Assistant";

                Panel header = BuildHeader();
                _modernTabs = BuildWorkspaceTabs();

                Controls.Clear();
                Controls.Add(_modernTabs);
                Controls.Add(header);

                ApplyModernStyleRecursive(this);
                ConfigureProfileGroup(groupNormal, false);
                ConfigureProfileGroup(groupStrip, true);
                ConfigureLogList();
                RefreshOperationButtonLabels();

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
                Height = 78,
                BackColor = UiHeader,
                Padding = new Padding(26, 14, 26, 12)
            };

            var title = new Label
            {
                AutoSize = true,
                Text = "CBL Print Assistant",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 18F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(24, 12)
            };

            var subtitle = new Label
            {
                AutoSize = true,
                Text = "Central de impressão para eventos e totens",
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
                Location = new Point(27, 49)
            };

            var version = new Label
            {
                AutoSize = false,
                Width = 170,
                Height = 26,
                TextAlign = ContentAlignment.MiddleRight,
                Text = $"Versão {Application.ProductVersion}",
                ForeColor = Color.FromArgb(203, 213, 225),
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(header.Width - 194, 25)
            };
            header.Resize += (_, _) => version.Left = header.ClientSize.Width - version.Width - 24;

            header.Controls.Add(title);
            header.Controls.Add(subtitle);
            header.Controls.Add(version);
            return header;
        }

        private TabControl BuildWorkspaceTabs()
        {
            var tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI Semibold", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Point(18, 8),
                ItemSize = new Size(150, 38),
                SizeMode = TabSizeMode.Fixed
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
                Padding = new Padding(22),
                AutoScroll = true
            };
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
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            Panel commandCard = CreateCard(150);
            commandCard.Margin = new Padding(0, 0, 0, 18);
            AddSectionTitle(commandCard, "Operação", "Controle rápido da escuta e sincronização", 22, 18);

            lblRunMode.Text = "Modo de execução";
            lblRunMode.Location = new Point(24, 70);
            lblRunMode.AutoSize = true;
            cmbRunMode.Location = new Point(24, 92);
            cmbRunMode.Size = new Size(270, 28);

            btnStartAll.Location = new Point(320, 82);
            btnStartAll.Size = new Size(180, 42);
            btnStopAll.Location = new Point(510, 82);
            btnStopAll.Size = new Size(130, 42);
            btnSyncFromSite.Location = new Point(666, 82);
            btnSyncFromSite.Size = new Size(190, 42);
            btnCheckUpdates.Location = new Point(866, 82);
            btnCheckUpdates.Size = new Size(185, 42);

            commandCard.Controls.Add(lblRunMode);
            commandCard.Controls.Add(cmbRunMode);
            commandCard.Controls.Add(btnStartAll);
            commandCard.Controls.Add(btnStopAll);
            commandCard.Controls.Add(btnSyncFromSite);
            commandCard.Controls.Add(btnCheckUpdates);

            var summaries = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 208,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = UiBackground,
                Margin = new Padding(0, 0, 0, 18)
            };
            summaries.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            summaries.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            summaries.Controls.Add(BuildProfileSummaryCard("Foto Normal", lblNormalStatusDot, lblNormalStatusText, cmbNormalPrinter, cmbNormalPaper, lblNormalSystemPrinterValue), 0, 0);
            summaries.Controls.Add(BuildProfileSummaryCard("Tirinha", lblStripStatusDot, lblStripStatusText, cmbStripPrinter, cmbStripPaper, lblStripSystemPrinterValue), 1, 0);

            Panel hintCard = CreateCard(112);
            hintCard.Margin = new Padding(0);
            var hintTitle = new Label
            {
                Text = "Impressão local offline",
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 11F),
                ForeColor = UiText,
                Location = new Point(24, 20)
            };
            var hint = new Label
            {
                Text = "O companion local usa HTTPS na rede privada e o perfil Foto Normal. A fila é persistente e o jobId impede reimpressões duplicadas.",
                AutoSize = false,
                Height = 42,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = UiMuted,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(24, 49),
                Width = hintCard.Width - 48
            };
            hintCard.Resize += (_, _) => hint.Width = hintCard.ClientSize.Width - 48;
            hintCard.Controls.Add(hintTitle);
            hintCard.Controls.Add(hint);

            root.Controls.Add(commandCard, 0, 0);
            root.Controls.Add(summaries, 0, 1);
            root.Controls.Add(hintCard, 0, 2);
            page.Controls.Add(root);
        }

        private void BuildPrintersPage(TabPage page)
        {
            Panel toolbar = CreateCard(86);
            toolbar.Dock = DockStyle.Top;
            toolbar.Margin = new Padding(0, 0, 0, 18);
            AddSectionTitle(toolbar, "Impressoras", "Perfis físicos usados para impressão", 22, 16);

            btnLoadPrinters.Text = "Atualizar impressoras";
            btnLoadPrinters.Size = new Size(185, 38);
            btnLoadPrinters.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveConfig.Text = "Salvar configuração";
            btnSaveConfig.Size = new Size(185, 38);
            btnSaveConfig.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            toolbar.Controls.Add(btnLoadPrinters);
            toolbar.Controls.Add(btnSaveConfig);
            toolbar.Resize += (_, _) =>
            {
                btnSaveConfig.Location = new Point(toolbar.ClientSize.Width - 207, 24);
                btnLoadPrinters.Location = new Point(toolbar.ClientSize.Width - 402, 24);
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

            groupNormal.Text = "  Foto Normal  ";
            groupNormal.Dock = DockStyle.Fill;
            groupNormal.Margin = new Padding(0, 0, 10, 0);
            groupStrip.Text = "  Tirinha  ";
            groupStrip.Dock = DockStyle.Fill;
            groupStrip.Margin = new Padding(10, 0, 0, 0);
            profiles.Controls.Add(groupNormal, 0, 0);
            profiles.Controls.Add(groupStrip, 1, 0);

            page.Controls.Add(profiles);
            page.Controls.Add(toolbar);
        }

        private void BuildSystemPage(TabPage page)
        {
            Panel card = CreateCard(360);
            card.Dock = DockStyle.Top;
            AddSectionTitle(card, "Sistema", "Conexão, identificação do equipamento e inicialização", 24, 20);

            label1.Text = "Endpoint da API";
            label1.Location = new Point(26, 82);
            txtSupabaseUrl.Location = new Point(26, 104);
            txtSupabaseUrl.Size = new Size(650, 28);

            label2.Text = "Unit ID";
            label2.Location = new Point(26, 154);
            txtUnitId.Location = new Point(26, 176);
            txtUnitId.Size = new Size(310, 28);

            label3.Text = "Kiosk ID";
            label3.Location = new Point(364, 154);
            txtKioskId.Location = new Point(364, 176);
            txtKioskId.Size = new Size(312, 28);

            chkStartWithWindows.Location = new Point(26, 234);
            chkAutoStartListening.Location = new Point(26, 268);
            chkEnableLocalIntegration.Location = new Point(364, 268);

            var note = new Label
            {
                AutoSize = false,
                Text = "A integração local 127.0.0.1:38451 continua disponível para configuração. O companion de impressão offline usa um serviço HTTPS separado na porta 38452.",
                ForeColor = UiMuted,
                Font = new Font("Segoe UI", 9.25F),
                Location = new Point(26, 310),
                Height = 40,
                Width = 760
            };

            card.Controls.Add(label1);
            card.Controls.Add(txtSupabaseUrl);
            card.Controls.Add(label2);
            card.Controls.Add(txtUnitId);
            card.Controls.Add(label3);
            card.Controls.Add(txtKioskId);
            card.Controls.Add(chkStartWithWindows);
            card.Controls.Add(chkAutoStartListening);
            card.Controls.Add(chkEnableLocalIntegration);
            card.Controls.Add(note);
            page.Controls.Add(card);
        }

        private void BuildLogsPage(TabPage page)
        {
            Panel header = CreateCard(84);
            header.Dock = DockStyle.Top;
            AddSectionTitle(header, "Logs e diagnóstico", "Acompanhe conexões, jobs e falhas do agente", 22, 16);

            var clear = new Button
            {
                Text = "Limpar visualização",
                Size = new Size(160, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            clear.Click += (_, _) => listBox1.Items.Clear();
            header.Controls.Add(clear);
            header.Resize += (_, _) => clear.Location = new Point(header.ClientSize.Width - clear.Width - 22, 24);

            listBox1.Dock = DockStyle.Fill;
            listBox1.Margin = new Padding(0);

            page.Controls.Add(listBox1);
            page.Controls.Add(header);
        }

        private Panel BuildProfileSummaryCard(string title, Label sourceDot, Label sourceStatus, ComboBox sourcePrinter, ComboBox sourcePaper, Label sourceSystem)
        {
            Panel card = CreateCard(192);
            card.Dock = DockStyle.Fill;
            card.Margin = title == "Foto Normal" ? new Padding(0, 0, 10, 0) : new Padding(10, 0, 0, 0);

            var titleLabel = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = UiText,
                Location = new Point(22, 20)
            };
            var statusLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 9.5F),
                Location = new Point(22, 54)
            };
            var printerCaption = CreateSummaryCaption("Impressora", 22, 94);
            var printerValue = CreateSummaryValue(22, 115);
            var paperCaption = CreateSummaryCaption("Papel", 275, 94);
            var paperValue = CreateSummaryValue(275, 115);
            var systemCaption = CreateSummaryCaption("Totem / sistema", 22, 148);
            var systemValue = CreateSummaryValue(22, 168);

            card.Controls.Add(titleLabel);
            card.Controls.Add(statusLabel);
            card.Controls.Add(printerCaption);
            card.Controls.Add(printerValue);
            card.Controls.Add(paperCaption);
            card.Controls.Add(paperValue);
            card.Controls.Add(systemCaption);
            card.Controls.Add(systemValue);

            void Refresh()
            {
                statusLabel.Text = sourceStatus.Text;
                statusLabel.ForeColor = sourceStatus.Text.Contains("Ativ", StringComparison.OrdinalIgnoreCase) ? UiSuccess : UiMuted;
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

        private static Label CreateSummaryCaption(string text, int x, int y)
        {
            return new Label
            {
                Text = text.ToUpperInvariant(),
                AutoSize = true,
                ForeColor = UiMuted,
                Font = new Font("Segoe UI Semibold", 7.8F),
                Location = new Point(x, y)
            };
        }

        private static Label CreateSummaryValue(int x, int y)
        {
            return new Label
            {
                Text = "—",
                AutoEllipsis = true,
                ForeColor = UiText,
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(x, y),
                Size = new Size(225, 22)
            };
        }

        private static Panel CreateCard(int height)
        {
            return new Panel
            {
                Height = height,
                BackColor = UiSurface,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(0)
            };
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
            group.Font = new Font("Segoe UI Semibold", 10F);
            group.Padding = new Padding(16);
            group.Resize += (_, _) => LayoutProfileGroup(group, strip);
            LayoutProfileGroup(group, strip);
        }

        private void LayoutProfileGroup(GroupBox group, bool strip)
        {
            int width = Math.Max(group.ClientSize.Width, 430);
            int fieldWidth = Math.Max(width - 34, 360);
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
            statusDot.Location = new Point(width - 112, 21);
            statusText.Location = new Point(width - 88, 25);

            agentLabel.Location = new Point(16, 34);
            agent.Location = new Point(16, 54);
            agent.Size = new Size(145, 27);
            tokenLabel.Location = new Point(184, 34);
            token.Location = new Point(184, 54);
            token.Size = new Size(tokenWidth, 27);

            printerLabel.Location = new Point(16, 94);
            printer.Location = new Point(16, 114);
            printer.Size = new Size(fieldWidth, 28);

            paperLabel.Text = "Papel";
            paperLabel.Location = new Point(16, 151);
            paper.Location = new Point(16, 171);
            paper.Size = new Size(fieldWidth, 28);

            rotationLabel.Location = new Point(16, 208);
            rotation.Location = new Point(16, 228);
            rotation.Size = new Size(fieldWidth, 28);

            int numericWidth = Math.Max((fieldWidth - 42) / 4, 78);
            int x1 = 16;
            int x2 = x1 + numericWidth + 14;
            int x3 = x2 + numericWidth + 14;
            int x4 = x3 + numericWidth + 14;
            dpiLabel.Location = new Point(x1, 268);
            bleedLabel.Location = new Point(x2, 268);
            offsetXLabel.Location = new Point(x3, 268);
            offsetYLabel.Location = new Point(x4, 268);
            dpi.Location = new Point(x1, 288);
            bleed.Location = new Point(x2, 288);
            offsetX.Location = new Point(x3, 288);
            offsetY.Location = new Point(x4, 288);
            dpi.Size = new Size(numericWidth, 27);
            bleed.Size = new Size(numericWidth, 27);
            offsetX.Size = new Size(numericWidth, 27);
            offsetY.Size = new Size(numericWidth, 27);

            systemCaption.Location = new Point(16, 330);
            systemValue.Location = new Point(126, 330);
            systemValue.Size = new Size(Math.Max(width - 144, 240), 22);
        }

        private void ConfigureLogList()
        {
            listBox1.BackColor = Color.FromArgb(15, 23, 42);
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
                    case TabControl tab:
                        tab.BackColor = UiBackground;
                        break;

                    case GroupBox group:
                        group.BackColor = UiSurface;
                        group.ForeColor = UiText;
                        break;

                    case Button button:
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderSize = 0;
                        button.BackColor = IsPrimaryAction(button) ? UiPrimary : Color.FromArgb(226, 232, 240);
                        button.ForeColor = IsPrimaryAction(button) ? Color.White : UiText;
                        button.Cursor = Cursors.Hand;
                        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point);
                        button.FlatAppearance.MouseOverBackColor = IsPrimaryAction(button) ? UiPrimaryHover : Color.FromArgb(203, 213, 225);
                        break;

                    case TextBox textBox:
                        textBox.BackColor = UiSurface;
                        textBox.ForeColor = UiText;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        textBox.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case ComboBox combo:
                        combo.BackColor = UiSurface;
                        combo.ForeColor = UiText;
                        combo.FlatStyle = FlatStyle.Flat;
                        combo.Font = new Font("Segoe UI", 9.25F);
                        break;

                    case NumericUpDown numeric:
                        numeric.BackColor = UiSurface;
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
                        if (!label.Name.EndsWith("StatusDot", StringComparison.OrdinalIgnoreCase) &&
                            !label.Name.EndsWith("StatusText", StringComparison.OrdinalIgnoreCase))
                        {
                            label.ForeColor = label.ForeColor == Color.White ? Color.White : UiText;
                        }
                        label.BackColor = Color.Transparent;
                        break;
                }

                if (control.HasChildren)
                    ApplyModernStyleRecursive(control);
            }
        }

        private static bool IsPrimaryAction(Button button)
        {
            return button.Name is "btnStartAll" or "btnSaveConfig" or "btnSyncFromSite";
        }
    }
}
