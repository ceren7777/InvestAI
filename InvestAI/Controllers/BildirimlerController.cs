using InvestAI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InvestAI.Controllers
{
    [Authorize]
    public class BildirimlerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public BildirimlerController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}