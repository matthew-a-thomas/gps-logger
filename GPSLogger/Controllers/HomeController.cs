using Microsoft.AspNetCore.Mvc;

namespace GPSLogger.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
