using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "Admin,Housekeeping,FrontDesk")]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var requests = _context.MaintenanceRequests.Include(m => m.Room).ToList();
            return View(requests);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Possibly pass a list of rooms
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(MaintenanceRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            request.RequestDate = DateTime.Now;
            _context.MaintenanceRequests.Add(request);

            // Optionally update room status to "Under Maintenance"
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room != null) room.Status = "Maintenance";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Resolve(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();
            request.IsResolved = true;

            // If resolved, set room back to "Vacant" if no other occupant
            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room != null) room.Status = "Vacant";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }

}
