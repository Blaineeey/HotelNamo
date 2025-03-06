using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace HotelNamo.Controllers
{
    // No [Authorize(Roles=...)] so normal users or guests can see it
    public class UserRoomController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserRoomController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /UserRoom
        // Show only rooms that are vacant/available
        public IActionResult Index()
        {
            var rooms = _context.Rooms
                .Where(r => r.Status == "Vacant")  // or "Available"
                .OrderBy(r => r.RoomNumber)
                .ToList();

            return View(rooms);  // calls Views/UserRoom/Index.cshtml
        }

        // Optional: Single room details
        [HttpGet]
        public IActionResult Details(int id)
        {
            var room = _context.Rooms.FirstOrDefault(r => r.Id == id && r.Status == "Vacant");
            if (room == null) return NotFound();

            return View(room); // calls Views/UserRoom/Details.cshtml
        }
    }
}
