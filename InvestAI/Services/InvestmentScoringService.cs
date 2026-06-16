using InvestAI.Models;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Services
{
    public class ScoringResult
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public float TotalScore { get; set; }
        public bool IsRecommended => TotalScore >= 50;
        public List<string> Rationale { get; set; } = new();
    }

    public interface IInvestmentScoringService
    {
        Task<List<ScoringResult>> ScoreRegionAsync(int regionId);
        Task<List<ScoringResult>> ScoreAllRegionsAsync();
        Task<List<InvestmentSuggestion>> GenerateSuggestionsAsync(int regionId);
    }

    public class InvestmentScoringService : IInvestmentScoringService
    {
        private readonly AppDbContext _context;

        public InvestmentScoringService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ScoringResult>> ScoreRegionAsync(int regionId)
        {
            var region = await _context.Regions
                .Include(r => r.RegionSectors).ThenInclude(rs => rs.Sector)
                .Include(r => r.RegionResources).ThenInclude(rr => rr.Resource)
                .FirstOrDefaultAsync(r => r.Id == regionId)
                ?? throw new KeyNotFoundException($"Bölge bulunamadı: {regionId}");

            return EvaluateAllSuggestions(region);
        }

        public async Task<List<ScoringResult>> ScoreAllRegionsAsync()
        {
            var regions = await _context.Regions
                .Include(r => r.RegionSectors).ThenInclude(rs => rs.Sector)
                .Include(r => r.RegionResources).ThenInclude(rr => rr.Resource)
                .ToListAsync();

            return regions
                .SelectMany(EvaluateAllSuggestions)
                .OrderByDescending(r => r.TotalScore)
                .ToList();
        }

        public async Task<List<InvestmentSuggestion>> GenerateSuggestionsAsync(int regionId)
        {
            var results = await ScoreRegionAsync(regionId);

            var region = await _context.Regions
                .Include(r => r.InvestmentSuggestions)
                .FirstAsync(r => r.Id == regionId);

            // Mevcut kural tabanlı önerileri temizle
            var existing = region.InvestmentSuggestions
                .Where(s => s.Title.StartsWith("[KURAL]"))
                .ToList();
            _context.InvestmentSuggestions.RemoveRange(existing);

            var suggestions = results.Select(r => new InvestmentSuggestion
            {
                RegionId    = r.RegionId,
                Title       = $"[KURAL] {r.Title}",
                Score       = r.TotalScore,
                Description = $"{r.RegionName} bölgesi için '{r.Title}' önerisi {r.TotalScore}/100 puan aldı.",
                Rationale   = string.Join(" | ", r.Rationale)
            }).ToList();

            await _context.InvestmentSuggestions.AddRangeAsync(suggestions);
            await _context.SaveChangesAsync();

            return suggestions;
        }

        // ---------------------------------------------------------------
        // Bir bölge için 5 öneri tipini değerlendir, >= 50 olanları döndür
        // ---------------------------------------------------------------
        private static List<ScoringResult> EvaluateAllSuggestions(Region region)
        {
            var all = new List<ScoringResult>
            {
                ScoreSeramikYanSanayi(region),
                ScoreMadencilikDestekHizmetleri(region),
                ScoreSanayiLojistikMerkezi(region),
                ScoreEnerjiDestekHizmetleri(region),
                ScoreYapiMalzemeleriUretimi(region)
            };

            return all
                .Where(r => r.IsRecommended)
                .OrderByDescending(r => r.TotalScore)
                .Take(5)
                .ToList();
        }

        // ---------------------------------------------------------------
        // 1. Seramik Yan Sanayi (max 100 puan)
        // ---------------------------------------------------------------
        private static ScoringResult ScoreSeramikYanSanayi(Region region)
        {
            var result = NewResult(region, "Seramik Yan Sanayi");
            int score = 0;

            var seramik = region.RegionSectors
                .FirstOrDefault(rs => rs.Sector.Name.Contains("Seramik", StringComparison.OrdinalIgnoreCase));
            if (seramik != null && seramik.Strength > 0)
            {
                score += 30;
                result.Rationale.Add($"Seramik sektörü mevcut (Strength: {seramik.Strength}): +30");
            }

            if (region.RegionResources.Any(rr => rr.Resource.Type.Equals("hammadde", StringComparison.OrdinalIgnoreCase)))
            {
                score += 25;
                result.Rationale.Add("Hammadde kaynağı mevcut: +25");
            }

            if (region.InfrastructureScore >= 60)
            {
                score += 20;
                result.Rationale.Add($"Altyapı skoru {region.InfrastructureScore} >= 60: +20");
            }

            if (region.Workforce >= 5000)
            {
                score += 15;
                result.Rationale.Add($"İşgücü {region.Workforce:N0} >= 5000: +15");
            }

            if (region.RegionResources.Any(rr => rr.Resource.Type.Equals("enerji", StringComparison.OrdinalIgnoreCase)))
            {
                score += 10;
                result.Rationale.Add("Enerji kaynağı mevcut: +10");
            }

            result.TotalScore = score;
            result.Rationale.Add($"Toplam: {score}/100");
            return result;
        }

        // ---------------------------------------------------------------
        // 2. Madencilik Destek Hizmetleri (max 100 puan)
        // ---------------------------------------------------------------
        private static ScoringResult ScoreMadencilikDestekHizmetleri(Region region)
        {
            var result = NewResult(region, "Madencilik Destek Hizmetleri");
            int score = 0;

            var madencilik = region.RegionSectors
                .FirstOrDefault(rs => rs.Sector.Name.Contains("Madencilik", StringComparison.OrdinalIgnoreCase));
            if (madencilik != null && madencilik.Strength > 0)
            {
                score += 35;
                result.Rationale.Add($"Madencilik sektörü mevcut (Strength: {madencilik.Strength}): +35");
            }

            if (region.RegionResources.Any(rr => rr.Resource.Type.Equals("maden", StringComparison.OrdinalIgnoreCase)))
            {
                score += 30;
                result.Rationale.Add("Maden kaynağı mevcut: +30");
            }

            if (region.InfrastructureScore >= 65)
            {
                score += 20;
                result.Rationale.Add($"Altyapı skoru {region.InfrastructureScore} >= 65: +20");
            }

            if (region.Workforce >= 3000)
            {
                score += 15;
                result.Rationale.Add($"İşgücü {region.Workforce:N0} >= 3000: +15");
            }

            result.TotalScore = score;
            result.Rationale.Add($"Toplam: {score}/100");
            return result;
        }

        // ---------------------------------------------------------------
        // 3. Sanayi Lojistik Merkezi (max 100 puan)
        // ---------------------------------------------------------------
        private static ScoringResult ScoreSanayiLojistikMerkezi(Region region)
        {
            var result = NewResult(region, "Sanayi Lojistik Merkezi");
            int score = 0;

            if (region.InfrastructureScore >= 70)
            {
                score += 30;
                result.Rationale.Add($"Altyapı skoru {region.InfrastructureScore} >= 70: +30");
            }

            if (region.RegionSectors.Count >= 2)
            {
                score += 25;
                result.Rationale.Add($"Sektör sayısı {region.RegionSectors.Count} >= 2: +25");
            }

            if (region.Population >= 20000)
            {
                score += 25;
                result.Rationale.Add($"Nüfus {region.Population:N0} >= 20000: +25");
            }

            if (region.RegionResources.Any(rr => rr.Resource.Type.Equals("enerji", StringComparison.OrdinalIgnoreCase)))
            {
                score += 20;
                result.Rationale.Add("Enerji kaynağı mevcut: +20");
            }

            result.TotalScore = score;
            result.Rationale.Add($"Toplam: {score}/100");
            return result;
        }

        // ---------------------------------------------------------------
        // 4. Enerji Destek Hizmetleri (max 100 puan)
        // ---------------------------------------------------------------
        private static ScoringResult ScoreEnerjiDestekHizmetleri(Region region)
        {
            var result = NewResult(region, "Enerji Destek Hizmetleri");
            int score = 0;

            var enerji = region.RegionSectors
                .FirstOrDefault(rs => rs.Sector.Name.Contains("Enerji", StringComparison.OrdinalIgnoreCase));
            if (enerji != null && enerji.Strength > 0)
            {
                score += 40;
                result.Rationale.Add($"Enerji sektörü mevcut (Strength: {enerji.Strength}): +40");
            }

            if (region.RegionSectors.Count >= 2)
            {
                score += 30;
                result.Rationale.Add($"Sektör sayısı {region.RegionSectors.Count} >= 2: +30");
            }

            if (region.InfrastructureScore >= 50)
            {
                score += 30;
                result.Rationale.Add($"Altyapı skoru {region.InfrastructureScore} >= 50: +30");
            }

            result.TotalScore = score;
            result.Rationale.Add($"Toplam: {score}/100");
            return result;
        }

        // ---------------------------------------------------------------
        // 5. Yapı Malzemeleri Üretimi (max 100 puan)
        // ---------------------------------------------------------------
        private static ScoringResult ScoreYapiMalzemeleriUretimi(Region region)
        {
            var result = NewResult(region, "Yapı Malzemeleri Üretimi");
            int score = 0;

            bool hasSeramikVeyaMadencilik = region.RegionSectors.Any(rs =>
                rs.Sector.Name.Contains("Seramik", StringComparison.OrdinalIgnoreCase) ||
                rs.Sector.Name.Contains("Madencilik", StringComparison.OrdinalIgnoreCase));

            if (hasSeramikVeyaMadencilik)
            {
                score += 30;
                result.Rationale.Add("Seramik veya Madencilik sektörü mevcut: +30");
            }

            if (region.RegionResources.Any(rr => rr.Resource.Type.Equals("hammadde", StringComparison.OrdinalIgnoreCase)))
            {
                score += 30;
                result.Rationale.Add("Hammadde kaynağı mevcut: +30");
            }

            if (region.InfrastructureScore >= 50)
            {
                score += 20;
                result.Rationale.Add($"Altyapı skoru {region.InfrastructureScore} >= 50: +20");
            }

            if (region.Workforce >= 4000)
            {
                score += 20;
                result.Rationale.Add($"İşgücü {region.Workforce:N0} >= 4000: +20");
            }

            result.TotalScore = score;
            result.Rationale.Add($"Toplam: {score}/100");
            return result;
        }

        private static ScoringResult NewResult(Region region, string title) => new()
        {
            RegionId   = region.Id,
            RegionName = region.Name,
            Title      = title
        };
    }
}
