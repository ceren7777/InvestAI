using InvestAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;

        public WeatherController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        [HttpGet]
        public async Task<IActionResult> FetchWeather()
        {
            await _weatherService.FetchWeatherForDistrictsAsync();

            return Content("Weather data imported successfully.");
        }
    }
}