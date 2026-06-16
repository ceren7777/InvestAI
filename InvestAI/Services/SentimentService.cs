using System.Text;
using System.Text.Json;

namespace InvestAI.Services
{
    public class SentimentResult
    {
        public string Il { get; set; } = string.Empty;
        public double SentimentScore { get; set; }
    }

    public class SentimentService
    {
        private readonly HttpClient _httpClient;
        private const string ServiceUrl = "http://localhost:8001/sentiment";

        public SentimentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SentimentResult> AnalyzeAsync(string il, List<string> haberler)
        {
            var requestBody = new
            {
                il = il,
                texts = haberler
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ServiceUrl, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return new SentimentResult
            {
                Il = il,
                SentimentScore = result.GetProperty("sentiment_score").GetDouble()
            };
        }
    }
}