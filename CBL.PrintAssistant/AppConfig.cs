namespace CBL.PrintAssistant
{
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; } = "";
        public string AgentId { get; set; } = "";
        public string AgentToken { get; set; } = "";
        public string UnitId { get; set; } = "";
        public string KioskId { get; set; } = "";
        public string PrinterName { get; set; } = "";
        public bool StartWithWindows { get; set; }

        public string RotationMode { get; set; } = "Automático";
        public int Bleed { get; set; } = 8;
        public int OffsetX { get; set; } = 0;
        public int OffsetY { get; set; } = 0;
    }
}