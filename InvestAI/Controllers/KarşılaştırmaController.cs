using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("Karşılaştırma")]
[Authorize]
public class KarsilastirmaController : Controller
{
    [Route("")]
    public IActionResult Index()
    {
        return View();
    }
}