using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class GuestBookingViewModel
    {
        [Required]
        public int RoomId { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        public string? SpecialRequests { get; set; }

        // Guest information (only required for guest bookings)
        [Required(ErrorMessage = "Please enter your full name")]
        [Display(Name = "Full Name")]
        public string GuestName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string GuestPhone { get; set; } = string.Empty;
    }
}