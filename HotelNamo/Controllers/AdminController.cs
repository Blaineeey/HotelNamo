using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For EF Core
using System.Linq;
using System.Threading.Tasks;
using HotelNamo.Data; // Your data namespace
using HotelNamo.Models; // Your models namespace

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

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context) // <-- Inject the DbContext
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context; // <-- Assign it
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
        Console.WriteLine("DEBUG: CreateStaff GET method called!");
        var model = new CreateStaffViewModel
        {
            AvailableRoles = new List<string> { "FrontDesk", "Housekeeping" }
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new ApplicationUser
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            UserName = model.Email
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }
            return RedirectToAction("Index");
        }

        // If we got here, something failed
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
        return View(model);
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
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom(Room model)
    {
        if (!ModelState.IsValid) return View(model);

        _context.Rooms.Add(model);
        await _context.SaveChangesAsync();
        return RedirectToAction("RoomList");
    }

}
