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
                string? crashLog = null;

                try
                {
                    crashLog = Path.Combine(
                        AppPaths.LogsDirectory,
                        $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                    File.WriteAllText(crashLog, ex.ToString());
                }
                catch
                {
                    // O relatório é auxiliar; uma falha de disco não deve ocultar o erro original.
                }

                string details = crashLog is null
                    ? "Não foi possível salvar o relatório local."
                    : $"Um relatório foi salvo em:\n{crashLog}";

                MessageBox.Show(
                    $"O aplicativo encontrou um erro inesperado.\n\n{details}",
                    "CBL Print Assistant",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
