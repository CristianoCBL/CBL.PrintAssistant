namespace CBL.PrintAssistant
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            label1 = new Label();
            txtSupabaseUrl = new TextBox();
            label2 = new Label();
            txtUnitId = new TextBox();
            label3 = new Label();
            txtKioskId = new TextBox();
            chkStartWithWindows = new CheckBox();
            chkAutoStartListening = new CheckBox();
            chkEnableLocalIntegration = new CheckBox();
            btnLoadPrinters = new Button();
            btnSaveConfig = new Button();
            btnStartAll = new Button();
            btnStopAll = new Button();
            btnCheckUpdates = new Button();
            btnSyncFromSite = new Button();
            lblRunMode = new Label();
            cmbRunMode = new ComboBox();
            groupNormal = new GroupBox();
            lblNormalSystemPrinterValue = new Label();
            label17 = new Label();
            lblNormalStatusText = new Label();
            lblNormalStatusDot = new Label();
            label16 = new Label();
            nudNormalOffsetY = new NumericUpDown();
            label15 = new Label();
            nudNormalOffsetX = new NumericUpDown();
            label14 = new Label();
            nudNormalBleed = new NumericUpDown();
            label13 = new Label();
            nudNormalDpi = new NumericUpDown();
            label27 = new Label();
            cmbNormalRotation = new ComboBox();
            label12 = new Label();
            cmbNormalPaper = new ComboBox();
            label28 = new Label();
            cmbNormalPrinter = new ComboBox();
            label11 = new Label();
            txtNormalToken = new TextBox();
            label10 = new Label();
            txtNormalAgentId = new TextBox();
            label9 = new Label();
            groupStrip = new GroupBox();
            lblStripSystemPrinterValue = new Label();
            label18 = new Label();
            lblStripStatusText = new Label();
            lblStripStatusDot = new Label();
            label19 = new Label();
            nudStripOffsetY = new NumericUpDown();
            label20 = new Label();
            nudStripOffsetX = new NumericUpDown();
            label21 = new Label();
            nudStripBleed = new NumericUpDown();
            label22 = new Label();
            nudStripDpi = new NumericUpDown();
            label29 = new Label();
            cmbStripRotation = new ComboBox();
            label23 = new Label();
            cmbStripPaper = new ComboBox();
            label30 = new Label();
            cmbStripPrinter = new ComboBox();
            label24 = new Label();
            txtStripToken = new TextBox();
            label25 = new Label();
            txtStripAgentId = new TextBox();
            label26 = new Label();
            label6 = new Label();
            listBox1 = new ListBox();
            groupNormal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudNormalOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalOffsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalBleed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalDpi).BeginInit();
            groupStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStripBleed).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStripDpi).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 15);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 20;
            label1.Text = "API Base URL";
            // 
            // txtSupabaseUrl
            // 
            txtSupabaseUrl.Location = new Point(18, 33);
            txtSupabaseUrl.Name = "txtSupabaseUrl";
            txtSupabaseUrl.Size = new Size(446, 23);
            txtSupabaseUrl.TabIndex = 19;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 68);
            label2.Name = "label2";
            label2.Size = new Size(43, 15);
            label2.TabIndex = 18;
            label2.Text = "Unit ID";
            // 
            // txtUnitId
            // 
            txtUnitId.Location = new Point(18, 86);
            txtUnitId.Name = "txtUnitId";
            txtUnitId.Size = new Size(220, 23);
            txtUnitId.TabIndex = 17;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(244, 68);
            label3.Name = "label3";
            label3.Size = new Size(49, 15);
            label3.TabIndex = 16;
            label3.Text = "Kiosk ID";
            // 
            // txtKioskId
            // 
            txtKioskId.Location = new Point(244, 86);
            txtKioskId.Name = "txtKioskId";
            txtKioskId.Size = new Size(220, 23);
            txtKioskId.TabIndex = 15;
            // 
            // chkStartWithWindows
            // 
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new Point(18, 121);
            chkStartWithWindows.Name = "chkStartWithWindows";
            chkStartWithWindows.Size = new Size(147, 19);
            chkStartWithWindows.TabIndex = 14;
            chkStartWithWindows.Text = "Iniciar com o Windows";
            // 
            // chkAutoStartListening
            // 
            chkAutoStartListening.AutoSize = true;
            chkAutoStartListening.Checked = true;
            chkAutoStartListening.CheckState = CheckState.Checked;
            chkAutoStartListening.Location = new Point(18, 146);
            chkAutoStartListening.Name = "chkAutoStartListening";
            chkAutoStartListening.Size = new Size(192, 19);
            chkAutoStartListening.TabIndex = 13;
            chkAutoStartListening.Text = "Iniciar escuta automaticamente";
            // 
            // chkEnableLocalIntegration
            // 
            chkEnableLocalIntegration.AutoSize = true;
            chkEnableLocalIntegration.Checked = true;
            chkEnableLocalIntegration.CheckState = CheckState.Checked;
            chkEnableLocalIntegration.Location = new Point(244, 146);
            chkEnableLocalIntegration.Name = "chkEnableLocalIntegration";
            chkEnableLocalIntegration.Size = new Size(201, 19);
            chkEnableLocalIntegration.TabIndex = 12;
            chkEnableLocalIntegration.Text = "Permitir integração local site/app";
            // 
            // btnLoadPrinters
            // 
            btnLoadPrinters.Location = new Point(482, 30);
            btnLoadPrinters.Name = "btnLoadPrinters";
            btnLoadPrinters.Size = new Size(174, 28);
            btnLoadPrinters.TabIndex = 11;
            btnLoadPrinters.Text = "Carregar Impressoras";
            btnLoadPrinters.Click += btnLoadPrinters_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(662, 30);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(174, 28);
            btnSaveConfig.TabIndex = 10;
            btnSaveConfig.Text = "Salvar Configuração";
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // btnStartAll
            // 
            btnStartAll.Location = new Point(482, 117);
            btnStartAll.Name = "btnStartAll";
            btnStartAll.Size = new Size(174, 28);
            btnStartAll.TabIndex = 9;
            btnStartAll.Text = "Iniciar Normal";
            btnStartAll.Click += btnStartAll_Click;
            // 
            // btnStopAll
            // 
            btnStopAll.Location = new Point(662, 117);
            btnStopAll.Name = "btnStopAll";
            btnStopAll.Size = new Size(174, 28);
            btnStopAll.TabIndex = 8;
            btnStopAll.Text = "Parar Escuta";
            btnStopAll.Click += btnStopAll_Click;
            // 
            // btnCheckUpdates
            // 
            btnCheckUpdates.Location = new Point(482, 152);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.Size = new Size(174, 28);
            btnCheckUpdates.TabIndex = 7;
            btnCheckUpdates.Text = "Verificar Atualização";
            btnCheckUpdates.Click += btnCheckUpdates_Click;
            // 
            // btnSyncFromSite
            // 
            btnSyncFromSite.Location = new Point(662, 152);
            btnSyncFromSite.Name = "btnSyncFromSite";
            btnSyncFromSite.Size = new Size(174, 28);
            btnSyncFromSite.TabIndex = 6;
            btnSyncFromSite.Text = "Sincronizar do Site";
            btnSyncFromSite.Click += btnSyncFromSite_Click;
            // 
            // lblRunMode
            // 
            lblRunMode.AutoSize = true;
            lblRunMode.Location = new Point(482, 71);
            lblRunMode.Name = "lblRunMode";
            lblRunMode.Size = new Size(107, 15);
            lblRunMode.TabIndex = 5;
            lblRunMode.Text = "Modo de execução";
            // 
            // cmbRunMode
            // 
            cmbRunMode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRunMode.FormattingEnabled = true;
            cmbRunMode.Location = new Point(482, 89);
            cmbRunMode.Name = "cmbRunMode";
            cmbRunMode.Size = new Size(354, 23);
            cmbRunMode.TabIndex = 4;
            cmbRunMode.SelectedIndexChanged += cmbRunMode_SelectedIndexChanged;
            // 
            // groupNormal
            // 
            groupNormal.Controls.Add(lblNormalSystemPrinterValue);
            groupNormal.Controls.Add(label17);
            groupNormal.Controls.Add(lblNormalStatusText);
            groupNormal.Controls.Add(lblNormalStatusDot);
            groupNormal.Controls.Add(label16);
            groupNormal.Controls.Add(nudNormalOffsetY);
            groupNormal.Controls.Add(label15);
            groupNormal.Controls.Add(nudNormalOffsetX);
            groupNormal.Controls.Add(label14);
            groupNormal.Controls.Add(nudNormalBleed);
            groupNormal.Controls.Add(label13);
            groupNormal.Controls.Add(nudNormalDpi);
            groupNormal.Controls.Add(label27);
            groupNormal.Controls.Add(cmbNormalRotation);
            groupNormal.Controls.Add(label12);
            groupNormal.Controls.Add(cmbNormalPaper);
            groupNormal.Controls.Add(label28);
            groupNormal.Controls.Add(cmbNormalPrinter);
            groupNormal.Controls.Add(label11);
            groupNormal.Controls.Add(txtNormalToken);
            groupNormal.Controls.Add(label10);
            groupNormal.Controls.Add(txtNormalAgentId);
            groupNormal.Controls.Add(label9);
            groupNormal.Location = new Point(18, 192);
            groupNormal.Name = "groupNormal";
            groupNormal.Size = new Size(400, 305);
            groupNormal.TabIndex = 3;
            groupNormal.TabStop = false;
            groupNormal.Text = "Perfil Normal";
            // 
            // lblNormalSystemPrinterValue
            // 
            lblNormalSystemPrinterValue.AutoEllipsis = true;
            lblNormalSystemPrinterValue.Location = new Point(126, 272);
            lblNormalSystemPrinterValue.Name = "lblNormalSystemPrinterValue";
            lblNormalSystemPrinterValue.Size = new Size(255, 18);
            lblNormalSystemPrinterValue.TabIndex = 0;
            lblNormalSystemPrinterValue.Text = "-";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(16, 272);
            label17.Name = "label17";
            label17.Size = new Size(96, 15);
            label17.TabIndex = 1;
            label17.Text = "Totem / Sistema:";
            // 
            // lblNormalStatusText
            // 
            lblNormalStatusText.AutoSize = true;
            lblNormalStatusText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNormalStatusText.Location = new Point(335, 25);
            lblNormalStatusText.Name = "lblNormalStatusText";
            lblNormalStatusText.Size = new Size(46, 15);
            lblNormalStatusText.TabIndex = 2;
            lblNormalStatusText.Text = "Inativo";
            // 
            // lblNormalStatusDot
            // 
            lblNormalStatusDot.AutoSize = true;
            lblNormalStatusDot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblNormalStatusDot.ForeColor = Color.Firebrick;
            lblNormalStatusDot.Location = new Point(312, 20);
            lblNormalStatusDot.Name = "lblNormalStatusDot";
            lblNormalStatusDot.Size = new Size(20, 21);
            lblNormalStatusDot.TabIndex = 3;
            lblNormalStatusDot.Text = "●";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(16, 25);
            label16.Name = "label16";
            label16.Size = new Size(39, 15);
            label16.TabIndex = 4;
            label16.Text = "Status";
            // 
            // nudNormalOffsetY
            // 
            nudNormalOffsetY.Location = new Point(304, 235);
            nudNormalOffsetY.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalOffsetY.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudNormalOffsetY.Name = "nudNormalOffsetY";
            nudNormalOffsetY.Size = new Size(77, 23);
            nudNormalOffsetY.TabIndex = 5;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(304, 217);
            label15.Name = "label15";
            label15.Size = new Size(49, 15);
            label15.TabIndex = 6;
            label15.Text = "Offset Y";
            // 
            // nudNormalOffsetX
            // 
            nudNormalOffsetX.Location = new Point(210, 235);
            nudNormalOffsetX.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalOffsetX.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudNormalOffsetX.Name = "nudNormalOffsetX";
            nudNormalOffsetX.Size = new Size(77, 23);
            nudNormalOffsetX.TabIndex = 7;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(210, 217);
            label14.Name = "label14";
            label14.Size = new Size(49, 15);
            label14.TabIndex = 8;
            label14.Text = "Offset X";
            // 
            // nudNormalBleed
            // 
            nudNormalBleed.Location = new Point(116, 235);
            nudNormalBleed.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalBleed.Name = "nudNormalBleed";
            nudNormalBleed.Size = new Size(77, 23);
            nudNormalBleed.TabIndex = 9;
            nudNormalBleed.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(116, 217);
            label13.Name = "label13";
            label13.Size = new Size(36, 15);
            label13.TabIndex = 10;
            label13.Text = "Bleed";
            // 
            // nudNormalDpi
            // 
            nudNormalDpi.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalDpi.Location = new Point(16, 235);
            nudNormalDpi.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            nudNormalDpi.Minimum = new decimal(new int[] { 150, 0, 0, 0 });
            nudNormalDpi.Name = "nudNormalDpi";
            nudNormalDpi.Size = new Size(87, 23);
            nudNormalDpi.TabIndex = 11;
            nudNormalDpi.Value = new decimal(new int[] { 300, 0, 0, 0 });
            // 
            // label27
            // 
            label27.AutoSize = true;
            label27.Location = new Point(16, 217);
            label27.Name = "label27";
            label27.Size = new Size(25, 15);
            label27.TabIndex = 12;
            label27.Text = "DPI";
            // 
            // cmbNormalRotation
            // 
            cmbNormalRotation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNormalRotation.FormattingEnabled = true;
            cmbNormalRotation.Location = new Point(16, 186);
            cmbNormalRotation.Name = "cmbNormalRotation";
            cmbNormalRotation.Size = new Size(365, 23);
            cmbNormalRotation.TabIndex = 13;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(16, 168);
            label12.Name = "label12";
            label12.Size = new Size(50, 15);
            label12.TabIndex = 14;
            label12.Text = "Rotação";
            // 
            // cmbNormalPaper
            // 
            cmbNormalPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNormalPaper.FormattingEnabled = true;
            cmbNormalPaper.Location = new Point(16, 137);
            cmbNormalPaper.Name = "cmbNormalPaper";
            cmbNormalPaper.Size = new Size(365, 23);
            cmbNormalPaper.TabIndex = 15;
            // 
            // label28
            // 
            label28.AutoSize = true;
            label28.Location = new Point(16, 119);
            label28.Name = "label28";
            label28.Size = new Size(60, 15);
            label28.TabIndex = 16;
            label28.Text = "Paper Size";
            // 
            // cmbNormalPrinter
            // 
            cmbNormalPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNormalPrinter.FormattingEnabled = true;
            cmbNormalPrinter.Location = new Point(16, 88);
            cmbNormalPrinter.Name = "cmbNormalPrinter";
            cmbNormalPrinter.Size = new Size(365, 23);
            cmbNormalPrinter.TabIndex = 17;
            cmbNormalPrinter.SelectedIndexChanged += cmbNormalPrinter_SelectedIndexChanged;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(16, 70);
            label11.Name = "label11";
            label11.Size = new Size(117, 15);
            label11.TabIndex = 18;
            label11.Text = "Impressora Windows";
            // 
            // txtNormalToken
            // 
            txtNormalToken.Location = new Point(145, 42);
            txtNormalToken.Name = "txtNormalToken";
            txtNormalToken.Size = new Size(236, 23);
            txtNormalToken.TabIndex = 19;
            txtNormalToken.UseSystemPasswordChar = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(145, 24);
            label10.Name = "label10";
            label10.Size = new Size(74, 15);
            label10.TabIndex = 20;
            label10.Text = "Agent Token";
            // 
            // txtNormalAgentId
            // 
            txtNormalAgentId.Location = new Point(16, 42);
            txtNormalAgentId.Name = "txtNormalAgentId";
            txtNormalAgentId.Size = new Size(107, 23);
            txtNormalAgentId.TabIndex = 21;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(16, 24);
            label9.Name = "label9";
            label9.Size = new Size(53, 15);
            label9.TabIndex = 22;
            label9.Text = "Agent ID";
            // 
            // groupStrip
            // 
            groupStrip.Controls.Add(lblStripSystemPrinterValue);
            groupStrip.Controls.Add(label18);
            groupStrip.Controls.Add(lblStripStatusText);
            groupStrip.Controls.Add(lblStripStatusDot);
            groupStrip.Controls.Add(label19);
            groupStrip.Controls.Add(nudStripOffsetY);
            groupStrip.Controls.Add(label20);
            groupStrip.Controls.Add(nudStripOffsetX);
            groupStrip.Controls.Add(label21);
            groupStrip.Controls.Add(nudStripBleed);
            groupStrip.Controls.Add(label22);
            groupStrip.Controls.Add(nudStripDpi);
            groupStrip.Controls.Add(label29);
            groupStrip.Controls.Add(cmbStripRotation);
            groupStrip.Controls.Add(label23);
            groupStrip.Controls.Add(cmbStripPaper);
            groupStrip.Controls.Add(label30);
            groupStrip.Controls.Add(cmbStripPrinter);
            groupStrip.Controls.Add(label24);
            groupStrip.Controls.Add(txtStripToken);
            groupStrip.Controls.Add(label25);
            groupStrip.Controls.Add(txtStripAgentId);
            groupStrip.Controls.Add(label26);
            groupStrip.Location = new Point(436, 192);
            groupStrip.Name = "groupStrip";
            groupStrip.Size = new Size(400, 305);
            groupStrip.TabIndex = 2;
            groupStrip.TabStop = false;
            groupStrip.Text = "Perfil Tirinha / Cinema";
            // 
            // lblStripSystemPrinterValue
            // 
            lblStripSystemPrinterValue.AutoEllipsis = true;
            lblStripSystemPrinterValue.Location = new Point(126, 272);
            lblStripSystemPrinterValue.Name = "lblStripSystemPrinterValue";
            lblStripSystemPrinterValue.Size = new Size(255, 18);
            lblStripSystemPrinterValue.TabIndex = 0;
            lblStripSystemPrinterValue.Text = "-";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(16, 272);
            label18.Name = "label18";
            label18.Size = new Size(96, 15);
            label18.TabIndex = 1;
            label18.Text = "Totem / Sistema:";
            // 
            // lblStripStatusText
            // 
            lblStripStatusText.AutoSize = true;
            lblStripStatusText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStripStatusText.Location = new Point(335, 26);
            lblStripStatusText.Name = "lblStripStatusText";
            lblStripStatusText.Size = new Size(46, 15);
            lblStripStatusText.TabIndex = 2;
            lblStripStatusText.Text = "Inativo";
            // 
            // lblStripStatusDot
            // 
            lblStripStatusDot.AutoSize = true;
            lblStripStatusDot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStripStatusDot.ForeColor = Color.Firebrick;
            lblStripStatusDot.Location = new Point(312, 21);
            lblStripStatusDot.Name = "lblStripStatusDot";
            lblStripStatusDot.Size = new Size(20, 21);
            lblStripStatusDot.TabIndex = 3;
            lblStripStatusDot.Text = "●";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(16, 25);
            label19.Name = "label19";
            label19.Size = new Size(39, 15);
            label19.TabIndex = 4;
            label19.Text = "Status";
            // 
            // nudStripOffsetY
            // 
            nudStripOffsetY.Location = new Point(304, 235);
            nudStripOffsetY.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripOffsetY.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudStripOffsetY.Name = "nudStripOffsetY";
            nudStripOffsetY.Size = new Size(77, 23);
            nudStripOffsetY.TabIndex = 5;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(304, 217);
            label20.Name = "label20";
            label20.Size = new Size(49, 15);
            label20.TabIndex = 6;
            label20.Text = "Offset Y";
            // 
            // nudStripOffsetX
            // 
            nudStripOffsetX.Location = new Point(210, 235);
            nudStripOffsetX.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripOffsetX.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudStripOffsetX.Name = "nudStripOffsetX";
            nudStripOffsetX.Size = new Size(77, 23);
            nudStripOffsetX.TabIndex = 7;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(210, 217);
            label21.Name = "label21";
            label21.Size = new Size(49, 15);
            label21.TabIndex = 8;
            label21.Text = "Offset X";
            // 
            // nudStripBleed
            // 
            nudStripBleed.Location = new Point(116, 235);
            nudStripBleed.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripBleed.Name = "nudStripBleed";
            nudStripBleed.Size = new Size(77, 23);
            nudStripBleed.TabIndex = 9;
            nudStripBleed.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(116, 217);
            label22.Name = "label22";
            label22.Size = new Size(36, 15);
            label22.TabIndex = 10;
            label22.Text = "Bleed";
            // 
            // nudStripDpi
            // 
            nudStripDpi.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripDpi.Location = new Point(16, 235);
            nudStripDpi.Maximum = new decimal(new int[] { 600, 0, 0, 0 });
            nudStripDpi.Minimum = new decimal(new int[] { 150, 0, 0, 0 });
            nudStripDpi.Name = "nudStripDpi";
            nudStripDpi.Size = new Size(87, 23);
            nudStripDpi.TabIndex = 11;
            nudStripDpi.Value = new decimal(new int[] { 300, 0, 0, 0 });
            // 
            // label29
            // 
            label29.AutoSize = true;
            label29.Location = new Point(16, 217);
            label29.Name = "label29";
            label29.Size = new Size(25, 15);
            label29.TabIndex = 12;
            label29.Text = "DPI";
            // 
            // cmbStripRotation
            // 
            cmbStripRotation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStripRotation.FormattingEnabled = true;
            cmbStripRotation.Location = new Point(16, 186);
            cmbStripRotation.Name = "cmbStripRotation";
            cmbStripRotation.Size = new Size(365, 23);
            cmbStripRotation.TabIndex = 13;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(16, 168);
            label23.Name = "label23";
            label23.Size = new Size(50, 15);
            label23.TabIndex = 14;
            label23.Text = "Rotação";
            // 
            // cmbStripPaper
            // 
            cmbStripPaper.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStripPaper.FormattingEnabled = true;
            cmbStripPaper.Location = new Point(16, 137);
            cmbStripPaper.Name = "cmbStripPaper";
            cmbStripPaper.Size = new Size(365, 23);
            cmbStripPaper.TabIndex = 15;
            // 
            // label30
            // 
            label30.AutoSize = true;
            label30.Location = new Point(16, 119);
            label30.Name = "label30";
            label30.Size = new Size(60, 15);
            label30.TabIndex = 16;
            label30.Text = "Paper Size";
            // 
            // cmbStripPrinter
            // 
            cmbStripPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStripPrinter.FormattingEnabled = true;
            cmbStripPrinter.Location = new Point(16, 88);
            cmbStripPrinter.Name = "cmbStripPrinter";
            cmbStripPrinter.Size = new Size(365, 23);
            cmbStripPrinter.TabIndex = 17;
            cmbStripPrinter.SelectedIndexChanged += cmbStripPrinter_SelectedIndexChanged;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(16, 70);
            label24.Name = "label24";
            label24.Size = new Size(117, 15);
            label24.TabIndex = 18;
            label24.Text = "Impressora Windows";
            // 
            // txtStripToken
            // 
            txtStripToken.Location = new Point(145, 42);
            txtStripToken.Name = "txtStripToken";
            txtStripToken.Size = new Size(236, 23);
            txtStripToken.TabIndex = 19;
            txtStripToken.UseSystemPasswordChar = true;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(145, 24);
            label25.Name = "label25";
            label25.Size = new Size(74, 15);
            label25.TabIndex = 20;
            label25.Text = "Agent Token";
            // 
            // txtStripAgentId
            // 
            txtStripAgentId.Location = new Point(16, 42);
            txtStripAgentId.Name = "txtStripAgentId";
            txtStripAgentId.Size = new Size(107, 23);
            txtStripAgentId.TabIndex = 21;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(16, 24);
            label26.Name = "label26";
            label26.Size = new Size(53, 15);
            label26.TabIndex = 22;
            label26.Text = "Agent ID";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 509);
            label6.Name = "label6";
            label6.Size = new Size(32, 15);
            label6.TabIndex = 1;
            label6.Text = "Logs";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(18, 527);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(818, 109);
            listBox1.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(854, 651);
            Controls.Add(listBox1);
            Controls.Add(label6);
            Controls.Add(groupStrip);
            Controls.Add(groupNormal);
            Controls.Add(cmbRunMode);
            Controls.Add(lblRunMode);
            Controls.Add(btnSyncFromSite);
            Controls.Add(btnCheckUpdates);
            Controls.Add(btnStopAll);
            Controls.Add(btnStartAll);
            Controls.Add(btnSaveConfig);
            Controls.Add(btnLoadPrinters);
            Controls.Add(chkEnableLocalIntegration);
            Controls.Add(chkAutoStartListening);
            Controls.Add(chkStartWithWindows);
            Controls.Add(txtKioskId);
            Controls.Add(label3);
            Controls.Add(txtUnitId);
            Controls.Add(label2);
            Controls.Add(txtSupabaseUrl);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CBL Print Assistant";
            groupNormal.ResumeLayout(false);
            groupNormal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudNormalOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalOffsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalBleed).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudNormalDpi).EndInit();
            groupStrip.ResumeLayout(false);
            groupStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStripBleed).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStripDpi).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtSupabaseUrl;
        private Label label2;
        private TextBox txtUnitId;
        private Label label3;
        private TextBox txtKioskId;
        private CheckBox chkStartWithWindows;
        private CheckBox chkAutoStartListening;
        private CheckBox chkEnableLocalIntegration;
        private Button btnLoadPrinters;
        private Button btnSaveConfig;
        private Button btnStartAll;
        private Button btnStopAll;
        private Button btnCheckUpdates;
        private Button btnSyncFromSite;
        private Label lblRunMode;
        private ComboBox cmbRunMode;
        private GroupBox groupNormal;
        private GroupBox groupStrip;
        private Label label6;
        private ListBox listBox1;

        private TextBox txtNormalAgentId;
        private TextBox txtNormalToken;
        private ComboBox cmbNormalPrinter;
        private ComboBox cmbNormalPaper;
        private ComboBox cmbNormalRotation;
        private NumericUpDown nudNormalDpi;
        private NumericUpDown nudNormalBleed;
        private NumericUpDown nudNormalOffsetX;
        private NumericUpDown nudNormalOffsetY;
        private Label lblNormalStatusDot;
        private Label lblNormalStatusText;
        private Label lblNormalSystemPrinterValue;

        private TextBox txtStripAgentId;
        private TextBox txtStripToken;
        private ComboBox cmbStripPrinter;
        private ComboBox cmbStripPaper;
        private ComboBox cmbStripRotation;
        private NumericUpDown nudStripDpi;
        private NumericUpDown nudStripBleed;
        private NumericUpDown nudStripOffsetX;
        private NumericUpDown nudStripOffsetY;
        private Label lblStripStatusDot;
        private Label lblStripStatusText;
        private Label lblStripSystemPrinterValue;

        private Label label9;
        private Label label10;
        private Label label11;
        private Label label12;
        private Label label13;
        private Label label14;
        private Label label15;
        private Label label16;
        private Label label17;
        private Label label18;
        private Label label19;
        private Label label20;
        private Label label21;
        private Label label22;
        private Label label23;
        private Label label24;
        private Label label25;
        private Label label26;
        private Label label27;
        private Label label28;
        private Label label29;
        private Label label30;
    }
}
