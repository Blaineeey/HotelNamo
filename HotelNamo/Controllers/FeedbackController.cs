using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public IActionResult Submit()
        {
            return View(new Feedback()); // ✅ Ensures model is initialized
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Feedback feedback)
        {
            // ✅ Get the logged-in user's ID
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["ErrorMessage"] = "You must be logged in to submit feedback.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Assign User ID before validation
            feedback.UserId = user.Id;
            feedback.DateSubmitted = DateTime.UtcNow;

            // ✅ Prevent UserId validation errors
            ModelState.Remove("UserId");

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Validation failed: Please fill all required fields.";
                return View(feedback);
            }

            try
            {
                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Your feedback has been successfully submitted!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting feedback. Please try again.";
                Console.WriteLine($"🔥 ERROR: {ex.Message}");
            }

            return RedirectToAction("Submit");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult FeedbackList()
        {
            var feedbacks = _context.Feedbacks
                .OrderByDescending(f => f.DateSubmitted)
                .ToList();

            return View(feedbacks);
        }
    }
}
