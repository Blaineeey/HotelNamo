namespace HotelNamo.Models
{
    public class Amenity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<RoomAmenity> RoomAmenities { get; set; } = new List<RoomAmenity>();
    }

}
