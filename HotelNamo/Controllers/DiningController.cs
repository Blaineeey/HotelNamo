using Microsoft.AspNetCore.Mvc;

namespace HotelNamo.Controllers
{
    public class DiningController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
