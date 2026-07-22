using System;
using System.Drawing;
using System.Windows.Forms;

namespace CBL.PrintAssistant
{
    public partial class MainForm
    {
        private Icon? _brandIcon;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_brandIcon != null)
                return;

            try
            {
                using Icon? executableIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (executableIcon == null)
                    return;

                _brandIcon = (Icon)executableIcon.Clone();
                Icon = _brandIcon;

                if (_trayIcon != null)
                    _trayIcon.Icon = _brandIcon;
            }
            catch
            {
                // A identidade visual não pode impedir a inicialização do agente.
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            _brandIcon?.Dispose();
            _brandIcon = null;
        }
    }
}
