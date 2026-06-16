using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    [Authorize]
    public class KategorilerController : Controller
    {
        public IActionResult Index() => View();
    }
}