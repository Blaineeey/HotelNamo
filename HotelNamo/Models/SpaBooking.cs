using System;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class SpaBooking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string Phone { get; set; }

        [Required]
        public int NumberOfGuests { get; set; }

        [Required]
        public DateTime PreferredDate { get; set; }

        [Required]
        public string PreferredTime { get; set; }

        [Required]
        public string Treatment { get; set; }

        public string SpecialRequests { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Confirmed";

        // Optional: Add relationship to user if booking is associated with a registered user
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}