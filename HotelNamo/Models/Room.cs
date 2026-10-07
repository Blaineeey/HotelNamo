namespace HotelNamo.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }

        // New property for room description
        public string Description { get; set; }
    }
}
