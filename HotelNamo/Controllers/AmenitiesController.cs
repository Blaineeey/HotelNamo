using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace HotelNamo.Controllers
{
    public class AmenitiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AmenitiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Main Amenities Index page
        public IActionResult Index()
        {
            return View();
        }

        // Swimming Pool page
        public IActionResult Pool()
        {
            return View();
        }

        // Fitness Center page
        public IActionResult Fitness()
        {
            return View();
        }

        // Spa page - GET
        public IActionResult Spa()
        {
            return View();
        }

        // Spa page - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Spa(SpaBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Create a new SpaBooking entity from the view model
                var spaBooking = new SpaBooking
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    NumberOfGuests = model.NumberOfGuests,
                    PreferredDate = model.PreferredDate,
                    PreferredTime = model.PreferredTime,
                    Treatment = model.Treatment,
                    SpecialRequests = model.SpecialRequests,
                    BookingDate = DateTime.Now,
                    Status = "Confirmed"
                };

                // If user is authenticated, associate booking with the user
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        spaBooking.UserId = user.Id;
                    }
                }

                // Add to database
                _context.SpaBookings.Add(spaBooking);
                await _context.SaveChangesAsync();

                // Store booking details in TempData to display on confirmation page
                TempData["BookingId"] = spaBooking.Id;
                TempData["BookingName"] = spaBooking.FullName;
                TempData["BookingDate"] = spaBooking.PreferredDate.ToString("dddd, MMMM d, yyyy");
                TempData["BookingTime"] = spaBooking.PreferredTime;
                TempData["BookingTreatment"] = spaBooking.Treatment;

                // Redirect to confirmation page
                return RedirectToAction("SpaBookingConfirmation");
            }

            // If we got this far, something failed; redisplay form
            return View(model);
        }

        // Spa Booking Confirmation page
        public IActionResult SpaBookingConfirmation()
        {
            // Check if we have booking data in TempData
            if (TempData["BookingName"] == null)
            {
                return RedirectToAction("Spa");
            }

            // You could also fetch the booking from the database using the ID if needed
            if (TempData.ContainsKey("BookingId"))
            {
                int bookingId = (int)TempData["BookingId"];
                // You can use the booking ID here if needed
            }

            return View();
        }

        // Main Food directory
        public IActionResult Food()
        {
            return View();
        }

        // Dining Reservation page
        public IActionResult Reservation()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [Route("ManageSpaBookings")]
        // Admin action to view all spa bookings (you can add this to an admin controller instead)
        public IActionResult ManageSpaBookings()
        {
            // This would typically be protected with [Authorize(Roles = "Admin")]
            var bookings = _context.SpaBookings.ToList();
            return View(bookings);
        }
    }
}