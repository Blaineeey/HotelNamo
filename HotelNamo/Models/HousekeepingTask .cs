namespace HotelNamo.Models
{
    public class HousekeepingTask
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        // Navigation property
        public Room Room { get; set; }

        public string Description { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsCompleted { get; set; }
    }

}
