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
    // Remove class-level Authorize attribute to allow guest payments
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
        [Authorize(Roles = "User")]
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

        [HttpGet]
        public async Task<IActionResult> PayGuest(int bookingId)
        {
            // Use UserId == null to identify guest bookings instead of IsGuestBooking property
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == null);

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

            ViewBag.GuestName = booking.GuestName;
            ViewBag.GuestEmail = booking.GuestEmail;

            return View("PayGuest", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ProcessPayment(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                return View("Pay", payment);
            }

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId);

            if (booking == null)
            {
                ModelState.AddModelError("", "Booking not found.");
                return View("Pay", payment);
            }

            // ✅ Process payment
            payment.IsPaid = true;
            payment.TransactionId = Guid.NewGuid().ToString();
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // ✅ Store Booking ID to use in Confirm step
            TempData["BookingId"] = booking.Id;

            // ✅ Redirect to Confirm Page after successful payment
            return RedirectToAction("Confirm", "Booking");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessGuestPayment(Payment payment)
        {
            if (!ModelState.IsValid)
            {
                var booking = await _context.Bookings.FindAsync(payment.BookingId);
                if (booking != null)
                {
                    ViewBag.GuestName = booking.GuestName;
                    ViewBag.GuestEmail = booking.GuestEmail;
                }
                return View("PayGuest", payment);
            }

            // Use UserId == null to identify guest bookings instead of IsGuestBooking property
            var guestBooking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == payment.BookingId && b.UserId == null);

            if (guestBooking == null)
            {
                ModelState.AddModelError("", "Booking not found.");
                return View("PayGuest", payment);
            }

            // Process payment
            payment.IsPaid = true;
            payment.TransactionId = Guid.NewGuid().ToString();
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Store Booking ID to use in Confirm step
            TempData["GuestBookingId"] = guestBooking.Id;
            TempData["GuestName"] = guestBooking.GuestName;
            TempData["GuestEmail"] = guestBooking.GuestEmail;

            // Redirect to Confirm Page after successful payment
            return RedirectToAction("Confirm", "Booking");
        }
    }
}