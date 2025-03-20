using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
                .Include(r => r.RoomImages)
                .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
                .Include(r => r.Feedbacks)
                .Where(r => r.Status == "Vacant")
                .ToList();

            return View(rooms);
        }

        // Optional: Single room details
        [HttpGet]
        public IActionResult Details(int id)
        {
            var room = _context.Rooms
                .Include(r => r.RoomAmenities)
                .ThenInclude(ra => ra.Amenity)
                .Include(r => r.RoomImages)
                .Include(r => r.Feedbacks)
                    .ThenInclude(f => f.User) 
                .Include(r => r.Feedbacks)
                    .ThenInclude(f => f.Booking)
                .FirstOrDefault(r => r.Id == id);

            if (room == null)
                return NotFound();

            return View(room);
        }
    }
}
