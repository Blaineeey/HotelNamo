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

    public IActionResult Index()
    {
        // Get counts for dashboard
        ViewBag.TotalRooms = _context.Rooms.Count();
        ViewBag.TotalBookings = _context.Bookings.Count();
        ViewBag.ActiveBookings = _context.Bookings.Count(b => b.CheckOutDate >= DateTime.Today);
        ViewBag.PendingMaintenanceRequests = _context.MaintenanceRequests.Count(m => m.Status == "Pending");
        ViewBag.PendingHousekeepingTasks = _context.HousekeepingTasks.Count(h => h.Status == "Pending");
        ViewBag.TotalSpaBookings = _context.SpaBookings.Count();
        ViewBag.TotalDiningReservations = _context.TableReservations.Count();
        ViewBag.NewFeedbackCount = _context.Feedbacks.Count(f => f.DateSubmitted.Date == DateTime.Today.Date);

        // Get latest bookings for dashboard
        ViewBag.LatestBookings = _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .OrderByDescending(b => b.BookingDate)
            .Take(5)
            .ToList();

        return View();
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

    [HttpGet]
    public IActionResult CreateStaff()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the user
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
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Validate the typed role
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            bool roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
            if (!roleExists)
            {
                ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' does not exist.");
                return View(model);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }
        }
        else
        {
            ModelState.AddModelError("SelectedRole", "Please enter a role.");
            return View(model);
        }

        return RedirectToAction("ListUsers");
    }

    // ROOM MANAGEMENT
    public IActionResult RoomList()
    {
        var rooms = _context.Rooms.ToList();
        return View(rooms);
    }

    [HttpGet]
    public IActionResult CreateRoom()
    {
        ViewBag.Amenities = _context.Amenities.ToList();
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
    public async Task<IActionResult> CreateRoom(Room room, int[] selectedAmenities, string selectedImage)
    {
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

        room.RoomAmenities = selectedAmenities.Select(a => new RoomAmenity { AmenityId = a }).ToList();
        room.RoomImages = new List<RoomImage>
        {
            new RoomImage { ImagePath = selectedImage }
        };

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return RedirectToAction("RoomList");
    }

    // BOOKING MANAGEMENT
    public async Task<IActionResult> AllBookings(DateTime? fromDate, DateTime? toDate, string status, string searchQuery)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        ViewBag.SearchQuery = searchQuery;

        var query = _context.Bookings
            .Include(b => b.Room)
            .Include(b => b.User)
            .AsQueryable();

        // Apply date filters
        if (fromDate.HasValue)
            query = query.Where(b => b.CheckInDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(b => b.CheckOutDate <= toDate.Value);

        // Apply status filter
        switch (status)
        {
            case "pending":
                query = query.Where(b => !b.IsConfirmed);
                break;
            case "confirmed":
                query = query.Where(b => b.IsConfirmed && b.ActualCheckInTime == null);
                break;
            case "checkedIn":
                query = query.Where(b => b.IsConfirmed && b.ActualCheckInTime != null && b.ActualCheckOutTime == null);
                break;
            case "checkedOut":
                query = query.Where(b => b.IsConfirmed && b.ActualCheckOutTime != null);
                break;
        }

        // Apply search query
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(b =>
                (b.User != null && (
                    b.User.FirstName.Contains(searchQuery) ||
                    b.User.LastName.Contains(searchQuery) ||
                    b.User.Email.Contains(searchQuery)
                )) ||
                (b.GuestName != null && b.GuestName.Contains(searchQuery)) ||
                (b.GuestEmail != null && b.GuestEmail.Contains(searchQuery))
            );
        }

        // Order by check-in date
        query = query.OrderByDescending(b => b.CheckInDate);

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> ConfirmBooking(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            return NotFound();

        booking.IsConfirmed = true;
        await _context.SaveChangesAsync();

        // Optional: Send confirmation email
        if (!string.IsNullOrEmpty(booking.GuestEmail))
        {
            await _emailSender.SendEmailAsync(
                booking.GuestEmail,
                "Your Hotel Booking is Confirmed",
                $"Dear {booking.GuestName},<br><br>Your booking (ID: {booking.Id}) has been confirmed. We look forward to welcoming you on {booking.CheckInDate:MMM dd, yyyy}.<br><br>Best regards,<br>HotelNamo Team"
            );
        }

        return RedirectToAction(nameof(AllBookings));
    }

    public async Task<IActionResult> AdminCheckIn(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            return NotFound();

        booking.ActualCheckInTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(AllBookings));
    }

    public async Task<IActionResult> AdminCheckOut(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            return NotFound();

        // Update room status
        var room = _context.Rooms.Find(booking.RoomId);
        if (room != null)
        {
            room.Status = "Vacant";
        }

        booking.ActualCheckOutTime = DateTime.Now;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(AllBookings));
    }

    // SPA BOOKING MANAGEMENT
    public async Task<IActionResult> ManageSpaBookings(DateTime? fromDate, DateTime? toDate, string status, string treatment, string searchQuery)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        ViewBag.Treatment = treatment;
        ViewBag.SearchQuery = searchQuery;

        // Get unique treatments for filter dropdown
        ViewBag.Treatments = await _context.SpaBookings
            .Select(sb => sb.Treatment)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();

        var query = _context.SpaBookings
            .Include(sb => sb.User)
            .AsQueryable();

        // Apply date filters
        if (fromDate.HasValue)
            query = query.Where(sb => sb.PreferredDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(sb => sb.PreferredDate <= toDate.Value);

        // Apply status filter
        if (!string.IsNullOrEmpty(status))
            query = query.Where(sb => sb.Status == status);

        // Apply treatment filter
        if (!string.IsNullOrEmpty(treatment))
            query = query.Where(sb => sb.Treatment == treatment);

        // Apply search query
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(sb =>
                sb.FullName.Contains(searchQuery) ||
                sb.Email.Contains(searchQuery) ||
                sb.Phone.Contains(searchQuery)
            );
        }

        // Order by date
        query = query.OrderByDescending(sb => sb.PreferredDate);

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> UpdateSpaBookingStatus(int bookingId, string status)
    {
        var booking = await _context.SpaBookings.FindAsync(bookingId);
        if (booking == null)
            return NotFound();

        booking.Status = status;
        await _context.SaveChangesAsync();

        // Optional: Send notification email
        if (status == "Confirmed" || status == "Cancelled")
        {
            await _emailSender.SendEmailAsync(
                booking.Email,
                $"Your Spa Booking is {status}",
                $"Dear {booking.FullName},<br><br>Your spa booking for {booking.PreferredDate:MMM dd, yyyy} at {booking.PreferredTime} has been {status.ToLower()}.<br><br>Best regards,<br>HotelNamo Spa Team"
            );
        }

        return RedirectToAction(nameof(ManageSpaBookings));
    }

    // DINING RESERVATION MANAGEMENT
    public async Task<IActionResult> ManageDiningReservations(DateTime? fromDate, DateTime? toDate, string status, string venue, string searchQuery)
    {
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Status = status;
        ViewBag.Venue = venue;
        ViewBag.SearchQuery = searchQuery;

        // Get unique venues for filter dropdown
        ViewBag.Venues = await _context.TableReservations
            .Select(tr => tr.Venue)
            .Distinct()
            .OrderBy(v => v)
            .ToListAsync();

        var query = _context.TableReservations
            .Include(tr => tr.User)
            .AsQueryable();

        // Apply date filters
        if (fromDate.HasValue)
            query = query.Where(tr => tr.ReservationDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(tr => tr.ReservationDate <= toDate.Value);

        // Apply status filter
        if (!string.IsNullOrEmpty(status))
            query = query.Where(tr => tr.Status == status);

        // Apply venue filter
        if (!string.IsNullOrEmpty(venue))
            query = query.Where(tr => tr.Venue == venue);

        // Apply search query
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(tr =>
                tr.FullName.Contains(searchQuery) ||
                tr.Email.Contains(searchQuery) ||
                tr.Phone.Contains(searchQuery)
            );
        }

        // Order by date
        query = query.OrderByDescending(tr => tr.ReservationDate);

        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> UpdateDiningReservationStatus(int reservationId, string status)
    {
        var reservation = await _context.TableReservations.FindAsync(reservationId);
        if (reservation == null)
            return NotFound();

        reservation.Status = status;
        await _context.SaveChangesAsync();

        // Optional: Send notification email
        if (status == "Confirmed" || status == "Cancelled")
        {
            await _emailSender.SendEmailAsync(
                reservation.Email,
                $"Your Dining Reservation is {status}",
                $"Dear {reservation.FullName},<br><br>Your reservation at {reservation.Venue} for {reservation.ReservationDate:MMM dd, yyyy} at {reservation.ReservationTime} has been {status.ToLower()}.<br><br>Best regards,<br>HotelNamo Dining Team"
            );
        }

        return RedirectToAction(nameof(ManageDiningReservations));
    }

    // FEEDBACK MANAGEMENT
    public async Task<IActionResult> ManageFeedback(int? minRating, int? maxRating, string roomCategory, DateTime? fromDate, DateTime? toDate)
    {
        ViewBag.MinRating = minRating;
        ViewBag.MaxRating = maxRating;
        ViewBag.RoomCategory = roomCategory;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        // Get unique room categories for filter dropdown
        ViewBag.RoomCategories = await _context.Rooms
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var query = _context.Feedbacks
            .Include(f => f.User)
            .Include(f => f.Room)
            .Include(f => f.Booking)
            .AsQueryable();

        // Apply rating filters
        if (minRating.HasValue)
            query = query.Where(f => f.Rating >= minRating.Value);
        if (maxRating.HasValue)
            query = query.Where(f => f.Rating <= maxRating.Value);

        // Apply room category filter
        if (!string.IsNullOrEmpty(roomCategory))
            query = query.Where(f => f.Room.Category == roomCategory);

        // Apply date filters
        if (fromDate.HasValue)
            query = query.Where(f => f.DateSubmitted >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(f => f.DateSubmitted <= toDate.Value);

        // Order by date (newest first)
        query = query.OrderByDescending(f => f.DateSubmitted);

        return View(await query.ToListAsync());
    }

    // ADMIN PROFILE
    [HttpGet]
    public async Task<IActionResult> AdminProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new AdminProfileViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AdminProfile(AdminProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;

        // Email change requires additional verification and may affect the user identity
        // So this is not implemented in this basic example

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        ViewBag.StatusMessage = "Your profile has been updated";
        return View(model);
    }
    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SelectedRole = userRoles.FirstOrDefault() // Gets the first role
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found");
            return View(model);
        }

        // Update user details
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Email = model.Email;
        user.UserName = model.Email; // Update username to match email

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Handle role update
        var currentRoles = await _userManager.GetRolesAsync(user);

        // Remove current roles
        if (currentRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        // Add new role if specified
        if (!string.IsNullOrEmpty(model.SelectedRole))
        {
            bool roleExists = await _roleManager.RoleExistsAsync(model.SelectedRole);
            if (!roleExists)
            {
                ModelState.AddModelError("SelectedRole", $"Role '{model.SelectedRole}' does not exist.");
                return View(model);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }
        }

        return RedirectToAction("ListUsers");
    }

    public async Task<IActionResult> DeleteUser(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        // Check if the user is the current admin (prevent self-deletion)
        if (User.Identity.Name == user.UserName)
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction("ListUsers");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the user.";
        }
        else
        {
            TempData["SuccessMessage"] = "User deleted successfully.";
        }

        return RedirectToAction("ListUsers");
    }

    // Add these methods to the AdminController.cs file

    [HttpGet]
    public async Task<IActionResult> EditRoom(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.RoomAmenities)
            .Include(r => r.RoomImages)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        // Get all available amenities for the form
        ViewBag.Amenities = await _context.Amenities.ToListAsync();

        // Get already selected amenity IDs for pre-selecting checkboxes
        ViewBag.SelectedAmenities = room.RoomAmenities.Select(ra => ra.AmenityId).ToList();

        // Get existing room image for pre-selection
        ViewBag.CurrentImage = room.RoomImages.FirstOrDefault()?.ImagePath;

        // Provide list of available images (same as in CreateRoom)
        ViewBag.ExistingImages = new List<string>
    {
        "single-room.jpg",
        "guest-room.jpg",
        "deluxe-room.jpg",
        "superior-room.jpg"
    };

        return View(room);
    }

    [HttpPost]
    public async Task<IActionResult> EditRoom(Room room, int[] selectedAmenities, string selectedImage)
    {
        ModelState.Remove("RoomImages");

        if (!ModelState.IsValid)
        {
            ViewBag.Amenities = await _context.Amenities.ToListAsync();
            ViewBag.SelectedAmenities = selectedAmenities;
            ViewBag.CurrentImage = selectedImage;
            ViewBag.ExistingImages = new List<string>
        {
            "single-room.jpg", "guest-room.jpg", "superior-room.jpg", "deluxe-room.jpg"
        };
            return View(room);
        }

        // Get the existing room to update
        var existingRoom = await _context.Rooms
            .Include(r => r.RoomAmenities)
            .Include(r => r.RoomImages)
            .FirstOrDefaultAsync(r => r.Id == room.Id);

        if (existingRoom == null)
        {
            return NotFound();
        }

        try
        {
            // Update basic room properties
            existingRoom.RoomNumber = room.RoomNumber;
            existingRoom.FloorNumber = room.FloorNumber;
            existingRoom.Category = room.Category;
            existingRoom.Price = room.Price;
            existingRoom.Status = room.Status;
            existingRoom.Description = room.Description;

            // Update room amenities (remove existing and add new ones)
            _context.RoomAmenities.RemoveRange(existingRoom.RoomAmenities);
            existingRoom.RoomAmenities = selectedAmenities.Select(a => new RoomAmenity
            {
                RoomId = existingRoom.Id,
                AmenityId = a
            }).ToList();

            // Update room image if a new one is selected
            if (!string.IsNullOrEmpty(selectedImage))
            {
                // Remove existing images if any
                if (existingRoom.RoomImages.Any())
                {
                    _context.RoomImages.RemoveRange(existingRoom.RoomImages);
                }

                // Add the new image
                existingRoom.RoomImages = new List<RoomImage>
            {
                new RoomImage { ImagePath = selectedImage }
            };
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Room #{existingRoom.RoomNumber} has been updated successfully.";
            return RedirectToAction("RoomList");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while updating the room: {ex.Message}");
            ViewBag.Amenities = await _context.Amenities.ToListAsync();
            ViewBag.SelectedAmenities = selectedAmenities;
            ViewBag.CurrentImage = selectedImage;
            ViewBag.ExistingImages = new List<string>
        {
            "single-room.jpg", "guest-room.jpg", "superior-room.jpg", "deluxe-room.jpg"
        };
            return View(room);
        }
    }

    [HttpGet]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms
            .Include(r => r.RoomAmenities)
            .Include(r => r.RoomImages)
            .Include(r => r.Bookings)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        // Check if room has active bookings
        bool hasActiveBookings = room.Bookings.Any(b => b.CheckOutDate >= DateTime.Today);
        if (hasActiveBookings)
        {
            // If there are active bookings, don't allow deletion
            TempData["ErrorMessage"] = $"Cannot delete Room #{room.RoomNumber} as it has active bookings.";
            return RedirectToAction("RoomList");
        }

        try
        {
            // Remove room amenities
            _context.RoomAmenities.RemoveRange(room.RoomAmenities);

            // Remove room images
            _context.RoomImages.RemoveRange(room.RoomImages);

            // Remove the room
            _context.Rooms.Remove(room);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Room #{room.RoomNumber} has been deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"An error occurred while deleting the room: {ex.Message}";
        }

        return RedirectToAction("RoomList");
    }
}