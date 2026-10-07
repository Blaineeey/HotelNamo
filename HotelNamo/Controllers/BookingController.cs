using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelNamo.Controllers
{
    // Remove Authorize attribute from the class to allow guest access
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create(int? roomId)
        {
            // ✅ Fetch only rooms that are not already booked or occupied
            var bookedRoomIds = _context.Bookings
                .Where(b => b.IsConfirmed || (b.CheckInDate <= DateTime.Today && b.CheckOutDate >= DateTime.Today))
                .Select(b => b.RoomId)
                .ToList();

            ViewBag.Rooms = _context.Rooms
                .Where(r => r.Status == "Vacant" && !bookedRoomIds.Contains(r.Id))
                .ToList();

            // Check if user is authenticated
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;

            // Choose proper model based on authentication status
            if (User.Identity.IsAuthenticated)
            {
                var model = new BookingViewModel
                {
                    RoomId = roomId ?? 0,
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(1)
                };

                return View(model);
            }
            else
            {
                var model = new GuestBookingViewModel
                {
                    RoomId = roomId ?? 0,
                    CheckInDate = DateTime.Today,
                    CheckOutDate = DateTime.Today.AddDays(1)
                };

                return View("CreateGuest", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
                return View(vm);
            }

            if (!CheckRoomAvailability(vm.RoomId, vm.CheckInDate, vm.CheckOutDate))
            {
                ModelState.AddModelError("", "Room is not available for the selected dates.");
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
                return View(vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var room = _context.Rooms.Find(vm.RoomId);
            int totalDays = (vm.CheckOutDate - vm.CheckInDate).Days;
            decimal totalPrice = totalDays * room.Price;

            // ✅ Check if a pending booking already exists
            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.RoomId == vm.RoomId && b.UserId == user.Id &&
                                          b.CheckInDate == vm.CheckInDate && b.CheckOutDate == vm.CheckOutDate &&
                                          !b.IsConfirmed);

            if (existingBooking == null)
            {
                var booking = new Booking
                {
                    RoomId = vm.RoomId,
                    UserId = user.Id,
                    CheckInDate = vm.CheckInDate,
                    CheckOutDate = vm.CheckOutDate,
                    SpecialRequests = vm.SpecialRequests,
                    TotalPrice = totalPrice,
                    CreatedDate = DateTime.Now,
                    IsConfirmed = false
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                existingBooking = booking;
            }

            // ✅ Redirect to Payment Page with the correct booking ID
            return RedirectToAction("Pay", "Payment", new { bookingId = existingBooking.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGuest(GuestBookingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
                return View(vm);
            }

            if (!CheckRoomAvailability(vm.RoomId, vm.CheckInDate, vm.CheckOutDate))
            {
                ModelState.AddModelError("", "Room is not available for the selected dates.");
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
                return View(vm);
            }

            var room = _context.Rooms.Find(vm.RoomId);
            int totalDays = (vm.CheckOutDate - vm.CheckInDate).Days;
            decimal totalPrice = totalDays * room.Price;

            // Create a guest booking
            var guestBooking = new Booking
            {
                RoomId = vm.RoomId,
                GuestName = vm.GuestName,
                GuestEmail = vm.GuestEmail,
                GuestPhone = vm.GuestPhone,
                CheckInDate = vm.CheckInDate,
                CheckOutDate = vm.CheckOutDate,
                SpecialRequests = vm.SpecialRequests,
                TotalPrice = totalPrice,
                CreatedDate = DateTime.Now,
                IsConfirmed = false
            };

            _context.Bookings.Add(guestBooking);
            await _context.SaveChangesAsync();

            // Store the booking ID in TempData for use in the payment process
            TempData["GuestBookingId"] = guestBooking.Id;
            TempData["GuestName"] = guestBooking.GuestName;
            TempData["GuestEmail"] = guestBooking.GuestEmail;

            // Redirect to Guest Payment Page
            return RedirectToAction("PayGuest", "Payment", new { bookingId = guestBooking.Id });
        }

        // Confirm view accessible to all
        public async Task<IActionResult> Confirm()
        {
            int? bookingId = null;

            // Try to get booking ID from TempData (authenticated user flow)
            if (TempData.ContainsKey("BookingId"))
            {
                bookingId = Convert.ToInt32(TempData["BookingId"]);
            }
            // Try to get booking ID from TempData (guest flow)
            else if (TempData.ContainsKey("GuestBookingId"))
            {
                bookingId = Convert.ToInt32(TempData["GuestBookingId"]);
            }

            if (!bookingId.HasValue)
                return RedirectToAction("Create");

            // Find the booking
            var confirmBooking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsConfirmed);

            if (confirmBooking == null)
                return RedirectToAction("Index", "Home");

            // Keep TempData for the confirmation step
            TempData.Keep();

            // Use different view based on booking type (check after getting from database)
            if (string.IsNullOrEmpty(confirmBooking.UserId))
            {
                return View("ConfirmGuest", confirmBooking);
            }
            return View(confirmBooking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,FrontDesk")]
        public async Task<IActionResult> ConfirmBooking()
        {
            if (!TempData.ContainsKey("BookingId"))
                return RedirectToAction("Create");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var bookingId = Convert.ToInt32(TempData["BookingId"]);

            var confirmableBooking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id && !b.IsConfirmed);

            if (confirmableBooking != null)
            {
                confirmableBooking.IsConfirmed = true;

                // ✅ Update Room Status to "Occupied"
                var room = await _context.Rooms.FindAsync(confirmableBooking.RoomId);
                if (room != null)
                {
                    room.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmGuestBooking()
        {
            if (!TempData.ContainsKey("GuestBookingId"))
                return RedirectToAction("Create");

            var bookingId = Convert.ToInt32(TempData["GuestBookingId"]);

            // Use UserId == null to identify guest bookings instead of IsGuestBooking property
            var guestBooking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == null && !b.IsConfirmed);

            if (guestBooking != null)
            {
                guestBooking.IsConfirmed = true;

                // Update Room Status
                var room = await _context.Rooms.FindAsync(guestBooking.RoomId);
                if (room != null)
                {
                    room.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
            }

            // Redirect to home page for guest users
            return RedirectToAction("Index", "Home");
        }

        // Allow both guests and authenticated users to cancel
        public async Task<IActionResult> Cancel(int bookingId, string? email = null)
        {
            Booking? cancelBooking = null;
            
            // For authenticated users
            if (User.Identity.IsAuthenticated)
            {
                cancelBooking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == _userManager.GetUserId(User));
            }
            // For guest users (require email verification)
            else if (!string.IsNullOrEmpty(email))
            {
                cancelBooking = await _context.Bookings
                    .Include(b => b.Room)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && b.GuestEmail == email);
            }

            if (cancelBooking == null)
                return NotFound();

            // Remove booking explicitly
            _context.Bookings.Remove(cancelBooking);

            // Set room status back to vacant if booking was confirmed explicitly
            if (cancelBooking.Room != null)
                cancelBooking.Room.Status = "Vacant";

            await _context.SaveChangesAsync();

            // Redirect based on user type
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("MyBookings");
            }
            return RedirectToAction("Index", "Home");
        }

        private bool CheckRoomAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            return !_context.Bookings.Any(b =>
                b.RoomId == roomId && b.IsConfirmed &&
                (checkIn < b.CheckOutDate && checkOut > b.CheckInDate));
        }

        [Authorize(Roles = "User")]
        public IActionResult MyBookings()
        {
            var userId = _userManager.GetUserId(User);

            var bookings = _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.RoomImages)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToList();

            return View(bookings);
        }
    }
}