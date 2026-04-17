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
            btnLoadPrinters = new Button();
            btnSaveConfig = new Button();
            btnStartAll = new Button();
            btnStopAll = new Button();
            btnCheckUpdates = new Button();
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
            cmbNormalRotation = new ComboBox();
            label12 = new Label();
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
            cmbStripRotation = new ComboBox();
            label23 = new Label();
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
            groupStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStripBleed).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(18, 15);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 0;
            label1.Text = "API Base URL";
            // 
            // txtSupabaseUrl
            // 
            txtSupabaseUrl.Location = new Point(18, 33);
            txtSupabaseUrl.Name = "txtSupabaseUrl";
            txtSupabaseUrl.Size = new Size(446, 23);
            txtSupabaseUrl.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(18, 68);
            label2.Name = "label2";
            label2.Size = new Size(43, 15);
            label2.TabIndex = 2;
            label2.Text = "Unit ID";
            // 
            // txtUnitId
            // 
            txtUnitId.Location = new Point(18, 86);
            txtUnitId.Name = "txtUnitId";
            txtUnitId.Size = new Size(220, 23);
            txtUnitId.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(244, 68);
            label3.Name = "label3";
            label3.Size = new Size(49, 15);
            label3.TabIndex = 4;
            label3.Text = "Kiosk ID";
            // 
            // txtKioskId
            // 
            txtKioskId.Location = new Point(244, 86);
            txtKioskId.Name = "txtKioskId";
            txtKioskId.Size = new Size(220, 23);
            txtKioskId.TabIndex = 5;
            // 
            // chkStartWithWindows
            // 
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new Point(18, 121);
            chkStartWithWindows.Name = "chkStartWithWindows";
            chkStartWithWindows.Size = new Size(147, 19);
            chkStartWithWindows.TabIndex = 6;
            chkStartWithWindows.Text = "Iniciar com o Windows";
            chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // btnLoadPrinters
            // 
            btnLoadPrinters.Location = new Point(482, 30);
            btnLoadPrinters.Name = "btnLoadPrinters";
            btnLoadPrinters.Size = new Size(174, 28);
            btnLoadPrinters.TabIndex = 7;
            btnLoadPrinters.Text = "Carregar Impressoras";
            btnLoadPrinters.UseVisualStyleBackColor = true;
            btnLoadPrinters.Click += btnLoadPrinters_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(662, 30);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(174, 28);
            btnSaveConfig.TabIndex = 8;
            btnSaveConfig.Text = "Salvar Configuração";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // btnStartAll
            // 
            btnStartAll.Location = new Point(482, 82);
            btnStartAll.Name = "btnStartAll";
            btnStartAll.Size = new Size(174, 28);
            btnStartAll.TabIndex = 9;
            btnStartAll.Text = "Iniciar Escuta";
            btnStartAll.UseVisualStyleBackColor = true;
            btnStartAll.Click += btnStartAll_Click;
            // 
            // btnStopAll
            // 
            btnStopAll.Location = new Point(662, 82);
            btnStopAll.Name = "btnStopAll";
            btnStopAll.Size = new Size(174, 28);
            btnStopAll.TabIndex = 10;
            btnStopAll.Text = "Parar Escuta";
            btnStopAll.UseVisualStyleBackColor = true;
            btnStopAll.Click += btnStopAll_Click;
            // 
            // btnCheckUpdates
            // 
            btnCheckUpdates.Location = new Point(482, 117);
            btnCheckUpdates.Name = "btnCheckUpdates";
            btnCheckUpdates.Size = new Size(354, 28);
            btnCheckUpdates.TabIndex = 11;
            btnCheckUpdates.Text = "Verificar Atualização";
            btnCheckUpdates.UseVisualStyleBackColor = true;
            btnCheckUpdates.Click += btnCheckUpdates_Click;
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
            groupNormal.Controls.Add(cmbNormalRotation);
            groupNormal.Controls.Add(label12);
            groupNormal.Controls.Add(cmbNormalPrinter);
            groupNormal.Controls.Add(label11);
            groupNormal.Controls.Add(txtNormalToken);
            groupNormal.Controls.Add(label10);
            groupNormal.Controls.Add(txtNormalAgentId);
            groupNormal.Controls.Add(label9);
            groupNormal.Location = new Point(18, 157);
            groupNormal.Name = "groupNormal";
            groupNormal.Size = new Size(400, 257);
            groupNormal.TabIndex = 12;
            groupNormal.TabStop = false;
            groupNormal.Text = "Perfil 1 - Foto Normal";
            // 
            // lblNormalSystemPrinterValue
            // 
            lblNormalSystemPrinterValue.AutoEllipsis = true;
            lblNormalSystemPrinterValue.Location = new Point(126, 224);
            lblNormalSystemPrinterValue.Name = "lblNormalSystemPrinterValue";
            lblNormalSystemPrinterValue.Size = new Size(255, 18);
            lblNormalSystemPrinterValue.TabIndex = 18;
            lblNormalSystemPrinterValue.Text = "-";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(16, 224);
            label17.Name = "label17";
            label17.Size = new Size(99, 15);
            label17.TabIndex = 17;
            label17.Text = "Totem / Sistema:";
            // 
            // lblNormalStatusText
            // 
            lblNormalStatusText.AutoSize = true;
            lblNormalStatusText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblNormalStatusText.Location = new Point(160, 25);
            lblNormalStatusText.Name = "lblNormalStatusText";
            lblNormalStatusText.Size = new Size(47, 15);
            lblNormalStatusText.TabIndex = 16;
            lblNormalStatusText.Text = "Inativo";
            // 
            // lblNormalStatusDot
            // 
            lblNormalStatusDot.AutoSize = true;
            lblNormalStatusDot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblNormalStatusDot.ForeColor = Color.Firebrick;
            lblNormalStatusDot.Location = new Point(137, 20);
            lblNormalStatusDot.Name = "lblNormalStatusDot";
            lblNormalStatusDot.Size = new Size(20, 21);
            lblNormalStatusDot.TabIndex = 15;
            lblNormalStatusDot.Text = "●";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(16, 25);
            label16.Name = "label16";
            label16.Size = new Size(39, 15);
            label16.TabIndex = 14;
            label16.Text = "Status";
            // 
            // nudNormalOffsetY
            // 
            nudNormalOffsetY.Location = new Point(304, 187);
            nudNormalOffsetY.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalOffsetY.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudNormalOffsetY.Name = "nudNormalOffsetY";
            nudNormalOffsetY.Size = new Size(77, 23);
            nudNormalOffsetY.TabIndex = 13;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(304, 169);
            label15.Name = "label15";
            label15.Size = new Size(50, 15);
            label15.TabIndex = 12;
            label15.Text = "Offset Y";
            // 
            // nudNormalOffsetX
            // 
            nudNormalOffsetX.Location = new Point(210, 187);
            nudNormalOffsetX.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalOffsetX.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudNormalOffsetX.Name = "nudNormalOffsetX";
            nudNormalOffsetX.Size = new Size(77, 23);
            nudNormalOffsetX.TabIndex = 11;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(210, 169);
            label14.Name = "label14";
            label14.Size = new Size(49, 15);
            label14.TabIndex = 10;
            label14.Text = "Offset X";
            // 
            // nudNormalBleed
            // 
            nudNormalBleed.Location = new Point(116, 187);
            nudNormalBleed.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudNormalBleed.Name = "nudNormalBleed";
            nudNormalBleed.Size = new Size(77, 23);
            nudNormalBleed.TabIndex = 9;
            nudNormalBleed.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(116, 169);
            label13.Name = "label13";
            label13.Size = new Size(37, 15);
            label13.TabIndex = 8;
            label13.Text = "Bleed";
            // 
            // cmbNormalRotation
            // 
            cmbNormalRotation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNormalRotation.FormattingEnabled = true;
            cmbNormalRotation.Location = new Point(16, 187);
            cmbNormalRotation.Name = "cmbNormalRotation";
            cmbNormalRotation.Size = new Size(87, 23);
            cmbNormalRotation.TabIndex = 7;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(16, 169);
            label12.Name = "label12";
            label12.Size = new Size(50, 15);
            label12.TabIndex = 6;
            label12.Text = "Rotação";
            // 
            // cmbNormalPrinter
            // 
            cmbNormalPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbNormalPrinter.FormattingEnabled = true;
            cmbNormalPrinter.Location = new Point(16, 134);
            cmbNormalPrinter.Name = "cmbNormalPrinter";
            cmbNormalPrinter.Size = new Size(365, 23);
            cmbNormalPrinter.TabIndex = 5;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(16, 116);
            label11.Name = "label11";
            label11.Size = new Size(115, 15);
            label11.TabIndex = 4;
            label11.Text = "Impressora Windows";
            // 
            // txtNormalToken
            // 
            txtNormalToken.Location = new Point(16, 86);
            txtNormalToken.Name = "txtNormalToken";
            txtNormalToken.Size = new Size(365, 23);
            txtNormalToken.TabIndex = 3;
            txtNormalToken.UseSystemPasswordChar = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(16, 68);
            label10.Name = "label10";
            label10.Size = new Size(74, 15);
            label10.TabIndex = 2;
            label10.Text = "Agent Token";
            // 
            // txtNormalAgentId
            // 
            txtNormalAgentId.Location = new Point(16, 42);
            txtNormalAgentId.Name = "txtNormalAgentId";
            txtNormalAgentId.Size = new Size(107, 23);
            txtNormalAgentId.TabIndex = 1;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(16, 24);
            label9.Name = "label9";
            label9.Size = new Size(54, 15);
            label9.TabIndex = 0;
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
            groupStrip.Controls.Add(cmbStripRotation);
            groupStrip.Controls.Add(label23);
            groupStrip.Controls.Add(cmbStripPrinter);
            groupStrip.Controls.Add(label24);
            groupStrip.Controls.Add(txtStripToken);
            groupStrip.Controls.Add(label25);
            groupStrip.Controls.Add(txtStripAgentId);
            groupStrip.Controls.Add(label26);
            groupStrip.Location = new Point(436, 157);
            groupStrip.Name = "groupStrip";
            groupStrip.Size = new Size(400, 257);
            groupStrip.TabIndex = 13;
            groupStrip.TabStop = false;
            groupStrip.Text = "Perfil 2 - Tirinha / Cinema";
            // 
            // lblStripSystemPrinterValue
            // 
            lblStripSystemPrinterValue.AutoEllipsis = true;
            lblStripSystemPrinterValue.Location = new Point(126, 224);
            lblStripSystemPrinterValue.Name = "lblStripSystemPrinterValue";
            lblStripSystemPrinterValue.Size = new Size(255, 18);
            lblStripSystemPrinterValue.TabIndex = 18;
            lblStripSystemPrinterValue.Text = "-";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(16, 224);
            label18.Name = "label18";
            label18.Size = new Size(99, 15);
            label18.TabIndex = 17;
            label18.Text = "Totem / Sistema:";
            // 
            // lblStripStatusText
            // 
            lblStripStatusText.AutoSize = true;
            lblStripStatusText.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStripStatusText.Location = new Point(160, 25);
            lblStripStatusText.Name = "lblStripStatusText";
            lblStripStatusText.Size = new Size(47, 15);
            lblStripStatusText.TabIndex = 16;
            lblStripStatusText.Text = "Inativo";
            // 
            // lblStripStatusDot
            // 
            lblStripStatusDot.AutoSize = true;
            lblStripStatusDot.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblStripStatusDot.ForeColor = Color.Firebrick;
            lblStripStatusDot.Location = new Point(137, 20);
            lblStripStatusDot.Name = "lblStripStatusDot";
            lblStripStatusDot.Size = new Size(20, 21);
            lblStripStatusDot.TabIndex = 15;
            lblStripStatusDot.Text = "●";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(16, 25);
            label19.Name = "label19";
            label19.Size = new Size(39, 15);
            label19.TabIndex = 14;
            label19.Text = "Status";
            // 
            // nudStripOffsetY
            // 
            nudStripOffsetY.Location = new Point(304, 187);
            nudStripOffsetY.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripOffsetY.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudStripOffsetY.Name = "nudStripOffsetY";
            nudStripOffsetY.Size = new Size(77, 23);
            nudStripOffsetY.TabIndex = 13;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(304, 169);
            label20.Name = "label20";
            label20.Size = new Size(50, 15);
            label20.TabIndex = 12;
            label20.Text = "Offset Y";
            // 
            // nudStripOffsetX
            // 
            nudStripOffsetX.Location = new Point(210, 187);
            nudStripOffsetX.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripOffsetX.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudStripOffsetX.Name = "nudStripOffsetX";
            nudStripOffsetX.Size = new Size(77, 23);
            nudStripOffsetX.TabIndex = 11;
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(210, 169);
            label21.Name = "label21";
            label21.Size = new Size(49, 15);
            label21.TabIndex = 10;
            label21.Text = "Offset X";
            // 
            // nudStripBleed
            // 
            nudStripBleed.Location = new Point(116, 187);
            nudStripBleed.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudStripBleed.Name = "nudStripBleed";
            nudStripBleed.Size = new Size(77, 23);
            nudStripBleed.TabIndex = 9;
            nudStripBleed.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(116, 169);
            label22.Name = "label22";
            label22.Size = new Size(37, 15);
            label22.TabIndex = 8;
            label22.Text = "Bleed";
            // 
            // cmbStripRotation
            // 
            cmbStripRotation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStripRotation.FormattingEnabled = true;
            cmbStripRotation.Location = new Point(16, 187);
            cmbStripRotation.Name = "cmbStripRotation";
            cmbStripRotation.Size = new Size(87, 23);
            cmbStripRotation.TabIndex = 7;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(16, 169);
            label23.Name = "label23";
            label23.Size = new Size(50, 15);
            label23.TabIndex = 6;
            label23.Text = "Rotação";
            // 
            // cmbStripPrinter
            // 
            cmbStripPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStripPrinter.FormattingEnabled = true;
            cmbStripPrinter.Location = new Point(16, 134);
            cmbStripPrinter.Name = "cmbStripPrinter";
            cmbStripPrinter.Size = new Size(365, 23);
            cmbStripPrinter.TabIndex = 5;
            // 
            // label24
            // 
            label24.AutoSize = true;
            label24.Location = new Point(16, 116);
            label24.Name = "label24";
            label24.Size = new Size(115, 15);
            label24.TabIndex = 4;
            label24.Text = "Impressora Windows";
            // 
            // txtStripToken
            // 
            txtStripToken.Location = new Point(16, 86);
            txtStripToken.Name = "txtStripToken";
            txtStripToken.Size = new Size(365, 23);
            txtStripToken.TabIndex = 3;
            txtStripToken.UseSystemPasswordChar = true;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(16, 68);
            label25.Name = "label25";
            label25.Size = new Size(74, 15);
            label25.TabIndex = 2;
            label25.Text = "Agent Token";
            // 
            // txtStripAgentId
            // 
            txtStripAgentId.Location = new Point(16, 42);
            txtStripAgentId.Name = "txtStripAgentId";
            txtStripAgentId.Size = new Size(107, 23);
            txtStripAgentId.TabIndex = 1;
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(16, 24);
            label26.Name = "label26";
            label26.Size = new Size(54, 15);
            label26.TabIndex = 0;
            label26.Text = "Agent ID";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(18, 426);
            label6.Name = "label6";
            label6.Size = new Size(31, 15);
            label6.TabIndex = 14;
            label6.Text = "Logs";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(18, 444);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(818, 199);
            listBox1.TabIndex = 15;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(854, 661);
            Controls.Add(listBox1);
            Controls.Add(label6);
            Controls.Add(groupStrip);
            Controls.Add(groupNormal);
            Controls.Add(btnCheckUpdates);
            Controls.Add(btnStopAll);
            Controls.Add(btnStartAll);
            Controls.Add(btnSaveConfig);
            Controls.Add(btnLoadPrinters);
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
            groupStrip.ResumeLayout(false);
            groupStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStripOffsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStripBleed).EndInit();
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
        private Button btnLoadPrinters;
        private Button btnSaveConfig;
        private Button btnStartAll;
        private Button btnStopAll;
        private Button btnCheckUpdates;
        private GroupBox groupNormal;
        private GroupBox groupStrip;
        private Label label6;
        private ListBox listBox1;

        private TextBox txtNormalAgentId;
        private TextBox txtNormalToken;
        private ComboBox cmbNormalPrinter;
        private ComboBox cmbNormalRotation;
        private NumericUpDown nudNormalBleed;
        private NumericUpDown nudNormalOffsetX;
        private NumericUpDown nudNormalOffsetY;
        private Label lblNormalStatusDot;
        private Label lblNormalStatusText;
        private Label lblNormalSystemPrinterValue;

        private TextBox txtStripAgentId;
        private TextBox txtStripToken;
        private ComboBox cmbStripPrinter;
        private ComboBox cmbStripRotation;
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
    }
}