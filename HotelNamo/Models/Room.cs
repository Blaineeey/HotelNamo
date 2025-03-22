using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HotelNamo.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        public int FloorNumber { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        public decimal Price { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        // Make Description explicitly nullable (since it can be optional)
        public string? Description { get; set; }

        // Navigation properties with proper initialization
        public ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();
        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        public List<Booking> Bookings { get; set; } = new List<Booking>();
        public List<HousekeepingTask>? HousekeepingTasks { get; set; } = new List<HousekeepingTask>();

        // Add feedback collection to track all reviews for this room
        public List<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        // Helper property to calculate average rating
        [NotMapped]
        public decimal AverageRating => Feedbacks.Any() ? (decimal)Feedbacks.Average(f => f.Rating) : 0;

        // Helper property to get feedback count
        [NotMapped]
        public int FeedbackCount => Feedbacks.Count;
    }
}