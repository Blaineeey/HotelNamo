using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using HotelNamo.Models;
using HotelNamo.Data;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

public class HomeController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    // Inject dependencies through the constructor
    public HomeController(
        IConfiguration configuration,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
        _configuration = configuration;
        _userManager = userManager;
        _context = context;
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
    public async Task<IActionResult> UserHome()
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Count bookings for the current user
        var bookingsCount = _context.Bookings
            .Count(b => b.UserId == user.Id);

        // Count feedback submitted by the current user
        var feedbackCount = _context.Feedbacks
            .Count(f => f.UserId == user.Id);

        // Get the user's most recent booking with room details
        var recentBooking = await _context.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == user.Id)
            .OrderByDescending(b => b.BookingDate)
            .FirstOrDefaultAsync();

        // Get recommended rooms (rooms with highest ratings or newest rooms)
        var recommendedRooms = await _context.Rooms
            .Include(r => r.RoomImages)
            .Include(r => r.Feedbacks)
            .Take(10) // Take more initially, then we'll sort client-side
            .AsNoTracking()
            .ToListAsync();

        // Now sort on the client side using the AverageRating property
        recommendedRooms = recommendedRooms
            .OrderByDescending(r => r.AverageRating)
            .ThenBy(r => Guid.NewGuid()) // Adding some randomness if ratings are equal
            .Take(2)
            .ToList();

        // Pass the data to the view
        ViewBag.BookingsCount = bookingsCount;
        ViewBag.FeedbackCount = feedbackCount;
        ViewBag.RecentBooking = recentBooking;
        ViewBag.RecommendedRooms = recommendedRooms;

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