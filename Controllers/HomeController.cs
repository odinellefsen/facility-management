using Microsoft.AspNetCore.Mvc;

namespace FacilityManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}