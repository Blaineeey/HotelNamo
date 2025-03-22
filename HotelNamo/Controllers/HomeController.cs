using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;

    // Inject IConfiguration through the constructor
    public HomeController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IActionResult Index()
    {
        return View();
    }

    [Authorize(Roles = "Admin")]
    public IActionResult AdminHome()
    {
        return RedirectToAction("Index", "Admin");
    }

    [Authorize(Roles = "User")]
    public IActionResult UserHome()
    {
        return View();
    }

    public IActionResult About()
    {
        return View();
    }

    public IActionResult Contact()
    {
        // Pass the Google Maps API key to the view using the injected configuration
        ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"];
        return View();
    }
}