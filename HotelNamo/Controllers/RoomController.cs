using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class RoomController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Anyone can view rooms (no login required)
    [AllowAnonymous]
    public IActionResult Index()
    {
        // Show all rooms or just those not under maintenance
        var rooms = _context.Rooms.ToList();
        return View(rooms);
    }

    // Optionally let users see details for a specific room
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var room = _context.Rooms.Find(id);
        if (room == null) return NotFound();
        return View(room);
    }

    // Only Admin or FrontDesk can create
    [Authorize(Roles = "Admin,FrontDesk")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin,FrontDesk")]
    [HttpPost]
    public async Task<IActionResult> Create(Room room)
    {
        if (!ModelState.IsValid) return View(room);
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // Similarly implement Edit, Delete with [Authorize(Roles = "Admin,FrontDesk")]
}
