using InvestAI.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    public class WorldBankController : Controller
    {
        private readonly WorldBankService _worldBankService;

        public WorldBankController(WorldBankService worldBankService)
        {
            _worldBankService = worldBankService;
        }

        [HttpGet]
        public async Task<IActionResult> FetchGDP()
        {
            await _worldBankService.FetchIndicatorDataAsync(
                "GDP Growth",
                "NY.GDP.MKTP.KD.ZG"
            );

            return Content("World Bank GDP data imported successfully.");
        }

        [HttpGet]
        public async Task<IActionResult> FetchAll()
        {
            await _worldBankService.FetchAllWorldBankIndicatorsAsync();

            return Content("All World Bank indicators imported successfully.");
        }
    }
}
