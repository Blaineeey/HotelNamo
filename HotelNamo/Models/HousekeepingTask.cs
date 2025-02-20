public class HousekeepingTask
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public string Status { get; set; } // Pending, Completed
}
