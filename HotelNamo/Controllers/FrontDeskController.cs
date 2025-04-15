using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;

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

        // 7. Show walk-in booking form
        [HttpGet]
        public IActionResult WalkInBooking()
        {
            // Get available (vacant) rooms
            var availableRooms = _context.Rooms
                .Where(r => r.Status == "Vacant")
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomNumber} - {r.Category} (${r.Price}/night)"
                })
                .ToList();

            ViewBag.AvailableRooms = availableRooms;

            return View();
        }

        // 8. Handle walk-in booking submission
        [HttpPost]
        public IActionResult WalkInBooking(Booking booking, bool checkInNow = false)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get room details to calculate price
                    var room = _context.Rooms.Find(booking.RoomId);
                    if (room == null)
                    {
                        ModelState.AddModelError("RoomId", "Selected room not found.");
                        return GetWalkInBookingView();
                    }

                    // Calculate number of nights and total price
                    var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                    if (nights <= 0)
                    {
                        ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date.");
                        return GetWalkInBookingView();
                    }

                    booking.TotalPrice = room.Price * nights;

                    // Set as confirmed since it's created by staff
                    booking.IsConfirmed = true;
                    booking.BookingDate = DateTime.UtcNow;

                    // If checking in now, set the actual check-in time
                    if (checkInNow)
                    {
                        booking.ActualCheckInTime = DateTime.Now;

                        // Update room status to occupied
                        room.Status = "Occupied";
                    }

                    _context.Bookings.Add(booking);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Walk-in booking created successfully.";
                    return RedirectToAction("Bookings");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating booking: {ex.Message}");
                }
            }

            return GetWalkInBookingView();
        }

        // Helper method to prepare the walk-in booking view
        private IActionResult GetWalkInBookingView()
        {
            var availableRooms = _context.Rooms
                .Where(r => r.Status == "Vacant")
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomNumber} - {r.Category} (${r.Price}/night)"
                })
                .ToList();

            ViewBag.AvailableRooms = availableRooms;

            return View();
        }
    }
}