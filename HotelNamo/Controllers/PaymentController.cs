using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "User")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Pay(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == _userManager.GetUserId(User));

            if (booking == null)
            {
                return NotFound();
            }

            var model = new Payment
            {
                BookingId = booking.Id,
                Amount = booking.TotalPrice,
                PaymentMethod = "Credit Card"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(Payment payment)
        {
            Console.WriteLine("🚀 Payment process started...");

            // Debug: Log ModelState errors
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ Invalid ModelState - Returning to payment page");

                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"🔴 Error in {key}: {error.ErrorMessage}");
                    }
                }

                return View("Pay", payment);
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            if (booking == null)
            {
                Console.WriteLine("❌ Booking not found!");
                ModelState.AddModelError("", "Booking not found.");
                return View("Pay", payment);
            }

            Console.WriteLine($"🔍 Processing payment for Booking ID: {booking.Id}, Amount: {payment.Amount}");

            // Simulate successful transaction
            payment.IsPaid = true;
            payment.TransactionId = Guid.NewGuid().ToString();
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);

            // ✅ Confirm booking after payment
            booking.IsConfirmed = true;
            Console.WriteLine("✅ Booking status changed to Confirmed");

            // ✅ Update room status
            if (booking.Room != null)
            {
                booking.Room.Status = "Occupied";
                Console.WriteLine($"🏠 Room {booking.Room.RoomNumber} marked as Occupied");
            }

            await _context.SaveChangesAsync();
            Console.WriteLine("✅ Database changes saved successfully");

            // ✅ Redirect back to MyBookings after payment
            return RedirectToAction("MyBookings", "Booking");
        }

    }
}
