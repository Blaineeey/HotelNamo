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
    public class DiningController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiningController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Dining/Reservations
        public IActionResult Reservations()
        {
            return View();
        }

        // GET: Dining/ReserveTable
        public async Task<IActionResult> ReserveTable()
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new TableReservationViewModel
            {
                ReservationDate = DateTime.Now.Date.AddDays(1),
                NumberOfGuests = 2,
                ReservationTime = "19:00"
            };

            if (user != null)
            {
                model.FullName = $"{user.FirstName} {user.LastName}";
                model.Email = user.Email;
            }

            return View(model);
        }

        // POST: Dining/ReserveTable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReserveTable(TableReservationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                var reservation = new TableReservation
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    NumberOfGuests = model.NumberOfGuests,
                    ReservationDate = model.ReservationDate,
                    ReservationTime = model.ReservationTime,
                    TableNumber = model.TableNumber,
                    Venue = model.Venue,
                    SpecialRequests = model.SpecialRequests,
                    Occasion = model.Occasion,
                    DietaryRestrictions = model.DietaryRestrictions,
                    BookingDate = DateTime.Now,
                    Status = "Confirmed",
                    UserId = user?.Id
                };

                _context.TableReservations.Add(reservation);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Confirmation), new { id = reservation.Id });
            }

            return View(model);
        }

        // GET: Dining/Confirmation/{id}
        public async Task<IActionResult> Confirmation(int id)
        {
            var reservation = await _context.TableReservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Dining/CheckAvailability
        [HttpGet]
        public JsonResult CheckAvailability(DateTime date, string time, string venue)
        {
            // Get all reservations for the selected date, time, and venue
            var reservations = _context.TableReservations
                .Where(r => r.ReservationDate.Date == date.Date &&
                       r.ReservationTime == time &&
                       r.Venue == venue &&
                       r.Status == "Confirmed")
                .Select(r => r.TableNumber)
                .ToList();

            return Json(new { reservedTables = reservations });
        }

        // GET: Dining/MyReservations
        [Authorize]
        public async Task<IActionResult> MyReservations()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var reservations = await _context.TableReservations
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            return View(reservations);
        }

        // GET: Dining/Cancel/{id}
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.TableReservations.FindAsync(id);

            if (reservation == null || (user.Id != reservation.UserId && !User.IsInRole("Admin")))
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Dining/CancelConfirmed/{id}
        [HttpPost, ActionName("CancelConfirmed")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var reservation = await _context.TableReservations.FindAsync(id);

            if (reservation != null)
            {
                reservation.Status = "Cancelled";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyReservations));
        }

        // Add these methods to your DiningController.cs

        // GET: Dining/Modify/{id}
        [Authorize]
        public async Task<IActionResult> Modify(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var reservation = await _context.TableReservations.FindAsync(id);

            if (reservation == null || (user.Id != reservation.UserId && !User.IsInRole("Admin")))
            {
                return NotFound();
            }

            // Only allow modification of future reservations
            var reservationDateTime = reservation.ReservationDate.Date.Add(TimeSpan.Parse(reservation.ReservationTime));
            if (reservationDateTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Past reservations cannot be modified.";
                return RedirectToAction(nameof(MyReservations));
            }

            return View(reservation);
        }

        // POST: Dining/ModifyConfirmed/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ModifyConfirmed(int id, TableReservation modifiedReservation)
        {
            if (id != modifiedReservation.Id)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var originalReservation = await _context.TableReservations.FindAsync(id);

            if (originalReservation == null || (user.Id != originalReservation.UserId && !User.IsInRole("Admin")))
            {
                return NotFound();
            }

            // Only allow modification of future reservations
            var reservationDateTime = originalReservation.ReservationDate.Date.Add(TimeSpan.Parse(originalReservation.ReservationTime));
            if (reservationDateTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Past reservations cannot be modified.";
                return RedirectToAction(nameof(MyReservations));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the values that can be modified
                    originalReservation.FullName = modifiedReservation.FullName;
                    originalReservation.Email = modifiedReservation.Email;
                    originalReservation.Phone = modifiedReservation.Phone;
                    originalReservation.NumberOfGuests = modifiedReservation.NumberOfGuests;
                    originalReservation.ReservationDate = modifiedReservation.ReservationDate;
                    originalReservation.ReservationTime = modifiedReservation.ReservationTime;
                    originalReservation.TableNumber = modifiedReservation.TableNumber;
                    originalReservation.Venue = modifiedReservation.Venue;
                    originalReservation.SpecialRequests = modifiedReservation.SpecialRequests;
                    originalReservation.Occasion = modifiedReservation.Occasion;
                    originalReservation.DietaryRestrictions = modifiedReservation.DietaryRestrictions;

                    // Check availability if table, date, time or venue has changed
                    if (originalReservation.TableNumber != modifiedReservation.TableNumber ||
                        originalReservation.ReservationDate != modifiedReservation.ReservationDate ||
                        originalReservation.ReservationTime != modifiedReservation.ReservationTime ||
                        originalReservation.Venue != modifiedReservation.Venue)
                    {
                        // Check if the table is available
                        var reservedTables = await _context.TableReservations
                            .Where(r => r.Id != id && // Exclude current reservation
                                   r.ReservationDate.Date == modifiedReservation.ReservationDate.Date &&
                                   r.ReservationTime == modifiedReservation.ReservationTime &&
                                   r.Venue == modifiedReservation.Venue &&
                                   r.Status == "Confirmed")
                            .Select(r => r.TableNumber)
                            .ToListAsync();

                        if (reservedTables.Contains(modifiedReservation.TableNumber))
                        {
                            ModelState.AddModelError("TableNumber", "This table is already reserved for the selected date and time.");
                            return View("Modify", modifiedReservation);
                        }
                    }

                    // Update the reservation
                    _context.Update(originalReservation);
                    await _context.SaveChangesAsync();

                    TempData["StatusMessage"] = "Your reservation has been updated successfully.";
                    return RedirectToAction(nameof(Confirmation), new { id = originalReservation.Id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TableReservationExists(modifiedReservation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View("Modify", modifiedReservation);
        }

        private bool TableReservationExists(int id)
        {
            return _context.TableReservations.Any(e => e.Id == id);
        }
    }
}