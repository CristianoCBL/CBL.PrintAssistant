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

        /// <summary>
        ///  Required method for Designer support.
        /// </summary>
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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(24, 24);
            label1.Name = "label1";
            label1.Size = new Size(76, 15);
            label1.TabIndex = 0;
            label1.Text = "API Base URL";
            label1.Click += lblSupabaseUrl_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(24, 84);
            label2.Name = "label2";
            label2.Size = new Size(74, 15);
            label2.TabIndex = 1;
            label2.Text = "Agent Token";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(24, 144);
            label3.Name = "label3";
            label3.Size = new Size(43, 15);
            label3.TabIndex = 2;
            label3.Text = "Unit ID";
            label3.Click += label3_Click;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(24, 204);
            label4.Name = "label4";
            label4.Size = new Size(49, 15);
            label4.TabIndex = 3;
            label4.Text = "Kiosk ID";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(24, 264);
            label5.Name = "label5";
            label5.Size = new Size(65, 15);
            label5.TabIndex = 4;
            label5.Text = "Impressora";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(24, 380);
            label6.Name = "label6";
            label6.Size = new Size(31, 15);
            label6.TabIndex = 5;
            label6.Text = "Logs";
            // 
            // txtSupabaseUrl
            // 
            txtSupabaseUrl.Location = new Point(24, 42);
            txtSupabaseUrl.Name = "txtSupabaseUrl";
            txtSupabaseUrl.Size = new Size(520, 23);
            txtSupabaseUrl.TabIndex = 6;
            txtSupabaseUrl.TextChanged += txtSupabaseUrl_TextChanged;
            // 
            // txtSupabaseKey
            // 
            txtSupabaseKey.Location = new Point(24, 102);
            txtSupabaseKey.Name = "txtSupabaseKey";
            txtSupabaseKey.Size = new Size(520, 23);
            txtSupabaseKey.TabIndex = 7;
            txtSupabaseKey.UseSystemPasswordChar = true;
            // 
            // txtUnitId
            // 
            txtUnitId.Location = new Point(24, 162);
            txtUnitId.Name = "txtUnitId";
            txtUnitId.Size = new Size(250, 23);
            txtUnitId.TabIndex = 8;
            txtUnitId.TextChanged += textBox3_TextChanged;
            // 
            // txtKioskId
            // 
            txtKioskId.Location = new Point(24, 222);
            txtKioskId.Name = "txtKioskId";
            txtKioskId.Size = new Size(250, 23);
            txtKioskId.TabIndex = 9;
            // 
            // cmbPrinters
            // 
            cmbPrinters.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPrinters.FormattingEnabled = true;
            cmbPrinters.Location = new Point(24, 282);
            cmbPrinters.Name = "cmbPrinters";
            cmbPrinters.Size = new Size(520, 23);
            cmbPrinters.TabIndex = 10;
            // 
            // btnLoadPrinters
            // 
            btnLoadPrinters.Location = new Point(580, 40);
            btnLoadPrinters.Name = "btnLoadPrinters";
            btnLoadPrinters.Size = new Size(170, 28);
            btnLoadPrinters.TabIndex = 11;
            btnLoadPrinters.Text = "Carregar Impressoras";
            btnLoadPrinters.UseVisualStyleBackColor = true;
            btnLoadPrinters.Click += btnLoadPrinters_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(580, 88);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(170, 28);
            btnSaveConfig.TabIndex = 12;
            btnSaveConfig.Text = "Salvar Configuração";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // btnTestPrint
            // 
            btnTestPrint.Location = new Point(580, 136);
            btnTestPrint.Name = "btnTestPrint";
            btnTestPrint.Size = new Size(170, 28);
            btnTestPrint.TabIndex = 13;
            btnTestPrint.Text = "Teste de Impressão";
            btnTestPrint.UseVisualStyleBackColor = true;
            btnTestPrint.Click += btnTestPrint_Click;
            // 
            // btnStartListener
            // 
            btnStartListener.Location = new Point(580, 184);
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
            chkStartWithWindows.Location = new Point(24, 322);
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
            listBox1.Location = new Point(24, 398);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(726, 214);
            listBox1.TabIndex = 16;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 641);
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
    }
}