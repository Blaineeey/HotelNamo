using Microsoft.AspNetCore.Identity;

namespace HotelNamo.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Preferences { get; set; } // e.g. "Non-smoking, near elevator"
                                                // etc.
    }

}
