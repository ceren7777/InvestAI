using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    [Authorize]
    public class YatirimSkorlariController : Controller
    {
        public IActionResult Index() => View();
    }
}