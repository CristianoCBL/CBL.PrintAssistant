namespace CBL.PrintAssistant
{
    public sealed class EffectivePrintSettings
    {
        public string PrinterName { get; init; } = "";
        public string PaperName { get; init; } = "";
        public string Orientation { get; init; } = "Automático";
        public string RotationMode { get; init; } = "Automático";
        public string FitMode { get; init; } = "Cover";
        public int Dpi { get; init; } = 300;
        public int Bleed { get; init; }
        public int MarginLeft { get; init; }
        public int MarginTop { get; init; }
        public int MarginRight { get; init; }
        public int MarginBottom { get; init; }
        public int OffsetX { get; init; }
        public int OffsetY { get; init; }
        public int Copies { get; init; } = 1;
        public bool IsTest { get; init; }
        public int ContractVersion { get; init; } = 1;
    }
}
