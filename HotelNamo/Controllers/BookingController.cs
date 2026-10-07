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
            ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "Vacant").ToList();
            var model = new BookingViewModel
            {
                RoomId = roomId ?? 0,
                CheckInDate = DateTime.Today,
                CheckOutDate = DateTime.Today.AddDays(1)
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Create(BookingViewModel vm)
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

            var room = _context.Rooms.Find(vm.RoomId);
            int totalDays = (vm.CheckOutDate - vm.CheckInDate).Days;
            decimal totalPrice = totalDays * room.Price;

            TempData["RoomId"] = vm.RoomId;
            TempData["CheckInDate"] = vm.CheckInDate.ToString("yyyy-MM-dd");
            TempData["CheckOutDate"] = vm.CheckOutDate.ToString("yyyy-MM-dd");
            TempData["SpecialRequests"] = vm.SpecialRequests;
            TempData["TotalPrice"] = totalPrice.ToString(); // Store explicitly as string

            return RedirectToAction("Confirm");
        }

        public IActionResult Confirm()
        {
            if (TempData["RoomId"] == null)
                return RedirectToAction("Create");

            var roomId = Convert.ToInt32(TempData["RoomId"]);
            var checkIn = DateTime.Parse(TempData["CheckInDate"].ToString()!);
            var checkOut = DateTime.Parse(TempData["CheckOutDate"].ToString()!);
            var specialRequests = TempData["SpecialRequests"]?.ToString();
            decimal totalPrice = decimal.Parse(TempData["TotalPrice"].ToString()!);

            var booking = new Booking
            {
                RoomId = roomId,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                SpecialRequests = specialRequests,
                TotalPrice = totalPrice,
                IsConfirmed = false, // Ensure it remains pending
                Room = _context.Rooms.Find(roomId)!
            };

            TempData.Keep();

            return View(booking);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmBooking()
        {
            if (!TempData.ContainsKey("RoomId"))
                return RedirectToAction("Create");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var roomId = Convert.ToInt32(TempData["RoomId"]);
            var checkIn = DateTime.Parse(TempData["CheckInDate"].ToString()!);
            var checkOut = DateTime.Parse(TempData["CheckOutDate"].ToString()!);
            var specialRequests = TempData["SpecialRequests"]?.ToString();
            decimal totalPrice = decimal.Parse(TempData["TotalPrice"].ToString()!);

            var booking = new Booking
            {
                RoomId = roomId,
                UserId = user.Id,
                CheckInDate = checkIn,
                CheckOutDate = checkOut,
                SpecialRequests = specialRequests,
                TotalPrice = totalPrice,
                CreatedDate = DateTime.Now,
                IsConfirmed = false // Booking remains pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // **Update room status to "Reserved" after booking**
            var room = await _context.Rooms.FindAsync(roomId);
            if (room != null)
            {
                room.Status = "Reserved";
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
