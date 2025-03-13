using System.ComponentModel.DataAnnotations;
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

        // Ensure this collection is initialized explicitly
        public ICollection<RoomImage> RoomImages { get; set; } = new List<RoomImage>();

        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
        // ✅ **Fix: Add the missing Bookings navigation property**
        public List<Booking>? Bookings { get; set; } = new List<Booking>();
        public List<HousekeepingTask>? HousekeepingTasks { get; set; }
    }
}