using HotelNamo.Data;
using HotelNamo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class RoomController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Anyone can view rooms (no login required)
    [AllowAnonymous]
    public IActionResult Index()
    {
        // Show all rooms or just those not under maintenance
        var rooms = _context.Rooms.ToList();
        return View(rooms);
    }

    // Optionally let users see details for a specific room
    [AllowAnonymous]
    public IActionResult Details(int id)
    {
        var room = _context.Rooms.Find(id);
        if (room == null) return NotFound();
        return View(room);
    }

    // Only Admin or FrontDesk can create
    [Authorize(Roles = "Admin,FrontDesk")]
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "Admin,FrontDesk")]
    public async Task<IActionResult> Create(Room room, int[] selectedAmenities, List<IFormFile> images)
    {
        if (!ModelState.IsValid)
            return View(room);

        room.RoomAmenities = selectedAmenities.Select(a => new RoomAmenity { AmenityId = a }).ToList();
        room.RoomImages = new List<RoomImage>();

        // Loop through each uploaded file
        foreach (var image in images)
        {
            if (image.Length > 0)
            {
                // Generate a unique file name clearly
                var fileName = Guid.NewGuid() + Path.GetExtension(image.FileName);

                // Define the path clearly
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "rooms", fileName);

                // Save the file clearly
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                // Add the image record to RoomImages
                var roomImage = new RoomImage { ImagePath = fileName };
                room.RoomImages.Add(roomImage);
            }
        }

        // Add amenities if applicable (ensure you handle amenities selection clearly)
        room.RoomAmenities = room.RoomAmenities ?? new List<RoomAmenity>();

        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index");
    }

    // Similarly implement Edit, Delete with [Authorize(Roles = "Admin,FrontDesk")]
}
