using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelNamo.Controllers
{
    // Allows normal users (role = "User") and front desk staff (role = "FrontDesk") to book rooms
    [Authorize(Roles = "User,FrontDesk")]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Booking/Create?roomId=5 (optional roomId)
        [HttpGet]
        public IActionResult Create(int? roomId)
        {
            // Pre-fill with a default date range
            var model = new Booking
            {
                RoomId = roomId ?? 0,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1)
            };
            return View(model);
        }

        // POST: /Booking/Create
        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.GetUserAsync(User);
            var booking = new Booking
            {
                RoomId = vm.RoomId,
                CheckInDate = vm.CheckInDate,
                CheckOutDate = vm.CheckOutDate,
                UserId = user.Id,
                IsConfirmed = false
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("MyBookings");
        }


        // Only front desk staff can check in
        [Authorize(Roles = "FrontDesk")]
        public async Task<IActionResult> CheckIn(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.IsConfirmed = true;
            booking.ActualCheckInTime = DateTime.Now;

            // Update the room status to "Occupied"
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null)
                room.Status = "Occupied";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // Only front desk staff can check out
        [Authorize(Roles = "FrontDesk")]
        public async Task<IActionResult> CheckOut(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null) return NotFound();

            booking.ActualCheckOutTime = DateTime.Now;

            // Update the room status to "Vacant"
            var room = await _context.Rooms.FindAsync(booking.RoomId);
            if (room != null)
                room.Status = "Vacant";

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // Normal users can see their own bookings
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = _context.Bookings
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.Id)
                .ToList();

            return View(bookings);
        }

        // Helper to check if a room is free for the given date range
        private bool CheckRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !_context.Bookings
                .Any(b => b.RoomId == roomId
                       && b.IsConfirmed
                       && checkIn < b.CheckOutDate
                       && checkOut > b.CheckInDate);
        }
    }
}
