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


    }
}
