using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "Admin,Maintenance")] // ✅ Only allow Admin & Maintenance staff
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MaintenanceController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Display all maintenance requests (Admin)
        public async Task<IActionResult> Index()
        {
            var requests = await _context.MaintenanceRequests
                .Include(m => m.Room)
                .Include(m => m.AssignedStaff)
                .ToListAsync();

            return View(requests);
        }

        // ✅ Display form to create a maintenance request (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.AssignedStaff = await GetMaintenanceStaffSelectList();

            return View();
        }

        // ✅ Pass selected Room & Staff to Confirmation Page
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateConfirmation(int RoomId, string AssignedStaffId)
        {
            var room = await _context.Rooms.FindAsync(RoomId);
            var staff = await _userManager.FindByIdAsync(AssignedStaffId);

            if (room == null || staff == null)
            {
                TempData["Error"] = "Invalid selection. Please try again.";
                return RedirectToAction("Create");
            }

            var model = new MaintenanceRequest
            {
                RoomId = RoomId,
                AssignedStaffId = AssignedStaffId,
                Room = room,
                IssueDescription = "" // Ensure issue description is initialized
            };

            ViewBag.AssignedStaffName = $"{staff.FirstName} {staff.LastName}";
            return View("Confirm", model);
        }


        // ✅ Final Submission After Confirmation
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Confirm(MaintenanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View("Confirm", request);
            }

            var room = await _context.Rooms.FindAsync(request.RoomId);
            if (room == null)
            {
                TempData["Error"] = "Invalid room selection.";
                return RedirectToAction("Create");
            }

            request.Status = "Pending";
            _context.MaintenanceRequests.Add(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // ✅ Edit Maintenance Request (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            ViewBag.Rooms = await GetRoomsSelectList();
            ViewBag.AssignedStaff = await GetMaintenanceStaffSelectList();

            return View(request);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Edit(MaintenanceRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Rooms = await GetRoomsSelectList();
                ViewBag.AssignedStaff = await GetMaintenanceStaffSelectList();
                return View(request);
            }

            _context.MaintenanceRequests.Update(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ✅ Mark Maintenance Request as Completed (Only Maintenance Staff)
        [Authorize(Roles = "Maintenance")]
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Completed";
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        // ✅ Delete a maintenance request (Admin Only)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var request = await _context.MaintenanceRequests.FindAsync(id);
            if (request == null) return NotFound();

            _context.MaintenanceRequests.Remove(request);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ✅ Maintenance Staff Dashboard - Show only assigned tasks
        [Authorize(Roles = "Maintenance")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var tasks = await _context.MaintenanceRequests
                .Where(m => m.AssignedStaffId == user.Id && m.Status != "Completed")
                .Include(m => m.Room)
                .ToListAsync();

            return View(tasks);
        }

        // ✅ Utility Method: Get Select List for Rooms
        private async Task<List<SelectListItem>> GetRoomsSelectList()
        {
            return await _context.Rooms
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomNumber} - {r.Category}"
                })
                .ToListAsync();
        }

        // ✅ Utility Method: Get Select List for Maintenance Staff
        private async Task<List<SelectListItem>> GetMaintenanceStaffSelectList()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var maintenanceStaff = new List<SelectListItem>();

            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Maintenance"))
                {
                    maintenanceStaff.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FirstName} {user.LastName}"
                    });
                }
            }

            return maintenanceStaff;
        }
    }
}
