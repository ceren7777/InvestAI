using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InvestAI.Controllers
{
    [Authorize]
    public class HaberAnaliziController : Controller
    {
        private static readonly HttpClient _http = new HttpClient()
        {
            Timeout = TimeSpan.FromSeconds(300)
        };

        public HaberAnaliziController() { }

        public IActionResult Index() => View();

        [HttpGet("/api/HaberAnalizi/news/{il}")]
        public async Task<IActionResult> GetNews(string il)
        {
            try
            {
                var res = await _http.GetAsync($"http://127.0.0.1:8001/sentiment/news/{Uri.EscapeDataString(il)}");
                var json = await res.Content.ReadAsStringAsync();
                return Content(json, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(503, ex.Message + " | " + ex.InnerException?.Message);
            }
        }
    }
}