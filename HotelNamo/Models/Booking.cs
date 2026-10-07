using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{

    public class Booking
    {
        public int Id { get; set; }

        [Required]
        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        [Required, DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public bool IsConfirmed { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? SpecialRequests { get; set; }

        // Add explicitly these two missing properties:
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }
        // Explicitly add calculated price property
        [DataType(DataType.Currency)]
        public decimal TotalPrice { get; set; }
    }

}
