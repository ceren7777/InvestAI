using InvestAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    public class TcmbController : Controller
    {
        private readonly TcmbService _tcmbService;

        public TcmbController(TcmbService tcmbService)
        {
            _tcmbService = tcmbService;
        }

        [HttpGet]
        public async Task<IActionResult> FetchExchangeRates()
        {
            await _tcmbService.FetchExchangeRatesAsync();

            return Content("TCMB exchange rates imported successfully.");
        }
    }
}