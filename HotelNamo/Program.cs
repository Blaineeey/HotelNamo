using HotelNamo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Set up the connection to SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Set up Identity with ApplicationUser
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();  // Add this to enable authentication
app.UseAuthorization();

// Seed roles and admin user if not already seeded
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Create roles if they don't exist
        var roles = new[] { "Admin", "User" };
        foreach (var role in roles)
        {
            var roleExists = await roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation($"Role {role} created.");
            }
        }

        // Create admin user if it doesn't exist
        var adminUser = await userManager.FindByEmailAsync("admin@user.com");
        if (adminUser == null)
        {
            var user = new ApplicationUser
            {
                UserName = "admin@user.com",
                Email = "admin@user.com",
                FirstName = "Admin",
                LastName = "User"
            };
            var result = await userManager.CreateAsync(user, "Admin123");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                logger.LogInformation("Admin user created and assigned to Admin role.");
            }
            else
            {
                logger.LogError("Failed to create admin user.");
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during the seeding process.");
    }
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
