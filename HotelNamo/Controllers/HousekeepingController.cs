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
using System.Collections.Generic;

namespace HotelNamo.Controllers
{
    [Authorize(Roles = "Admin,HouseKeeping")]
    public class HousekeepingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HousekeepingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        // ✅ Housekeeping Dashboard - Only Show Tasks Assigned to the Logged-in Housekeeping Staff
        [Authorize(Roles = "HouseKeeping")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            var tasks = await _context.HousekeepingTasks
                .Include(ht => ht.Room)
                .Where(ht => ht.AssignedStaffId == userId)
                .ToListAsync();

            return View("Dashboard", tasks);
        }

        // ✅ View All Tasks (Admin & Housekeeping Staff)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var isHousekeeping = await _userManager.IsInRoleAsync(user, "HouseKeeping");

            var tasks = await _context.HousekeepingTasks
                .Include(ht => ht.Room)
                .Include(ht => ht.AssignedStaff)
                .Where(ht => isHousekeeping ? ht.AssignedStaffId == user.Id : true)
                .ToListAsync();

            return View(tasks);
        }

        // ✅ Step 1: Load Task Creation Form
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Rooms = _context.Rooms
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomNumber} - {r.Category}"
                }).ToList();

            var housekeepingUsers = await _userManager.Users.ToListAsync();
            var housekeepingStaff = new List<SelectListItem>();

            foreach (var user in housekeepingUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "HouseKeeping"))
                {
                    housekeepingStaff.Add(new SelectListItem
                    {
                        Value = user.Id,
                        Text = $"{user.FirstName} {user.LastName}"
                    });
                }
            }

            ViewBag.Staff = housekeepingStaff;
            return View();
        }

        // ✅ Step 2: Capture Room & Staff Selection
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult SelectDetails(int RoomId, string AssignedStaffId)
        {
            if (RoomId == 0 || string.IsNullOrEmpty(AssignedStaffId))
            {
                TempData["ErrorMessage"] = "Please select a room and an assigned staff member.";
                return RedirectToAction("Create");
            }

            TempData["RoomId"] = RoomId;
            TempData["AssignedStaffId"] = AssignedStaffId;

            var selectedRoom = _context.Rooms.Find(RoomId);
            var selectedStaff = _context.Users.Find(AssignedStaffId);

            ViewBag.SelectedRoom = selectedRoom != null ? $"{selectedRoom.RoomNumber} - {selectedRoom.Category}" : "Not Found";
            ViewBag.SelectedStaff = selectedStaff != null ? $"{selectedStaff.FirstName} {selectedStaff.LastName}" : "Not Found";

            return View("Confirm", new HousekeepingTask());
        }

        // ✅ Step 3: Final Confirmation and Task Creation
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateConfirmed(HousekeepingTask task)
        {
            if (TempData["RoomId"] == null || TempData["AssignedStaffId"] == null)
            {
                TempData["ErrorMessage"] = "Session expired. Please restart the task creation process.";
                return RedirectToAction("Create");
            }

            task.RoomId = (int)TempData["RoomId"];
            task.AssignedStaffId = TempData["AssignedStaffId"].ToString();
            task.Status = "Pending"; // Default status
 

            _context.HousekeepingTasks.Add(task);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Task successfully created!";
            return RedirectToAction("Index");
        }

        // ✅ Housekeeping Staff Can Mark Task As Completed
        [Authorize(Roles = "HouseKeeping")]
        public async Task<IActionResult> CompleteTask(int id)
        {
            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task == null) return NotFound();

            task.Status = "Completed";
            task.CompletedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard");
        }

        // ✅ Admin can delete tasks
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.HousekeepingTasks.FindAsync(id);
            if (task == null) return NotFound();

            _context.HousekeepingTasks.Remove(task);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
