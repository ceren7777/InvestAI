using System.Text.Json;
using InvestAI.Models;
using Microsoft.Extensions.Configuration;

namespace InvestAI.Services
{
    public class GoogleMapsService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public GoogleMapsService(
            HttpClient httpClient,
            AppDbContext context,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _context = context;
            _configuration = configuration;
        }

        public async Task CalculateDrivingDistancesAsync()
        {
            var districts = _context.DistrictLocations.Take(10).ToList();
            var airports = _context.AirportDatas
                .Where(x =>
                    !x.Name.Contains("Üssü") &&
                    !x.Name.Contains("Jet") &&
                    !x.Name.Contains("Heliport") &&
                    !x.Name.Contains("SEABIRD"))
                .Take(10)
                .ToList();
            string apiKey = _configuration["GoogleMaps:ApiKey"]!;

            foreach (var district in districts)
            {
                foreach (var airport in airports)
                {
                    string origins =
                        Uri.EscapeDataString($"{district.District}, Turkey");

                    string destinations =
                        Uri.EscapeDataString($"{airport.Name}, Turkey");

                    string url =
                        $"https://maps.googleapis.com/maps/api/distancematrix/json?origins={origins}&destinations={destinations}&key={apiKey}";

                    var json = await _httpClient.GetStringAsync(url);

                    using JsonDocument doc = JsonDocument.Parse(json);

                    var rows = doc.RootElement.GetProperty("rows");

                    var elements = rows[0].GetProperty("elements");

                    var element = elements[0];

                    string status = element.GetProperty("status").GetString()!;

                    if (status != "OK")
                    {
                        _context.GoogleRouteDistances.Add(new GoogleRouteDistance
                        {
                            OriginDistrict = district.District,
                            DestinationName = airport.Name,
                            DestinationType = "Airport",
                            DistanceKm = 0,
                            DurationMinutes = 0,
                            RetrievedAt = DateTime.UtcNow,
                            Source = "Google Maps API ERROR: " + status
                        });

                        continue;
                    }

                    double distanceMeters =
                        element.GetProperty("distance")
                            .GetProperty("value")
                            .GetDouble();

                    double durationSeconds =
                        element.GetProperty("duration")
                            .GetProperty("value")
                            .GetDouble();

                    double distanceKm =
                        Math.Round(distanceMeters / 1000, 2);

                    int durationMinutes =
                        (int)Math.Round(durationSeconds / 60);

                    bool exists =
                        _context.GoogleRouteDistances.Any(x =>
                            x.OriginDistrict == district.District &&
                            x.DestinationName == airport.Name);

                    if (!exists)
                    {
                        _context.GoogleRouteDistances.Add(
                            new GoogleRouteDistance
                            {
                                OriginDistrict = district.District,
                                DestinationName = airport.Name,
                                DestinationType = "Airport",
                                DistanceKm = distanceKm,
                                DurationMinutes = durationMinutes,
                                RetrievedAt = DateTime.UtcNow,
                                Source = "Google Maps API"
                            });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}