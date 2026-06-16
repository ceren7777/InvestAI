using InvestAI.Models;
using InvestAI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvestAI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvestAnalysisController : ControllerBase
    {
        private readonly InvestAnalysisService _service;
        private readonly AppDbContext _context;

        public InvestAnalysisController(InvestAnalysisService service, AppDbContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.InvestmentAnalysisView
                .OrderByDescending(x => x.YatirimSkoru)
                .ToListAsync();
            return Ok(data);
        }

        [HttpGet("iller")]
        public async Task<IActionResult> GetIller()
        {
            var iller = await _context.InvestmentAnalysisView
                .Select(x => x.Il)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            return Ok(iller);
        }

        [HttpGet("ilceler/{il}")]
        public async Task<IActionResult> GetIlceler(string il)
        {
            var ilceler = await _context.InvestmentAnalysisView
                .Where(x => x.Il == il)
                .Select(x => x.Ilce)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
            return Ok(ilceler);
        }

        [HttpGet("sektor-dagilim/{il}/{ilce}")]
        public async Task<IActionResult> GetSektorDagilim(string il, string ilce)
        {
            var sektorler = await _context.Database
                .SqlQueryRaw<SektorDagilimDto>(
                    @"SELECT sector_name AS ""SectorName"", 
                             employee_count AS ""EmployeeCount"", 
                             percentage AS ""Percentage""
                      FROM district_sector_distribution
                      WHERE il = {0} AND ilce = {1}
                      ORDER BY percentage DESC",
                    il, ilce)
                .ToListAsync();

            return Ok(sektorler);
        }
    }

    public class SektorDagilimDto
    {
        public string SectorName { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal Percentage { get; set; }
    }
}
