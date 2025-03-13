using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelNamo.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }

        [Required]
        public string IssueDescription { get; set; }

        public string Status { get; set; } = "Pending";

        [Required]
        public int RoomId { get; set; }  // Ensure RoomId exists
        public Room Room { get; set; }

        public string AssignedStaffId { get; set; }
        [ForeignKey("AssignedStaffId")]
        public ApplicationUser AssignedStaff { get; set; }

    }
}
