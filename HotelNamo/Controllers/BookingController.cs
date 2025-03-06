using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "Guest,FrontDesk")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Maybe pass a list of vacant rooms or a date range
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Booking model)
        {
            if (!ModelState.IsValid) return View(model);

            // Check if room is available for the date range
            bool isAvailable = CheckRoomAvailability(model.RoomId, model.CheckInDate, model.CheckOutDate);
            if (!isAvailable)
            {
                ModelState.AddModelError("", "Room is not available for the selected dates.");
                return View(model);
            }

            // Assign the user who is booking
            var user = await _userManager.GetUserAsync(User);
            model.UserId = user.Id;
            model.IsConfirmed = false;

            _context.Bookings.Add(model);
            await _context.SaveChangesAsync();

            // Optionally update room status to "Occupied" if immediate check-in
            return RedirectToAction("Index", "Home");
        }

        private bool CheckRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            // Query Bookings to see if there's an overlap
            var overlappingBooking = _context.Bookings
                .Where(b => b.RoomId == roomId && b.IsConfirmed == true)
                .Where(b => checkIn < b.CheckOutDate && checkOut > b.CheckInDate)
                .FirstOrDefault();

            return overlappingBooking == null; // If null, means no overlap, so it's available
        }
        [Authorize(Roles = "FrontDesk")]
        public async Task<IActionResult> CheckIn(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.IsConfirmed = true;
            booking.ActualCheckInTime = DateTime.Now;

            // Update Room status
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null) room.Status = "Occupied";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "FrontDesk")]
        public async Task<IActionResult> CheckOut(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.ActualCheckOutTime = DateTime.Now;

            // Update Room status
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null) room.Status = "Vacant";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
