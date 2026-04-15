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
    }
}