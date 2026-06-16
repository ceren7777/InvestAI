using InvestAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OllamaAgentController : ControllerBase
    {
        private readonly OllamaAgentService _service;

        public OllamaAgentController(OllamaAgentService service)
        {
            _service = service;
        }

        [HttpGet("{il}/{ilce}")]
        public async Task<IActionResult> Analyze(string il, string ilce)
        {
            var result = await _service.AnalyzeAsync(il, ilce);
            return Ok(result);
        }
    }
}