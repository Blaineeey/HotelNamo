using HotelNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HotelNamo.Data
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure roles exist
            string[] roles = { "Admin", "User", "FrontDesk", "HouseKeeping", "Maintenance" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin user explicitly
            string adminEmail = "admin@hotelnamo.com";
            string adminPassword = "Admin123!";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create context explicitly once at top clearly
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Explicitly Seed Amenities first (if none)
            if (!context.Amenities.Any())
            {
                context.Amenities.AddRange(
                    new Amenity { Name = "WiFi", Description = "Free high-speed internet" },
                    new Amenity { Name = "TV", Description = "High-quality Television" },
                    new Amenity { Name = "Air Conditioning", Description = "Cooling system" },
                    new Amenity { Name = "Breakfast Included", Description = "Free breakfast each morning" }
                );
                await context.SaveChangesAsync();
            }

            // Explicitly Seed Rooms before RoomImages
            if (!context.Rooms.Any())
            {
                context.Rooms.AddRange(
                    new Room { RoomNumber = "101", FloorNumber = 1, Category = "Single", Price = 100M, Status = "Vacant", Description = "Single Room" },
                    new Room { RoomNumber = "102", FloorNumber = 1, Category = "Double", Price = 120, Status = "Vacant", Description = "Comfortable double room." },
                    new Room { RoomNumber = "201", FloorNumber = 2, Category = "Suite", Price = 250, Status = "Vacant", Description = "Luxurious suite." }
                );
                await context.SaveChangesAsync();
            }

            // Now explicitly seed RoomImages after rooms seeded
            if (!context.RoomImages.Any())
            {
                var firstRoom = context.Rooms.FirstOrDefault(r => r.Category == "Single");
                var secondRoom = context.Rooms.FirstOrDefault(r => r.Category == "Double");
                var suiteRoom = context.Rooms.FirstOrDefault(r => r.Category == "Suite");

                if (firstRoom != null && secondRoom != null && suiteRoom != null)
                {
                    context.RoomImages.AddRange(
                        new RoomImage { RoomId = firstRoom.Id, ImagePath = "single-room.jpg" },
                        new RoomImage { RoomId = secondRoom.Id, ImagePath = "guest-room.jpg" },
                        new RoomImage { RoomId = suiteRoom.Id, ImagePath = "superior-room.jpg" }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
