using InvestAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace InvestAI.Controllers
{
    [Authorize]
    public class GoogleMapsController : Controller
    {
        private readonly GoogleMapsService _googleMapsService;

        public GoogleMapsController(GoogleMapsService googleMapsService)
        {
            _googleMapsService = googleMapsService;
        }

        [HttpGet]
        public async Task<IActionResult> CalculateDrivingDistances()
        {
            await _googleMapsService.CalculateDrivingDistancesAsync();

            return Content("Google Maps driving distances imported successfully.");
        }
    }
}