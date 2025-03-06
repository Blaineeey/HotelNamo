using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "FrontDesk,Admin")]
    public class FrontDeskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FrontDeskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Show all bookings
        public IActionResult Bookings()
        {
            var bookings = _context.Bookings
                .Include(b => b.Room)
                .OrderByDescending(b => b.Id)
                .ToList();

            return View(bookings);
        }

        // 2. Confirm or change status of booking
        [HttpPost]
        public IActionResult ConfirmBooking(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.IsConfirmed = true;
            _context.SaveChanges();

            return RedirectToAction("Bookings");
        }

        // 3. Check In
        [HttpPost]
        public IActionResult CheckIn(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            // Mark booking as confirmed (if not already)
            booking.IsConfirmed = true;
            booking.ActualCheckInTime = DateTime.Now;

            // Update room status
            var room = _context.Rooms.Find(booking.RoomId);
            if (room != null)
            {
                room.Status = "Occupied";
            }

            _context.SaveChanges();
            return RedirectToAction("Bookings");
        }

        // 4. Check Out
        [HttpPost]
        public IActionResult CheckOut(int bookingId)
        {
            var booking = _context.Bookings.Find(bookingId);
            if (booking == null) return NotFound();

            booking.ActualCheckOutTime = DateTime.Now;

            // Update room status
            var room = _context.Rooms.Find(booking.RoomId);
            if (room != null)
            {
                room.Status = "Vacant";
            }

            _context.SaveChanges();
            return RedirectToAction("Bookings");
        }

        // 5. Show all rooms
        public IActionResult Rooms()
        {
            var rooms = _context.Rooms.ToList();
            return View(rooms);
        }

        // 6. Possibly allow front desk to update room status
        [HttpPost]
        public IActionResult UpdateRoomStatus(int roomId, string newStatus)
        {
            var room = _context.Rooms.Find(roomId);
            if (room == null) return NotFound();

            room.Status = newStatus;
            _context.SaveChanges();

            return RedirectToAction("Rooms");
        }
    }
}
