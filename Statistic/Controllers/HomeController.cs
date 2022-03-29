using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Statistic.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                ViewData["IsAuthenticated"] = "true";
                ViewData["Email"] = User.Identity.Name;
            }

            return View();
        }
        
    }
}