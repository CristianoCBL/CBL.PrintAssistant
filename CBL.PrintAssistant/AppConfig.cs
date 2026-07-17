namespace CBL.PrintAssistant
{
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; } = "";
        public string UnitId { get; set; } = "";
        public string KioskId { get; set; } = "";
        public bool StartWithWindows { get; set; }
        public bool AutoStartListening { get; set; } = true;
        public bool EnableLocalIntegration { get; set; } = true;
        public bool AllowJobOverrides { get; set; } = true;
        public string RunMode { get; set; } = "Normal";

        public PrintProfileConfig NormalProfile { get; set; } = new PrintProfileConfig();
        public PrintProfileConfig StripProfile { get; set; } = new PrintProfileConfig();
    }

    public class PrintProfileConfig
    {
        public string AgentId { get; set; } = "";
        public string AgentToken { get; set; } = "";
        public string PrinterName { get; set; } = "";
        public string PaperName { get; set; } = "";
        public string Orientation { get; set; } = "Automático";
        public string RotationMode { get; set; } = "Automático";
        public string FitMode { get; set; } = "Cover";
        public int Dpi { get; set; } = 300;
        public int Bleed { get; set; } = 8;
        public int MarginLeft { get; set; } = 0;
        public int MarginTop { get; set; } = 0;
        public int MarginRight { get; set; } = 0;
        public int MarginBottom { get; set; } = 0;
        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
    }
}
