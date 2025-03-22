namespace HotelNamo.Models
{
    public class RoomImage
    {
        public int Id { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int RoomId { get; set; }

        public Room Room { get; set; }
    }

}
