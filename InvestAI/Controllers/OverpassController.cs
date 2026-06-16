using InvestAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    public class OverpassController : Controller
    {
        private readonly OverpassService _overpassService;

        public OverpassController(OverpassService overpassService)
        {
            _overpassService = overpassService;
        }

        [HttpGet]
        public async Task<IActionResult> FetchPorts()
        {
            await _overpassService.FetchPortsAsync();

            return Content("Port data imported successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> FetchDistrictLocations()
        {
            await _overpassService.FetchDistrictLocationsAsync();

            return Content("District locations imported successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> CalculateAirportDistances()
        {
            await _overpassService.CalculateDistrictAirportDistancesAsync();

            return Content("District airport distances calculated successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> CalculatePortDistances()
        {
            await _overpassService.CalculateDistrictPortDistancesAsync();

            return Content("District port distances calculated successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> FetchRailways()
        {
            await _overpassService.FetchRailwaysAsync();

            return Content("Railway data imported successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> CalculateRailwayDistances()
        {
            await _overpassService.CalculateDistrictRailwayDistancesAsync();

            return Content("District railway distances calculated successfully.");
        }
    }
}