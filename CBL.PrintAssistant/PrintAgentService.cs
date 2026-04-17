using System.Text;
using System.Text.Json;

namespace CBL.PrintAssistant
{
    public class PrintAgentService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<PrintAgentResponse?> SendAsync(string apiUrl, PrintAgentRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync(apiUrl, content);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {body}");

            return JsonSerializer.Deserialize<PrintAgentResponse>(body);
        }
    }
}