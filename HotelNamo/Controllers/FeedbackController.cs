using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelNamo.Data;
using HotelNamo.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace HotelNamo.Controllers
{
    [Authorize]
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FeedbackController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int? bookingId)
        {
            // Initialize an empty feedback model
            var feedback = new Feedback();

            // If a booking ID was provided, load booking details
            if (bookingId.HasValue)
            {
                var userId = _userManager.GetUserId(User);

                // Get the booking with room details
                var booking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

                if (booking != null)
                {
                    // Check if booking is eligible for feedback
                    bool isEligible = booking.IsConfirmed &&
                                      booking.ActualCheckOutTime.HasValue;

                    // Check if feedback already exists
                    var existingFeedback = await _context.Feedbacks
                        .FirstOrDefaultAsync(f => f.BookingId == booking.Id);

                    if (!isEligible)
                    {
                        TempData["ErrorMessage"] = "This booking is not eligible for feedback yet.";
                    }
                    else if (existingFeedback != null)
                    {
                        TempData["ErrorMessage"] = "You've already submitted feedback for this booking.";
                    }
                    else
                    {
                        // Pre-populate the feedback model with booking data
                        feedback.BookingId = booking.Id;
                        feedback.RoomId = booking.RoomId;

                        // Pass booking info to view
                        ViewBag.BookingDetails = booking;
                        ViewBag.RoomNumber = booking.Room?.RoomNumber;
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Booking not found or you don't have permission to submit feedback for it.";
                }
            }

            return View(feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Feedback feedback)
        {
            // Check required fields manually to avoid validation errors
            if (feedback.Rating < 1 || feedback.Rating > 5)
            {
                ModelState.AddModelError("Rating", "Rating is required and must be between 1 and 5 stars.");
            }

            if (string.IsNullOrWhiteSpace(feedback.Review))
            {
                ModelState.AddModelError("Review", "Please provide your review.");
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to submit feedback.";
                return RedirectToAction("Login", "Account");
            }

            // First remove validation for navigation properties and ID fields
            // This needs to happen BEFORE model validation!
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("Booking");
            ModelState.Remove("Room");

            // Assign User ID and timestamp
            feedback.UserId = userId;
            feedback.DateSubmitted = DateTime.UtcNow;

            // Load the booking to verify it belongs to user and is eligible
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == feedback.BookingId && b.UserId == userId);

            if (booking == null)
            {
                TempData["ErrorMessage"] = "Invalid booking.";
                return View(feedback);
            }

            // Check if booking is eligible for feedback
            if (!booking.IsConfirmed || booking.ActualCheckOutTime == null)
            {
                TempData["ErrorMessage"] = "This booking is not eligible for feedback yet.";
                return View(feedback);
            }

            // Check if feedback already exists
            var existingFeedback = await _context.Feedbacks
                .FirstOrDefaultAsync(f => f.BookingId == feedback.BookingId);

            if (existingFeedback != null)
            {
                TempData["ErrorMessage"] = "You've already submitted feedback for this booking.";
                return View(feedback);
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill all required fields.";
                ViewBag.BookingDetails = booking;
                ViewBag.RoomNumber = booking.Room?.RoomNumber;
                return View(feedback);
            }

            try
            {
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your feedback has been successfully submitted! Thank you for sharing your experience.";
                return RedirectToAction("MyBookings", "Booking"); // Redirect to booking list
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting feedback. Please try again.";
                Console.WriteLine($"🔥 ERROR: {ex.Message}");

                ViewBag.BookingDetails = booking;
                ViewBag.RoomNumber = booking.Room?.RoomNumber;
                return View(feedback);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.User)
                .Include(f => f.Room)
                .Include(f => f.Booking)
                .OrderByDescending(f => f.DateSubmitted)
                .ToListAsync();

            return View(feedbacks);
        }
    }
}