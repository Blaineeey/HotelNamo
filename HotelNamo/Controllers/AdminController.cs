using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For EF Core
using System.Linq;
using System.Threading.Tasks;
using HotelNamo.Data; // Your data namespace
using HotelNamo.Models; // Your models namespace
using Microsoft.AspNetCore.Identity.UI.Services;


[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context; // <-- Add this
    private readonly IEmailSender _emailSender;
    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context,
         IEmailSender emailSender)// <-- Inject the DbContext
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _emailSender = emailSender;// <-- Assign it
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
        // No dynamic roles, just a text input for the role
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid)
        {
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
            ModelState.AddModelError("SelectedRole", "Please enter a role.");
            // Optionally delete the user or handle differently
            return View(model);
        }

        return RedirectToAction("ListUsers");
    }


    // ---------- ROOM MANAGEMENT -----------
    public IActionResult RoomList()
    {
        var rooms = _context.Rooms.ToList();
        return View(rooms);
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
    public async Task<IActionResult> CreateRoom(Room room, int[] selectedAmenities, string selectedImage)
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



}
