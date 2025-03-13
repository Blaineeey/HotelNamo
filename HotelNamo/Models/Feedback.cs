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
        public string UserId { get; set; } // ✅ Ensures UserId is required

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; } // ⭐ 1-5 Star Rating

        [Required]
        [MaxLength(500)]
        public string Review { get; set; } // ✅ Ensures Review exists

        public DateTime DateSubmitted { get; set; } = DateTime.UtcNow; // ✅ Ensures DateSubmitted exists

        // ✅ Relationship with User
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
