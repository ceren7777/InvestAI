using InvestAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InvestAI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class YatirimAIController : Controller
    {
        private readonly HttpClient _http;
        private readonly AppDbContext _db;

        public YatirimAIController(IHttpClientFactory factory, AppDbContext db)
        {
            _http = factory.CreateClient();
            _db = db;
        }

        [HttpGet("/YatirimAI")]
        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost("analiz")]
        public async Task<IActionResult> Analiz([FromBody] AnalizRequest req)
        {
            try
            {
                var skor = await _db.InvestScoreViews
                    .Where(x => EF.Functions.ILike(x.Il, req.Il))
                    .OrderByDescending(x => x.YatirimSkoru)
                    .FirstOrDefaultAsync();

                if (skor == null)
                    return NotFound($"İl bulunamadı: {req.Il}");

                var prompt = $"""
Sen InvestAI adlı bölgesel yatırım karar destek sisteminin yapay zeka asistanısın.

Görevin:
Türkiye'deki şehirleri yatırım potansiyeli açısından profesyonel bir yatırım danışmanı gibi analiz etmek.

Kurallar:
- Cevap tamamen Türkçe olacak.
- Verilen veriler dışında bilgi uydurma.
- Sayısal verileri sadece tekrar etme, yatırımcı açısından yorumla.
- Kısa ve boş cevap verme.
- Tüm başlıkları doldur.
- Cevap 180-260 kelime arasında olsun.
- Markdown işaretleri kullanma.
- Yıldız, kare işareti ve kod bloğu işareti kullanma.
- Her sektörün neden öne çıktığını açıkla.
- Son bölümde net yatırım kararı ver.

Kullanıcı sorusu:
{req.Soru}

Analiz edilen il:
{skor.Il}

Veriler:
Genel yatırım skoru: {skor.YatirimSkoru}/100
Öne çıkan sektör: {skor.IlceDominantSektorAdi ?? skor.IlDominantSektorAdi ?? "Belirsiz"}
Teşvik bölgesi: {skor.TesvikBolge}
OSB sayısı: {skor.OsbSayisi}
Maden türleri: {skor.MadenTurleri ?? "Bulunmuyor"}
Başlıca tarım ürünleri: {skor.BasliUrunler ?? "Bulunmuyor"}
Maden skoru: {skor.MadenSkoru}/100
Enerji skoru: {skor.EnerjiSkoru}/100

Cevabı tam olarak şu formatta yaz:

GENEL DEĞERLENDİRME:
İlin genel yatırım potansiyelini 2-3 cümleyle açıkla.

ÖNE ÇIKAN SEKTÖRLER:
1. Birinci sektör: Neden uygun olduğunu açıkla.
2. İkinci sektör: Neden uygun olduğunu açıkla.
3. Üçüncü sektör: Neden uygun olduğunu açıkla.

AVANTAJLAR:
1. Birinci avantajı açıkla.
2. İkinci avantajı açıkla.
3. Üçüncü avantajı açıkla.

RİSKLER:
1. Birinci riski açıkla.
2. İkinci riski açıkla.

YATIRIM ÖNERİSİ:
1. Birinci öneriyi yaz.
2. İkinci öneriyi yaz.
3. Üçüncü öneriyi yaz.

YATIRIM KARARI:
Uygun Yatırım Bölgesi, Dikkatli Değerlendirilmeli veya Yüksek Riskli Bölge seçeneklerinden birini seç ve gerekçesini açıkla.

SONUÇ:
Yatırımcı açısından kısa ve net sonuç yaz.
""";

                var body = JsonSerializer.Serialize(new { prompt });

                var content = new StringContent(
                    body,
                    Encoding.UTF8,
                    "application/json"
                );

                _http.Timeout = TimeSpan.FromMinutes(5);

                var response = await _http.PostAsync(
                    "http://localhost:8002/ai/analyze",
                    content
                );

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(
                        500,
                        $"Ollama servis hatası: {response.StatusCode}"
                    );
                }

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<OllamaResult>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }
                );

                var yorum = result?.Result;

                if (string.IsNullOrWhiteSpace(yorum))
                {
                    return Ok(new
                    {
                        il = req.Il,
                        ilce = skor.Ilce,
                        soru = req.Soru,
                        yorum = "AI servisinden boş cevap döndü. Lütfen tekrar deneyin."
                    });
                }

                return Ok(new
                {
                    il = req.Il,
                    ilce = skor.Ilce,
                    soru = req.Soru,
                    yorum = yorum.Trim()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Hata: {ex.Message}");
            }
        }

        public class AnalizRequest
        {
            public string Il { get; set; } = "";
            public string Soru { get; set; } = "";
        }

        public class OllamaResult
        {
            [JsonPropertyName("result")]
            public string? Result { get; set; }
        }
    }
}