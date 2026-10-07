namespace HotelNamo.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Category { get; set; } // e.g., Deluxe, Suite, Standard
        public string Status { get; set; }   // e.g., Occupied, Vacant, Maintenance
        public decimal Price { get; set; }
        // Additional fields as needed
    }
}