using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Housekeeping,Admin")]
public class HousekeepingController : Controller
{
    private readonly ApplicationDbContext _context;

    public HousekeepingController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var tasks = _context.HousekeepingTasks.Include(h => h.Room).ToList();
        return View(tasks);
    }

    [HttpGet]
    public IActionResult Create()
    {
        // Possibly pass a list of rooms
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(HousekeepingTask task)
    {
        if (!ModelState.IsValid) return View(task);
        _context.HousekeepingTasks.Add(task);
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Complete(int id)
    {
        var task = await _context.HousekeepingTasks.FindAsync(id);
        if (task == null) return NotFound();
        task.IsCompleted = true;
        await _context.SaveChangesAsync();
        return RedirectToAction("Index");
    }
}
