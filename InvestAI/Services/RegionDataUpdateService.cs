using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using InvestAI.Models;

namespace InvestAI.Services
{
    public class RegionDataUpdateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RegionDataUpdateService> _logger;

        private static readonly TimeSpan UpdateInterval = TimeSpan.FromHours(24);

        public RegionDataUpdateService(
            IServiceScopeFactory scopeFactory,
            ILogger<RegionDataUpdateService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger       = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("=== RegionDataUpdateService başladı ===");
            Console.WriteLine("[RegionDataUpdateService] Servis başlatıldı.");

            // Uygulama açılışında hemen çalıştır
            await RunUpdateAsync(stoppingToken);

            // Sonra her 24 saatte bir tekrar et
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(UpdateInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[RegionDataUpdateService] Servis durduruldu.");
                    break;
                }

                await RunUpdateAsync(stoppingToken);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        private async Task RunUpdateAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"[RegionDataUpdateService] Güncelleme başladı: {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}");

            // ── Adım 1: Bölge ID'lerini ayrı bir scope'ta al ──────────────
            List<int> regionIds;
            try
            {
                using var listScope = _scopeFactory.CreateScope();
                var listContext = listScope.ServiceProvider.GetRequiredService<AppDbContext>();
                regionIds = await listContext.Regions
                    .Select(r => r.Id)
                    .ToListAsync(stoppingToken);
                Console.WriteLine($"[RegionDataUpdateService] {regionIds.Count} bölge bulundu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegionDataUpdateService] HATA — DB okuma: {ex.Message}");
                _logger.LogError(ex, "Bölgeler veritabanından alınırken hata oluştu.");
                return;
            }

            // ── Adım 2: Her bölge için yeni scope → yeni AppDbContext ─────
            int updatedCount = 0;

            foreach (var regionId in regionIds)
            {
                if (stoppingToken.IsCancellationRequested) break;

                using (var scope = _scopeFactory.CreateScope())
                {
                    var context  = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var wikidata = scope.ServiceProvider.GetRequiredService<IWikidataService>();

                    var region = await context.Regions.FindAsync(new object[] { regionId }, stoppingToken);
                    if (region == null) continue;

                    WikidataResult? result = null;
                    try
                    {
                        result = await wikidata.GetCityInfoAsync(region.Name, region.City, region.WikidataId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RegionDataUpdateService] HATA — Wikidata sorgusu '{region.Name}': {ex.Message}");
                        _logger.LogWarning(ex, "Wikidata sorgusu başarısız: {City}", region.Name);
                        continue;
                    }

                    if (result == null)
                    {
                        Console.WriteLine($"[RegionDataUpdateService] Sonuç bulunamadı: '{region.Name}'");
                        continue;
                    }

                    bool changed = false;

                    // Nüfus: Wikidata verisi daha büyükse güncelle
                    if (result.Population.HasValue && result.Population.Value > region.Population)
                    {
                        Console.WriteLine($"[RegionDataUpdateService] {region.Name}: Nüfus {region.Population} → {result.Population.Value}");
                        region.Population          = (int)result.Population.Value;
                        region.PopulationUpdatedAt = DateTime.UtcNow;
                        changed = true;
                    }

                    // Koordinatlar: 0 ise veya anlamlı fark varsa güncelle
                    if (result.Latitude.HasValue && result.Longitude.HasValue)
                    {
                        var newLat = result.Latitude.Value;
                        var newLon = result.Longitude.Value;

                        if (region.Latitude == 0 || region.Longitude == 0 ||
                            Math.Abs(region.Latitude  - newLat) > 0.0001 ||
                            Math.Abs(region.Longitude - newLon) > 0.0001)
                        {
                            Console.WriteLine($"[RegionDataUpdateService] {region.Name}: Koordinat ({newLat:F4}, {newLon:F4})");
                            region.Latitude  = newLat;
                            region.Longitude = newLon;
                            changed = true;
                        }
                    }

                    if (!changed) continue;

                    try
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        updatedCount++;
                        Console.WriteLine($"[RegionDataUpdateService] {region.Name}: kaydedildi.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[RegionDataUpdateService] HATA — SaveChanges '{region.Name}': {ex.Message}");
                        _logger.LogError(ex, "Değişiklikler kaydedilirken hata oluştu: {Region}", region.Name);
                    }
                }
            }

            Console.WriteLine($"[RegionDataUpdateService] Güncelleme tamamlandı. {updatedCount} bölge değişti.");
            _logger.LogInformation("Bölge verisi güncelleme tamamlandı. {Count} bölge değişti.", updatedCount);
        }
    }
}
