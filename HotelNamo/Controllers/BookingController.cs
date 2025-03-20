using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "User,FrontDesk")]
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

            var model = new BookingViewModel
            {
                RoomId = roomId ?? 0,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1)
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Create(BookingViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
                return View(vm);
            }

            if (!CheckRoomAvailability(vm.RoomId, vm.CheckInDate, vm.CheckOutDate))
            {
                ModelState.AddModelError("", "Room is not available for the selected dates.");
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
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


        public async Task<IActionResult> Confirm()
        {
            if (!TempData.ContainsKey("BookingId"))
                return RedirectToAction("Create");

            var bookingId = Convert.ToInt32(TempData["BookingId"]);

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsConfirmed);

            if (booking == null)
                return RedirectToAction("MyBookings");

            TempData.Keep();

            return View(booking);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking()
        {
            if (!TempData.ContainsKey("BookingId"))
                return RedirectToAction("Create");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var bookingId = Convert.ToInt32(TempData["BookingId"]);

            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == user.Id && !b.IsConfirmed);

            if (booking != null)
            {
                booking.IsConfirmed = true;

                // ✅ Update Room Status to "Occupied"
                var room = await _context.Rooms.FindAsync(booking.RoomId);
                if (room != null)
                {
                    room.Status = "Occupied";
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyBookings");
        }



        // Allows a user to cancel their own booking explicitly
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == _userManager.GetUserId(User));

            if (booking == null)
                return NotFound();

            // Remove booking explicitly
            _context.Bookings.Remove(booking);

            // Set room status back to vacant if booking was confirmed explicitly
            if (booking.Room != null)
                booking.Room.Status = "Vacant";

            await _context.SaveChangesAsync();

            return RedirectToAction("MyBookings");
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
