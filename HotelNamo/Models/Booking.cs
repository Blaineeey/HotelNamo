using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelNamo.Models
{
    public class Booking
    {
        public int Id { get; set; }

        // UserId is optional for guest bookings
        public string? UserId { get; set; }

        // For guest bookings
        public string? GuestName { get; set; }
        public string? GuestEmail { get; set; }
        public string? GuestPhone { get; set; }
        
        [NotMapped]
        public bool IsGuestBooking => string.IsNullOrEmpty(UserId);

        [Required]
        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public string? SpecialRequests { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Maintain your new fields
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }

        // Feedback relationship
        public Feedback? Feedback { get; set; }

        [NotMapped]
        public bool CanSubmitFeedback => IsConfirmed && CheckOutDate < DateTime.UtcNow && Feedback == null;
    }
}