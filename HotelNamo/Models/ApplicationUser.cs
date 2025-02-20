using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public int Id { get; set; }  // Ensure this matches the database
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Role { get; set; } // Admin, Guest, Housekeeping, FrontDesk
}
