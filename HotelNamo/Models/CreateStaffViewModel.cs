using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class CreateStaffViewModel
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public List<string> AvailableRoles { get; set; }
        public string SelectedRole { get; set; }
    }
}
