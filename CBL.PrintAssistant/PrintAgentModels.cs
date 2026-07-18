using System.Text.Json.Serialization;

namespace CBL.PrintAssistant
{
    public class PrintAgentRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "";

        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = "";

        [JsonPropertyName("agent_token")]
        public string AgentToken { get; set; } = "";

        [JsonPropertyName("machine_name")]
        public string? MachineName { get; set; }

        [JsonPropertyName("unit_id")]
        public string? UnitId { get; set; }

        [JsonPropertyName("kiosk_id")]
        public string? KioskId { get; set; }

        [JsonPropertyName("windows_printer_name")]
        public string? WindowsPrinterName { get; set; }

        [JsonPropertyName("app_version")]
        public string? AppVersion { get; set; }

        [JsonPropertyName("print_order_id")]
        public string? PrintOrderId { get; set; }

        [JsonPropertyName("copies_printed")]
        public int? CopiesPrinted { get; set; }

        [JsonPropertyName("error_message")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("retryable")]
        public bool? Retryable { get; set; }

        [JsonPropertyName("capabilities")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public PrintAgentCapabilities? Capabilities =>
            Action.Equals("register", StringComparison.OrdinalIgnoreCase) ||
            Action.Equals("heartbeat", StringComparison.OrdinalIgnoreCase)
                ? PrintAgentCapabilities.ContractV2
                : null;
    }

    public sealed class PrintAgentCapabilities
    {
        public static PrintAgentCapabilities ContractV2 { get; } = new PrintAgentCapabilities();

        [JsonPropertyName("contract_v2")]
        public bool ContractV2Enabled { get; init; } = true;
    }

    public class PrintAgentResponse
    {
        [JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [JsonPropertyName("claimed")]
        public bool? Claimed { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("job")]
        public PrintJobDto? Job { get; set; }

        [JsonPropertyName("agent")]
        public PrintAgentInfoDto? Agent { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class PrintAgentInfoDto
    {
        [JsonPropertyName("agent_id")]
        public string? AgentId { get; set; }

        [JsonPropertyName("printer_id")]
        public string? PrinterId { get; set; }

        [JsonPropertyName("printer_name")]
        public string? PrinterName { get; set; }

        [JsonPropertyName("paper_size")]
        public string? PaperSize { get; set; }

        [JsonPropertyName("dpi")]
        public int? Dpi { get; set; }
    }

    public class PrintJobDto
    {
        [JsonPropertyName("print_order_id")]
        public string PrintOrderId { get; set; } = "";

        [JsonPropertyName("capture_id")]
        public string? CaptureId { get; set; }

        [JsonPropertyName("session_id")]
        public string? SessionId { get; set; }

        [JsonPropertyName("unit_id")]
        public string? UnitId { get; set; }

        [JsonPropertyName("kiosk_id")]
        public string? KioskId { get; set; }

        [JsonPropertyName("printer_id")]
        public string? PrinterId { get; set; }

        [JsonPropertyName("printer_name")]
        public string? PrinterName { get; set; }

        [JsonPropertyName("windows_printer_name")]
        public string? WindowsPrinterName { get; set; }

        [JsonPropertyName("copies")]
        public int Copies { get; set; } = 1;

        [JsonPropertyName("paper_size")]
        public string? PaperSize { get; set; }

        [JsonPropertyName("orientation")]
        public string? Orientation { get; set; }

        [JsonPropertyName("rotation")]
        public string? Rotation { get; set; }

        [JsonPropertyName("fit")]
        public string? Fit { get; set; }

        [JsonPropertyName("dpi")]
        public int? Dpi { get; set; }

        [JsonPropertyName("bleed")]
        public int? Bleed { get; set; }

        [JsonPropertyName("margin_left")]
        public int? MarginLeft { get; set; }

        [JsonPropertyName("margin_top")]
        public int? MarginTop { get; set; }

        [JsonPropertyName("margin_right")]
        public int? MarginRight { get; set; }

        [JsonPropertyName("margin_bottom")]
        public int? MarginBottom { get; set; }

        [JsonPropertyName("offset_x")]
        public int? OffsetX { get; set; }

        [JsonPropertyName("offset_y")]
        public int? OffsetY { get; set; }

        [JsonPropertyName("is_test")]
        public bool? IsTest { get; set; }

        [JsonPropertyName("contract_version")]
        public int? ContractVersion { get; set; }

        [JsonPropertyName("image_url")]
        public string ImageUrl { get; set; } = "";

        [JsonPropertyName("print_type")]
        public string? PrintType { get; set; }

        [JsonPropertyName("print_category")]
        public string? PrintCategory { get; set; }
    }
}
