using HotelNamo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelNamo.Models;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "FrontDesk,Guest")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Process(int bookingId)
        {
            // Return a view with booking info
            return View(new PaymentViewModel { BookingId = bookingId });
        }

        [HttpPost]
        public async Task<IActionResult> Process(PaymentViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var payment = new Payment
            {
                BookingId = model.BookingId,
                Amount = model.Amount,
                PaymentDate = DateTime.Now,
                PaymentMethod = model.PaymentMethod
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Possibly mark booking as paid
            return RedirectToAction("Receipt", new { paymentId = payment.Id });
        }

        public async Task<IActionResult> Receipt(int paymentId)
        {
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null) return NotFound();
            return View(payment);
        }
    }

}
