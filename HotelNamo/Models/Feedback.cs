using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HotelNamo.Models
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // User who submitted the feedback

        [Required]
        public int BookingId { get; set; } // Required booking reference

        [Required]
        public int RoomId { get; set; } // Required room reference

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // ⭐ 1-5 Star Rating

        [Required]
        [MaxLength(500)]
        public string Review { get; set; } // Feedback content

        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
    }
}