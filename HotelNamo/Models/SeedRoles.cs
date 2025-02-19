using Microsoft.AspNetCore.Identity;

namespace HotelNamo.Models
{
    public class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            string[] roleNames = { "Admin", "User", "Manager" };

            // Seed roles if they don't already exist
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create an admin user if it doesn't exist
            var adminEmail = "admin@localhost.com";  // Use an appropriate local email for the admin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User"
                };
                await userManager.CreateAsync(adminUser, "AdminPassword123!");  // Use a strong password

                // Assign the 'Admin' role to the admin user
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
