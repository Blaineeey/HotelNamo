using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Threading.Tasks;
using HotelNamo.Models;

[Authorize(Roles = "User,Admin")] // Allow only logged-in users
public class UserController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public UserController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> UserHome()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var userData = GetUserDashboard(user.Id); // Call the correct method
        return View(userData);
    }


    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    private UserDashboardModel GetUserDashboard(int userId)
    {
        var model = new UserDashboardModel();

        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand(@"
            SELECT 
                (SELECT COUNT(*) FROM Reservations WHERE userId = @UserId) AS TotalReservations,
                (SELECT ISNULL(SUM(amount), 0) 
                 FROM Payments 
                 WHERE reservationId IN (SELECT reservationId FROM Reservations WHERE userId = @UserId)) AS TotalSpent,
                (SELECT TOP 1 RoomID FROM Reservations WHERE userId = @UserId ORDER BY checkInDate DESC) AS LastRoomBooked
            ", connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        model.TotalReservations = reader.GetInt32(0);
                        model.TotalSpent = reader.GetDecimal(1);
                        model.LastRoomBooked = reader.IsDBNull(2) ? "No bookings yet" : reader.GetString(2);
                    }
                }
            }
        }

        return model;
    }

}
