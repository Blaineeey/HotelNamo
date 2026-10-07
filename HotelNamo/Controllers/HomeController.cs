using System.Diagnostics;
using HotelNamo.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelNamo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Rooms()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Services()
        {
            return View();
        }
        public IActionResult Booking()
        {
            return View();
        }
        public IActionResult Payment()
        {
            return View();
        }
        public IActionResult Billing()
        {
            return View();
        }
        public IActionResult Reservation()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
