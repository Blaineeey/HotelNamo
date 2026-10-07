using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "User")]
    public IActionResult UserHome()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminHome()
    {
        ViewData["Layout"] = "_Layout2";
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
}
