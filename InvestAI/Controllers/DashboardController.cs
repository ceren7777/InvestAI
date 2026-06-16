using InvestAI.Models;
using InvestAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IInvestmentScoringService _scoringService;
        private readonly AppDbContext _context;
        private readonly IWikidataService _wikidata;

        public DashboardController(
            IInvestmentScoringService scoringService,
            AppDbContext context,
            IWikidataService wikidata)
        {
            _scoringService = scoringService;
            _context        = context;
            _wikidata       = wikidata;
        }
        public IActionResult DogalgazTest()
        {
            var data = _context.DogalgazAnaHat
                .Take(10)
                .ToList();

            return Json(data);
        }
        public async Task<IActionResult> Index(int regionId = 1)
        {
            var regions = await _context.Regions.ToListAsync();
            var currentRegion = await _context.Regions
                .Include(r => r.RegionSectors)
                    .ThenInclude(rs => rs.Sector)
                .Include(r => r.RegionResources)
                    .ThenInclude(rr => rr.Resource)
                .FirstOrDefaultAsync(r => r.Id == regionId);

            var scoringResults = await _scoringService.ScoreRegionAsync(regionId);

            var swot = await _context.SwotAnalyses
                .FirstOrDefaultAsync(s => s.RegionId == regionId);

            // Wikidata: WikidataId varsa direkt sorgula, güncelle ve kaydet
            if (currentRegion != null && !string.IsNullOrWhiteSpace(currentRegion.WikidataId))
            {
                WikidataResult? wikidataResult = null;
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    wikidataResult = await _wikidata.GetCityInfoAsync(
                        currentRegion.Name,
                        currentRegion.City,
                        currentRegion.WikidataId)
                        .WaitAsync(cts.Token);
                }
                catch (Exception)
                {
                    // Timeout veya API erişilemezse sessizce devam et
                }

                if (wikidataResult != null)
                {
                    bool changed = false;

                    if (wikidataResult.Population.HasValue)
                    {
                        currentRegion.Population          = (int)wikidataResult.Population.Value;
                        currentRegion.PopulationUpdatedAt = DateTime.UtcNow;
                        changed = true;
                    }

                    if (wikidataResult.Latitude.HasValue)
                    {
                        currentRegion.Latitude = wikidataResult.Latitude.Value;
                        changed = true;
                    }

                    if (wikidataResult.Longitude.HasValue)
                    {
                        currentRegion.Longitude = wikidataResult.Longitude.Value;
                        changed = true;
                    }

                    if (changed)
                        await _context.SaveChangesAsync();
                }
            }

            ViewBag.Regions          = regions;
            ViewBag.CurrentRegion    = currentRegion;
            ViewBag.ScoringResults   = scoringResults;
            ViewBag.SelectedRegionId = regionId;
            ViewBag.Swot             = swot;

            return View();
        }
    }
}
