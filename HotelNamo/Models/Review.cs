public class Review
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int RoomId { get; set; }
    public Room Room { get; set; }
    public int Rating { get; set; } // 1 to 5 stars
    public string Comment { get; set; }
    public DateTime ReviewDate { get; set; }
}
