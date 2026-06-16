using System.Text.Json;
using InvestAI.Models;

namespace InvestAI.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _context;

        public WeatherService(HttpClient httpClient, AppDbContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task FetchWeatherForDistrictsAsync()
        {
            var districts = _context.DistrictLocations
                .Where(x => x.Latitude != 0 && x.Longitude != 0)
                .Take(50)
                .ToList();

            foreach (var district in districts)
            {
                string latitude = district.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
                string longitude = district.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);

                string url =
                    $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current_weather=true";

                var json = await _httpClient.GetStringAsync(url);

                using JsonDocument doc = JsonDocument.Parse(json);

                var current = doc.RootElement.GetProperty("current_weather");

                double temperature = current.GetProperty("temperature").GetDouble();
                double windSpeed = current.GetProperty("windspeed").GetDouble();

                _context.WeatherDatas.Add(new WeatherData
                {
                    City = district.City,
                    District = district.District,
                    Latitude = district.Latitude,
                    Longitude = district.Longitude,
                    Temperature = temperature,
                    WindSpeed = windSpeed,
                    RetrievedAt = DateTime.UtcNow,
                    Source = "Open-Meteo API"
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}