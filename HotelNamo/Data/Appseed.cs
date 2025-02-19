using HotelNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public class AppSeed
{
    public static async Task SeedAdminUser(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
    {
        // Check if the admin user already exists
        var adminUser = await userManager.FindByEmailAsync("admin@user.com");

        if (adminUser == null)
        {
            // Create the admin user
            var newAdmin = new ApplicationUser
            {
                UserName = "admin@user.com",
                Email = "admin@user.com",
                FirstName = "Admin",
                LastName = "User"
            };

            var result = await userManager.CreateAsync(newAdmin, "Admin123");

            if (result.Succeeded)
            {
                // Assign the "Admin" role to this user
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
}
