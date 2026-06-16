using InvestAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvestmentAnalysisController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvestmentAnalysisController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.InvestmentAnalysisView
                .OrderByDescending(x => x.YatirimSkoru)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("top")]
        public async Task<IActionResult> GetTop()
        {
            var data = await _context.InvestmentAnalysisView
                .OrderByDescending(x => x.YatirimSkoru)
                .Take(10)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("locations/{il}")]
        public async Task<IActionResult> GetLocations(string il)
        {
            var ilceler = await _context.PopulationGrowths
                .Where(x => x.City == il)
                .Select(x => x.District)
                .Distinct()
                .ToListAsync();

            var locations = await _context.DistrictLocations
                .Where(x => ilceler.Contains(x.District))
                .ToListAsync();

            return Ok(locations);
        }
    }
}