using System.Drawing;
using System.Windows.Forms;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private static readonly Color UiBackground = Color.FromArgb(244, 247, 250);
        private static readonly Color UiSurface = Color.White;
        private static readonly Color UiBorder = Color.FromArgb(218, 224, 230);
        private static readonly Color UiPrimary = Color.FromArgb(20, 94, 120);
        private static readonly Color UiPrimaryHover = Color.FromArgb(15, 78, 101);
        private static readonly Color UiText = Color.FromArgb(31, 41, 55);
        private static readonly Color UiMuted = Color.FromArgb(100, 116, 139);

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
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
                MinimumSize = new Size(Math.Max(MinimumSize.Width, 960), Math.Max(MinimumSize.Height, 720));
                Text = "CBL Print Assistant";
                ApplyModernStyleRecursive(this);

                if (listBox1 != null)
                {
                    listBox1.BackColor = Color.FromArgb(17, 24, 39);
                    listBox1.ForeColor = Color.FromArgb(229, 231, 235);
                    listBox1.BorderStyle = BorderStyle.FixedSingle;
                    listBox1.Font = new Font("Consolas", 8.75F, FontStyle.Regular, GraphicsUnit.Point);
                }

                groupNormal.Text = "  Foto Normal  ";
                groupStrip.Text = "  Tirinha  ";
                btnStartAll.Text = GetSelectedRunMode() switch
                {
                    ProfileStrip => "▶  Iniciar Tirinha",
                    ModeBoth => "▶  Iniciar Ambos",
                    _ => "▶  Iniciar Normal"
                };
                btnStopAll.Text = "■  Parar";
                btnSaveConfig.Text = "Salvar configuração";
                btnSyncFromSite.Text = "Sincronizar do site";
                btnCheckUpdates.Text = "Verificar atualização";
            }
            finally
            {
                ResumeLayout(true);
            }
        }

        private void ApplyModernStyleRecursive(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                switch (control)
                {
                    case GroupBox group:
                        group.BackColor = UiSurface;
                        group.ForeColor = UiText;
                        group.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
                        break;

                    case Button button:
                        button.FlatStyle = FlatStyle.Flat;
                        button.FlatAppearance.BorderSize = 0;
                        button.BackColor = IsPrimaryAction(button) ? UiPrimary : Color.FromArgb(229, 235, 240);
                        button.ForeColor = IsPrimaryAction(button) ? Color.White : UiText;
                        button.Cursor = Cursors.Hand;
                        button.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Regular, GraphicsUnit.Point);
                        button.FlatAppearance.MouseOverBackColor = IsPrimaryAction(button)
                            ? UiPrimaryHover
                            : Color.FromArgb(215, 224, 232);
                        break;

                    case TextBox textBox:
                        textBox.BackColor = UiSurface;
                        textBox.ForeColor = UiText;
                        textBox.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case ComboBox combo:
                        combo.BackColor = UiSurface;
                        combo.ForeColor = UiText;
                        combo.FlatStyle = FlatStyle.Flat;
                        break;

                    case NumericUpDown numeric:
                        numeric.BackColor = UiSurface;
                        numeric.ForeColor = UiText;
                        numeric.BorderStyle = BorderStyle.FixedSingle;
                        break;

                    case CheckBox check:
                        check.ForeColor = UiText;
                        check.UseVisualStyleBackColor = false;
                        check.BackColor = parent is GroupBox ? UiSurface : UiBackground;
                        break;

                    case Label label:
                        if (!label.Name.EndsWith("StatusDot", StringComparison.OrdinalIgnoreCase))
                            label.ForeColor = label.Name.Contains("Status", StringComparison.OrdinalIgnoreCase) ? UiMuted : UiText;
                        label.BackColor = Color.Transparent;
                        break;

                    case Panel panel:
                        panel.BackColor = parent is GroupBox ? UiSurface : UiBackground;
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
