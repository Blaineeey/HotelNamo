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
        return View();
    }
}
