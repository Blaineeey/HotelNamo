using System;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class TableReservation
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
        public DateTime ReservationDate { get; set; }

        [Required]
        public string ReservationTime { get; set; }

        [Required]
        public string TableNumber { get; set; }

        // Dining venue (e.g., "The Grand Restaurant", "Lounge & Bar")
        [Required]
        public string Venue { get; set; }

        public string SpecialRequests { get; set; }

        // Occasion (e.g., Birthday, Anniversary)
        public string Occasion { get; set; }

        // Dietary restrictions
        public string DietaryRestrictions { get; set; }

        [Required]
        public DateTime BookingDate { get; set; } = DateTime.Now;

        [Required]
        public string Status { get; set; } = "Confirmed";

        // Optional: Add relationship to user if booking is associated with a registered user
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}