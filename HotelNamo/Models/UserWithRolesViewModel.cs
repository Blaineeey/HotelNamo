using System.Collections.Generic;

namespace HotelNamo.Models
{
    public class UserWithRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
