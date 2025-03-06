namespace HotelNamo.Models
{
    public class MaintenanceRequest
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        // Navigation property
        public Room Room { get; set; }
        public string IssueDescription { get; set; }
        public DateTime RequestDate { get; set; }
        public bool IsResolved { get; set; }
    }

}
