namespace CBL.PrintAssistant
{
    public static class AppPaths
    {
        public static string DataDirectory
        {
            get
            {
                string directory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "CBL.PrintAssistant");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public static string LogsDirectory
        {
            get
            {
                string directory = Path.Combine(DataDirectory, "logs");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public static string ResolveConfigPath(string startupPath)
        {
            string targetPath = Path.Combine(DataDirectory, "appconfig.json");
            string legacyPath = Path.Combine(startupPath, "appconfig.json");

            if (!File.Exists(targetPath) && File.Exists(legacyPath))
            {
                File.Copy(legacyPath, targetPath, overwrite: false);
            }

            return targetPath;
        }
    }
}
