using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.IO;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
        IEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboardData = new AdminDashboardViewModel
            {
                // User Statistics
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalStaff = (await _userManager.GetUsersInRoleAsync("Staff")).Count,
                TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count,

                // Room Statistics
                TotalRooms = await _context.Rooms.CountAsync(),
                AvailableRooms = await _context.Rooms.CountAsync(r => r.Status == "Available"),
                OccupiedRooms = await _context.Rooms.CountAsync(r => r.Status == "Occupied"),
                MaintenanceRooms = await _context.Rooms.CountAsync(r => r.Status == "Maintenance"),

                // Booking Statistics
                TotalBookings = await _context.Bookings.CountAsync(),
                ActiveBookings = await _context.Bookings.CountAsync(b => b.IsConfirmed && b.CheckOutDate > DateTime.Now),
                CompletedBookings = await _context.Bookings.CountAsync(b => b.IsConfirmed && b.CheckOutDate <= DateTime.Now),
                CancelledBookings = await _context.Bookings.CountAsync(b => !b.IsConfirmed),

                // Financial Statistics
                TotalRevenue = await _context.Payments.SumAsync(p => p.Amount),
                MonthlyRevenue = await _context.Payments
                    .Where(p => p.PaymentDate >= DateTime.Now.AddMonths(-1))
                    .SumAsync(p => p.Amount),

                // Recent Activities
                RecentBookings = await _context.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .OrderByDescending(b => b.BookingDate)
                    .Take(5)
                    .ToListAsync(),

                RecentPayments = await _context.Payments
                    .Include(p => p.Booking)
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync(),

                PendingMaintenance = await _context.MaintenanceRequests
                    .Include(m => m.Room)
                    .Where(m => m.Status == "Pending")
                    .Take(5)
                    .ToListAsync(),

                PendingHousekeeping = await _context.HousekeepingTasks
                    .Include(h => h.Room)
                    .Where(h => h.Status == "Pending")
                    .Take(5)
                    .ToListAsync(),

                // Chart Data
                MonthlyBookings = await GetMonthlyBookings(),
                RoomTypeDistribution = await GetRoomTypeDistribution(),
                RevenueByMonth = await GetRevenueByMonth()
            };

            return View(dashboardData);
        }
        catch
        {
            // Log the exception (you can add proper logging here)
            return View("Error", new ErrorViewModel { RequestId = "Dashboard Error" });
        }
    }

    private async Task<List<ChartData>> GetMonthlyBookings()
    {
        var currentYear = DateTime.Now.Year;
        var monthlyData = new List<ChartData>();

        for (int month = 1; month <= 12; month++)
        {
            var count = await _context.Bookings
                .CountAsync(b => b.BookingDate.Year == currentYear && b.BookingDate.Month == month);

            monthlyData.Add(new ChartData
            {
                Label = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Value = count
            });
        }

        return monthlyData;
    }

    private async Task<List<ChartData>> GetRoomTypeDistribution()
    {
        return await _context.Rooms
            .GroupBy(r => r.Category)
            .Select(g => new ChartData
            {
                Label = g.Key,
                Value = g.Count()
            })
            .ToListAsync();
    }

    private async Task<List<ChartData>> GetRevenueByMonth()
    {
        var currentYear = DateTime.Now.Year;
        var monthlyData = new List<ChartData>();

        for (int month = 1; month <= 12; month++)
        {
            var revenue = await _context.Payments
                .Where(p => p.PaymentDate.Year == currentYear && p.PaymentDate.Month == month)
                .SumAsync(p => p.Amount);

            monthlyData.Add(new ChartData
            {
                Label = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month),
                Value = (int)revenue
            });
        }

        return monthlyData;
    }

    public async Task<IActionResult> ListUsers()
    {
        var allUsers = _userManager.Users.ToList();
        var list = new List<UserWithRolesViewModel>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            list.Add(new UserWithRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = roles
            });
        }

        return View(list);
    }


    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult CreateStaff()
    {
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        // No dynamic roles, just a text input for the role
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // 1. Create the user
        var user = new ApplicationUser
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            // Show identity errors
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // 2. Validate the typed role
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            bool roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
            if (!roleExists)
            {
                // If the typed role doesn't exist, show an error
                ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' does not exist.");
                // Optionally delete the newly created user or handle differently
                // await _userManager.DeleteAsync(user);
                ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
                return View(model);
            }
            else
            {
                // 3. Assign the typed role
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }
        }
        else
        {
            // If no role typed, you could default to "User" or show an error
            ModelState.AddModelError("SelectedRole", "Please select a role.");
            // Optionally delete the user or handle differently
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        return RedirectToAction("ListUsers");
    }


    // ---------- ROOM MANAGEMENT -----------
    public IActionResult RoomList()
    {
        var rooms = _context.Rooms
            .Include(r => r.RoomImages)
            .ToList();
        return View(rooms);
    }

    [HttpGet]
    public IActionResult RoomDetails(int id)
    {
        var room = _context.Rooms
            .Include(r => r.RoomImages)
            .Include(r => r.RoomAmenities).ThenInclude(ra => ra.Amenity)
            .FirstOrDefault(r => r.Id == id);
        if (room == null) return NotFound();
        return View(room);
    }

    [HttpGet]
    public IActionResult EditRoom(int id)
    {
        var room = _context.Rooms
            .Include(r => r.RoomImages)
            .FirstOrDefault(r => r.Id == id);
        if (room == null) return NotFound();
        return View(room);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoom(Room model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == model.Id);
        if (room == null) return NotFound();
        room.RoomNumber = model.RoomNumber;
        room.Category = model.Category;
        room.Price = model.Price;
        room.Status = model.Status;
        room.Description = model.Description;
        await _context.SaveChangesAsync();
        return RedirectToAction("RoomList");
    }

    [HttpGet]
    public IActionResult CreateRoom()
    {
        ViewBag.Amenities = _context.Amenities.ToList();

        // Explicitly add existing images to ViewBag
        ViewBag.ExistingImages = new List<string>
    {
        "single-room.jpg",
        "guest-room.jpg",
        "deluxe-room.jpg",
        "superior-room.jpg"
    };

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(Room room, int[] selectedAmenities, string selectedImage, IFormFile uploadImage)
    {
        // Explicitly remove ModelState validation for RoomImages as we're assigning it manually
        ModelState.Remove("RoomImages");

        if (!ModelState.IsValid)
        {
            ViewBag.Amenities = _context.Amenities.ToList();
            ViewBag.ExistingImages = new List<string>
        {
            "single-room.jpg", "guest-room.jpg", "superior-room.jpg", "deluxe-room.jpg"
        };
            return View(room);
        }

        // Use uploaded image if provided
        if (uploadImage != null && uploadImage.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(uploadImage.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rooms", fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadImage.CopyToAsync(stream);
            }
            selectedImage = fileName;
        }

        room.RoomAmenities = selectedAmenities.Select(a => new RoomAmenity { AmenityId = a }).ToList();

        // Explicitly assign existing image clearly
        room.RoomImages = new List<RoomImage>
    {
        new RoomImage { ImagePath = selectedImage }
    };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return RedirectToAction("RoomList");
    }

    // ---------- BOOKINGS MANAGEMENT -----------
    [HttpGet]
    public async Task<IActionResult> AllBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .OrderByDescending(b => b.CreatedDate)
            .ToListAsync();

        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return NotFound();
        }

        if (!booking.IsConfirmed)
        {
            booking.IsConfirmed = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AllBookings");
    }

    [HttpGet]
    public async Task<IActionResult> AdminCheckIn(int bookingId)
    {
        var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.IsConfirmed && booking.ActualCheckInTime == null)
        {
            booking.ActualCheckInTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AllBookings");
    }

    [HttpGet]
    public async Task<IActionResult> AdminCheckOut(int bookingId)
    {
        var booking = await _context.Bookings.Include(b => b.Room).FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking == null)
        {
            return NotFound();
        }

        if (booking.ActualCheckInTime != null && booking.ActualCheckOutTime == null)
        {
            booking.ActualCheckOutTime = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AllBookings");
    }

    [HttpGet]
    public async Task<IActionResult> AdminProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var vm = new AdminProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> AdminProfile(AdminProfileViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FirstName = vm.FirstName;
        user.LastName = vm.LastName;
        user.Email = vm.Email;
        user.UserName = vm.Email;

        await _userManager.UpdateAsync(user);
        return RedirectToAction("Index");
    }

    // ---------- USER MANAGEMENT -----------
    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
        var vm = new UserWithRolesViewModel { UserId = user.Id, Email = user.Email, Roles = roles };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(UserWithRolesViewModel vm, string? selectedRole)
    {
        var user = await _userManager.FindByIdAsync(vm.UserId);
        if (user == null) return NotFound();

        // Update email/username
        user.Email = vm.Email;
        user.UserName = vm.Email;
        await _userManager.UpdateAsync(user);

        // Update role: replace current roles with the selected one (if provided and exists)
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }
        if (!string.IsNullOrWhiteSpace(selectedRole) && await _roleManager.RoleExistsAsync(selectedRole))
        {
            await _userManager.AddToRoleAsync(user, selectedRole);
        }

        return RedirectToAction("ListUsers");
    }

    [HttpGet]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        var vm = new UserWithRolesViewModel { UserId = user.Id, Email = user.Email };
        return View(vm);
    }

    [HttpPost, ActionName("DeleteUser")]
    public async Task<IActionResult> DeleteUserConfirmed(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();
        await _userManager.DeleteAsync(user);
        return RedirectToAction("ListUsers");
    }


}
