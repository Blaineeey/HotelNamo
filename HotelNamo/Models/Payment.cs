public class Payment
{
    public int Id { get; set; }
    public int ReservationId { get; set; }
    public Reservation Reservation { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentStatus { get; set; } // Pending, Completed, Failed
    public string PaymentMethod { get; set; } // Ensure this exists
}
