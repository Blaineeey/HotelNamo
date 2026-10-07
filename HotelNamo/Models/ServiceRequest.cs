public class ServiceRequest
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public string RequestType { get; set; } // Spa, Airport Transfer, Room Service
    public string Status { get; set; } // Pending, Completed
}
