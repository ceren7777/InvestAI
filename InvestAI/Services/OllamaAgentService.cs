using Microsoft.EntityFrameworkCore;
using InvestAI.Models;
using System.Text;
using System.Text.Json;

namespace InvestAI.Services
{
    public class AgentReport
    {
        public string Il { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public string EkonomiAnalizi { get; set; } = string.Empty;
        public string DogalKaynakAnalizi { get; set; } = string.Empty;
        public string AltyapiAnalizi { get; set; } = string.Empty;
        public string RiskAnalizi { get; set; } = string.Empty;
        public string FirsatAnalizi { get; set; } = string.Empty;
        public string SwotVeOneri { get; set; } = string.Empty;
        public decimal? YatirimSkoru { get; set; }
        public double SentimentSkoru { get; set; }
    }

    public class OllamaAgentService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private const string OllamaUrl = "http://localhost:11434/api/generate";
        private const string Model = "llama3.2:3b";

        public OllamaAgentService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        private async Task<string> AskAgentAsync(string prompt)
        {
            var body = new
            {
                model = Model,
                prompt = prompt,
                stream = false,
                options = new { num_predict = 150, temperature = 0.7 }
            };
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(OllamaUrl, content);
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(responseJson);
            return result.GetProperty("response").GetString() ?? "";
        }

        public async Task<AgentReport> AnalyzeAsync(string il, string ilce)
        {
            var data = await _context.InvestmentAnalysisView
                .Where(x => EF.Functions.ILike(x.Il, il) && EF.Functions.ILike(x.Ilce, ilce))
                .FirstOrDefaultAsync();

            if (data == null)
                return new AgentReport { Il = il, Ilce = ilce, SwotVeOneri = "Veri bulunamadı." };

            var baglamBase = $@"
İl: {data.Il}, İlçe: {data.Ilce}
Yatırım Skoru: {data.YatirimSkoru}
Maden Türleri: {data.MadenTurleri ?? "Yok"}
Başlı Ürünler: {data.BasliUrunler ?? "Yok"}
Dominant Sektör: {data.IlceDominantSektorAdi ?? "Bilinmiyor"}
Teşvik Bölgesi: {data.TesvikBolge}
OSB Sayısı: {data.OsbSayisi}
Maden Sayısı: {data.MadenSayisi}
";

            // 6 ajan paralel çalışsın
            var ekonomiTask = AskAgentAsync($"{baglamBase}\nEkonomist olarak bu ilin istihdam ve sektörel yapısını 3 cümleyle analiz et. Türkçe yaz.");
            var dogalKaynakTask = AskAgentAsync($"{baglamBase}\nDoğal kaynak uzmanı olarak maden ve tarım potansiyelini 3 cümleyle analiz et. Türkçe yaz.");
            var altyapiTask = AskAgentAsync($"{baglamBase}\nAltyapı uzmanı olarak OSB ve lojistik altyapısını 3 cümleyle analiz et. Türkçe yaz.");
            var riskTask = AskAgentAsync($"{baglamBase}\nRisk analisti olarak yatırım risklerini 3 cümleyle analiz et. Türkçe yaz.");
            var firsatTask = AskAgentAsync($"{baglamBase}\nFırsat analisti olarak yatırım fırsatlarını 3 cümleyle analiz et. Türkçe yaz.");

            await Task.WhenAll(ekonomiTask, dogalKaynakTask, altyapiTask, riskTask, firsatTask);

            var tumAnalizler = $@"
İl: {il}, İlçe: {ilce}
Ekonomi: {ekonomiTask.Result}
Doğal Kaynak: {dogalKaynakTask.Result}
Altyapı: {altyapiTask.Result}
Risk: {riskTask.Result}
Fırsat: {firsatTask.Result}
";
            var swotTask = await AskAgentAsync($"{tumAnalizler}\nYukarıdaki analizlere dayanarak kısa bir SWOT analizi ve yatırım önerisi yaz. Türkçe yaz.");

            return new AgentReport
            {
                Il = il,
                Ilce = ilce,
                EkonomiAnalizi = ekonomiTask.Result,
                DogalKaynakAnalizi = dogalKaynakTask.Result,
                AltyapiAnalizi = altyapiTask.Result,
                RiskAnalizi = riskTask.Result,
                FirsatAnalizi = firsatTask.Result,
                SwotVeOneri = swotTask,
                YatirimSkoru = data.YatirimSkoru,
                SentimentSkoru = 50
            };
        }
    }
}