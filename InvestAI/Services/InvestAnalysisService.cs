using InvestAI.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Services
{
    public class AnalysisResult
    {
        public string Il { get; set; } = string.Empty;
        public string Ilce { get; set; } = string.Empty;
        public decimal? YatirimSkoru { get; set; }
        public double SentimentSkoru { get; set; }
        public double BirlesikSkor { get; set; }
        public string? MadenTurleri { get; set; }
        public string? BasliUrunler { get; set; }
        public string? DominantSektor { get; set; }
        public int? TesvikBolge { get; set; }
        public long? OsbSayisi { get; set; }
    }

    public class InvestAnalysisService
    {
        private readonly AppDbContext _context;
        private readonly SentimentService _sentimentService;

        public InvestAnalysisService(AppDbContext context, SentimentService sentimentService)
        {
            _context = context;
            _sentimentService = sentimentService;
        }

        public async Task<List<AnalysisResult>> GetTopAsync(int count = 10)
        {
            var scores = await _context.InvestmentAnalysisView
                .OrderByDescending(x => x.YatirimSkoru)
                .Take(count)
                .ToListAsync();

            var results = new List<AnalysisResult>();

            foreach (var s in scores)
            {
                double sentimentSkoru = 50;
                try
                {
                    var sentiment = await _sentimentService.AnalyzeAsync(s.Il, new List<string>
                    {
                        $"{s.Il} yatırım ekonomi sanayi"
                    });
                    sentimentSkoru = sentiment.SentimentScore;
                }
                catch { }

                var birlesik = ((double)(s.YatirimSkoru ?? 0) * 0.8) + (sentimentSkoru * 0.2);

                results.Add(new AnalysisResult
                {
                    Il = s.Il,
                    Ilce = s.Ilce,
                    YatirimSkoru = s.YatirimSkoru,
                    SentimentSkoru = sentimentSkoru,
                    BirlesikSkor = Math.Round(birlesik, 2),
                    MadenTurleri = s.MadenTurleri,
                    BasliUrunler = s.BasliUrunler,
                    DominantSektor = s.IlceDominantSektorAdi,
                    TesvikBolge = s.TesvikBolge,
                    OsbSayisi = s.OsbSayisi
                });
            }

            return results.OrderByDescending(r => r.BirlesikSkor).ToList();
        }

        public async Task<AnalysisResult?> GetByIlIlceAsync(string il, string ilce)
        {
            var s = await _context.InvestmentAnalysisView
                .Where(x => x.Il.ToLower() == il.ToLower() && x.Ilce.ToLower() == ilce.ToLower())
                .FirstOrDefaultAsync();

            if (s == null) return null;

            double sentimentSkoru = 50;
            try
            {
                var sentiment = await _sentimentService.AnalyzeAsync(il, new List<string>
                {
                    $"{il} yatırım ekonomi sanayi"
                });
                sentimentSkoru = sentiment.SentimentScore;
            }
            catch { }

            var birlesik = ((double)(s.YatirimSkoru ?? 0) * 0.8) + (sentimentSkoru * 0.2);

            return new AnalysisResult
            {
                Il = s.Il,
                Ilce = s.Ilce,
                YatirimSkoru = s.YatirimSkoru,
                SentimentSkoru = sentimentSkoru,
                BirlesikSkor = Math.Round(birlesik, 2),
                MadenTurleri = s.MadenTurleri,
                BasliUrunler = s.BasliUrunler,
                DominantSektor = s.IlceDominantSektorAdi,
                TesvikBolge = s.TesvikBolge,
                OsbSayisi = s.OsbSayisi
            };
        }
    }
}