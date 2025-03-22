using Microsoft.AspNetCore.Mvc;

namespace HotelNamo.Controllers
{
    public class AmenitiesController : Controller
    {
        // Main Amenities Index page
        public IActionResult Index()
        {
            return View();
        }

        // Swimming Pool page
        public IActionResult Pool()
        {
            return View();
        }

        // Fitness Center page
        public IActionResult Fitness()
        {
            return View();
        }

        // Spa page
        public IActionResult Spa()
        {
            return View();
        }

        // Main Food directory
        public IActionResult Food()
        {
            return View();
        }

        // Dining Reservation page
        public IActionResult Reservation()
        {
            return View();
        }
    }
}