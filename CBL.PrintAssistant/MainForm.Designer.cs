namespace CBL.PrintAssistant
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
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
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            txtSupabaseUrl = new TextBox();
            txtSupabaseKey = new TextBox();
            txtUnitId = new TextBox();
            txtKioskId = new TextBox();
            cmbPrinters = new ComboBox();
            btnLoadPrinters = new Button();
            btnSaveConfig = new Button();
            btnTestPrint = new Button();
            btnStartListener = new Button();
            chkStartWithWindows = new CheckBox();
            listBox1 = new ListBox();
            label7 = new Label();
            cmbRotation = new ComboBox();
            label8 = new Label();
            lblStatusDot = new Label();
            lblStatusText = new Label();
            groupBox1 = new GroupBox();
            lblSystemPrinterValue = new Label();
            label10 = new Label();
            lblAgentIdValue = new Label();
            label9 = new Label();
            groupBox2 = new GroupBox();
            nudOffsetY = new NumericUpDown();
            label13 = new Label();
            nudOffsetX = new NumericUpDown();
            label12 = new Label();
            nudBleed = new NumericUpDown();
            label11 = new Label();
            groupBox3 = new GroupBox();
            lblLastJobValue = new Label();
            label15 = new Label();
            lblLastHeartbeatValue = new Label();
            label14 = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffsetY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOffsetX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudBleed).BeginInit();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(20, 20);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 0;
            label1.Text = "API Base URL";
            label1.Click += lblSupabaseUrl_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(20, 76);
            label2.Name = "label2";
            label2.Size = new Size(74, 15);
            label2.TabIndex = 1;
            label2.Text = "Agent Token";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 132);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 2;
            label3.Text = "Unit ID";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 188);
            label4.Name = "label4";
            label4.Size = new Size(49, 15);
            label4.TabIndex = 3;
            label4.Text = "Kiosk ID";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(20, 244);
            label5.Name = "label5";
            label5.Size = new Size(115, 15);
            label5.TabIndex = 4;
            label5.Text = "Impressora Windows";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(20, 488);
            label6.Name = "label6";
            label6.Size = new Size(31, 15);
            label6.TabIndex = 5;
            label6.Text = "Logs";
            // 
            // txtSupabaseUrl
            // 
            txtSupabaseUrl.Location = new Point(20, 38);
            txtSupabaseUrl.Name = "txtSupabaseUrl";
            txtSupabaseUrl.Size = new Size(540, 23);
            txtSupabaseUrl.TabIndex = 6;
            txtSupabaseUrl.TextChanged += txtSupabaseUrl_TextChanged;
            // 
            // txtSupabaseKey
            // 
            txtSupabaseKey.Location = new Point(20, 94);
            txtSupabaseKey.Name = "txtSupabaseKey";
            txtSupabaseKey.Size = new Size(540, 23);
            txtSupabaseKey.TabIndex = 7;
            txtSupabaseKey.UseSystemPasswordChar = true;
            // 
            // txtUnitId
            // 
            txtUnitId.Location = new Point(20, 150);
            txtUnitId.Name = "txtUnitId";
            txtUnitId.Size = new Size(260, 23);
            txtUnitId.TabIndex = 8;
            txtUnitId.TextChanged += textBox3_TextChanged;
            // 
            // txtKioskId
            // 
            txtKioskId.Location = new Point(300, 150);
            txtKioskId.Name = "txtKioskId";
            txtKioskId.Size = new Size(260, 23);
            txtKioskId.TabIndex = 9;
            // 
            // cmbPrinters
            // 
            cmbPrinters.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPrinters.FormattingEnabled = true;
            cmbPrinters.Location = new Point(20, 262);
            cmbPrinters.Name = "cmbPrinters";
            cmbPrinters.Size = new Size(540, 23);
            cmbPrinters.TabIndex = 10;
            // 
            // btnLoadPrinters
            // 
            btnLoadPrinters.Location = new Point(580, 36);
            btnLoadPrinters.Name = "btnLoadPrinters";
            btnLoadPrinters.Size = new Size(170, 28);
            btnLoadPrinters.TabIndex = 11;
            btnLoadPrinters.Text = "Carregar Impressoras";
            btnLoadPrinters.UseVisualStyleBackColor = true;
            btnLoadPrinters.Click += btnLoadPrinters_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(580, 76);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(170, 28);
            btnSaveConfig.TabIndex = 12;
            btnSaveConfig.Text = "Salvar Configuração";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // btnTestPrint
            // 
            btnTestPrint.Location = new Point(580, 116);
            btnTestPrint.Name = "btnTestPrint";
            btnTestPrint.Size = new Size(170, 28);
            btnTestPrint.TabIndex = 13;
            btnTestPrint.Text = "Teste de Impressão";
            btnTestPrint.UseVisualStyleBackColor = true;
            btnTestPrint.Click += btnTestPrint_Click;
            // 
            // btnStartListener
            // 
            btnStartListener.Location = new Point(580, 156);
            btnStartListener.Name = "btnStartListener";
            btnStartListener.Size = new Size(170, 28);
            btnStartListener.TabIndex = 14;
            btnStartListener.Text = "Iniciar Escuta";
            btnStartListener.UseVisualStyleBackColor = true;
            btnStartListener.Click += btnStartListener_Click;
            // 
            // chkStartWithWindows
            // 
            chkStartWithWindows.AutoSize = true;
            chkStartWithWindows.Location = new Point(20, 458);
            chkStartWithWindows.Name = "chkStartWithWindows";
            chkStartWithWindows.Size = new Size(147, 19);
            chkStartWithWindows.TabIndex = 15;
            chkStartWithWindows.Text = "Iniciar com o Windows";
            chkStartWithWindows.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Location = new Point(20, 506);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(730, 169);
            listBox1.TabIndex = 16;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(20, 300);
            label7.Name = "label7";
            label7.Size = new Size(50, 15);
            label7.TabIndex = 17;
            label7.Text = "Rotação";
            // 
            // cmbRotation
            // 
            cmbRotation.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRotation.FormattingEnabled = true;
            cmbRotation.Location = new Point(20, 318);
            cmbRotation.Name = "cmbRotation";
            cmbRotation.Size = new Size(180, 23);
            cmbRotation.TabIndex = 18;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(580, 208);
            label8.Name = "label8";
            label8.Size = new Size(39, 15);
            label8.TabIndex = 19;
            label8.Text = "Status";
            // 
            // lblStatusDot
            // 
            lblStatusDot.AutoSize = true;
            lblStatusDot.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStatusDot.ForeColor = Color.Firebrick;
            lblStatusDot.Location = new Point(580, 224);
            lblStatusDot.Name = "lblStatusDot";
            lblStatusDot.Size = new Size(24, 25);
            lblStatusDot.TabIndex = 20;
            lblStatusDot.Text = "●";
            // 
            // lblStatusText
            // 
            lblStatusText.AutoSize = true;
            lblStatusText.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatusText.Location = new Point(605, 229);
            lblStatusText.Name = "lblStatusText";
            lblStatusText.Size = new Size(55, 19);
            lblStatusText.TabIndex = 21;
            lblStatusText.Text = "Inativo";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblSystemPrinterValue);
            groupBox1.Controls.Add(label10);
            groupBox1.Controls.Add(lblAgentIdValue);
            groupBox1.Controls.Add(label9);
            groupBox1.Location = new Point(20, 356);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(360, 90);
            groupBox1.TabIndex = 22;
            groupBox1.TabStop = false;
            groupBox1.Text = "Identificação";
            // 
            // lblSystemPrinterValue
            // 
            lblSystemPrinterValue.AutoEllipsis = true;
            lblSystemPrinterValue.Location = new Point(117, 52);
            lblSystemPrinterValue.Name = "lblSystemPrinterValue";
            lblSystemPrinterValue.Size = new Size(225, 18);
            lblSystemPrinterValue.TabIndex = 3;
            lblSystemPrinterValue.Text = "-";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(12, 52);
            label10.Name = "label10";
            label10.Size = new Size(99, 15);
            label10.TabIndex = 2;
            label10.Text = "Totem / Sistema:";
            // 
            // lblAgentIdValue
            // 
            lblAgentIdValue.AutoEllipsis = true;
            lblAgentIdValue.Location = new Point(117, 25);
            lblAgentIdValue.Name = "lblAgentIdValue";
            lblAgentIdValue.Size = new Size(225, 18);
            lblAgentIdValue.TabIndex = 1;
            lblAgentIdValue.Text = "-";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(12, 25);
            label9.Name = "label9";
            label9.Size = new Size(54, 15);
            label9.TabIndex = 0;
            label9.Text = "Agent ID:";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(nudOffsetY);
            groupBox2.Controls.Add(label13);
            groupBox2.Controls.Add(nudOffsetX);
            groupBox2.Controls.Add(label12);
            groupBox2.Controls.Add(nudBleed);
            groupBox2.Controls.Add(label11);
            groupBox2.Location = new Point(398, 356);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(352, 90);
            groupBox2.TabIndex = 23;
            groupBox2.TabStop = false;
            groupBox2.Text = "Ajuste de Impressão";
            // 
            // nudOffsetY
            // 
            nudOffsetY.Location = new Point(247, 48);
            nudOffsetY.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudOffsetY.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudOffsetY.Name = "nudOffsetY";
            nudOffsetY.Size = new Size(75, 23);
            nudOffsetY.TabIndex = 5;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(247, 25);
            label13.Name = "label13";
            label13.Size = new Size(50, 15);
            label13.TabIndex = 4;
            label13.Text = "Offset Y";
            // 
            // nudOffsetX
            // 
            nudOffsetX.Location = new Point(137, 48);
            nudOffsetX.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudOffsetX.Minimum = new decimal(new int[] { 50, 0, 0, int.MinValue });
            nudOffsetX.Name = "nudOffsetX";
            nudOffsetX.Size = new Size(75, 23);
            nudOffsetX.TabIndex = 3;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(137, 25);
            label12.Name = "label12";
            label12.Size = new Size(49, 15);
            label12.TabIndex = 2;
            label12.Text = "Offset X";
            // 
            // nudBleed
            // 
            nudBleed.Location = new Point(20, 48);
            nudBleed.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            nudBleed.Name = "nudBleed";
            nudBleed.Size = new Size(75, 23);
            nudBleed.TabIndex = 1;
            nudBleed.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(20, 25);
            label11.Name = "label11";
            label11.Size = new Size(37, 15);
            label11.TabIndex = 0;
            label11.Text = "Bleed";
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(lblLastJobValue);
            groupBox3.Controls.Add(label15);
            groupBox3.Controls.Add(lblLastHeartbeatValue);
            groupBox3.Controls.Add(label14);
            groupBox3.Location = new Point(580, 262);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(170, 79);
            groupBox3.TabIndex = 24;
            groupBox3.TabStop = false;
            groupBox3.Text = "Monitor";
            // 
            // lblLastJobValue
            // 
            lblLastJobValue.AutoEllipsis = true;
            lblLastJobValue.Location = new Point(14, 54);
            lblLastJobValue.Name = "lblLastJobValue";
            lblLastJobValue.Size = new Size(145, 16);
            lblLastJobValue.TabIndex = 3;
            lblLastJobValue.Text = "-";
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(14, 39);
            label15.Name = "label15";
            label15.Size = new Size(59, 15);
            label15.TabIndex = 2;
            label15.Text = "Últ. job:";
            // 
            // lblLastHeartbeatValue
            // 
            lblLastHeartbeatValue.AutoEllipsis = true;
            lblLastHeartbeatValue.Location = new Point(14, 23);
            lblLastHeartbeatValue.Name = "lblLastHeartbeatValue";
            lblLastHeartbeatValue.Size = new Size(145, 16);
            lblLastHeartbeatValue.TabIndex = 1;
            lblLastHeartbeatValue.Text = "-";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(14, 8);
            label14.Name = "label14";
            label14.Size = new Size(88, 15);
            label14.TabIndex = 0;
            label14.Text = "Últ. heartbeat:";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(774, 694);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(lblStatusText);
            Controls.Add(lblStatusDot);
            Controls.Add(label8);
            Controls.Add(cmbRotation);
            Controls.Add(label7);
            Controls.Add(listBox1);
            Controls.Add(chkStartWithWindows);
            Controls.Add(btnStartListener);
            Controls.Add(btnTestPrint);
            Controls.Add(btnSaveConfig);
            Controls.Add(btnLoadPrinters);
            Controls.Add(cmbPrinters);
            Controls.Add(txtKioskId);
            Controls.Add(txtUnitId);
            Controls.Add(txtSupabaseKey);
            Controls.Add(txtSupabaseUrl);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "CBL Print Assistant";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffsetY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOffsetX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudBleed).EndInit();
            groupBox3.ResumeLayout(false);
            groupBox3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private TextBox txtSupabaseUrl;
        private TextBox txtSupabaseKey;
        private TextBox txtUnitId;
        private TextBox txtKioskId;
        private ComboBox cmbPrinters;
        private Button btnLoadPrinters;
        private Button btnSaveConfig;
        private Button btnTestPrint;
        private Button btnStartListener;
        private CheckBox chkStartWithWindows;
        private ListBox listBox1;
        private Label label7;
        private ComboBox cmbRotation;
        private Label label8;
        private Label lblStatusDot;
        private Label lblStatusText;
        private GroupBox groupBox1;
        private Label lblSystemPrinterValue;
        private Label label10;
        private Label lblAgentIdValue;
        private Label label9;
        private GroupBox groupBox2;
        private NumericUpDown nudOffsetY;
        private Label label13;
        private NumericUpDown nudOffsetX;
        private Label label12;
        private NumericUpDown nudBleed;
        private Label label11;
        private GroupBox groupBox3;
        private Label lblLastJobValue;
        private Label label15;
        private Label lblLastHeartbeatValue;
        private Label label14;
    }
}