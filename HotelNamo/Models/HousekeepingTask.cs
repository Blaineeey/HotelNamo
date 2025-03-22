using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelNamo.Models
{
    public class HousekeepingTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string TaskDescription { get; set; }

        public DateTime DueDate { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        // ✅ Make AssignedStaffId Nullable
        public string? AssignedStaffId { get; set; }
        [ForeignKey("AssignedStaffId")]
        public ApplicationUser? AssignedStaff { get; set; }

        public string Status { get; set; } = "Pending"; // Default Status

        public DateTime? CompletedAt { get; set; }
    }
}
