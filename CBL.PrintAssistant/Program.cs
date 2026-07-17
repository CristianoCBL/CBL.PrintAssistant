using System.Diagnostics;
using System.Threading;

namespace CBL.PrintAssistant
{
    internal static class Program
    {
        private const string SingleInstanceMutexName = "Global\\CBL.PrintAssistant";

        [STAThread]
        static void Main()
        {
            using var mutex = new Mutex(true, SingleInstanceMutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                MessageBox.Show(
                    "O CBL Print Assistant já está em execução. Verifique o ícone próximo ao relógio do Windows.",
                    "CBL Print Assistant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            ApplicationConfiguration.Initialize();

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                string logDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CBL.PrintAssistant",
                    "logs");
                Directory.CreateDirectory(logDirectory);

                string crashLog = Path.Combine(logDirectory, $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                File.WriteAllText(crashLog, ex.ToString());

                MessageBox.Show(
                    $"O aplicativo encontrou um erro inesperado.\n\nUm relatório foi salvo em:\n{crashLog}",
                    "CBL Print Assistant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
