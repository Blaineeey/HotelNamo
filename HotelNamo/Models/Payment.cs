using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelNamo.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int BookingId { get; set; }  // ✅ This is required

        [ForeignKey("BookingId")]
        public Booking? Booking { get; set; }  // ❌ Make this nullable (not required)

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Credit Card";

        public string? TransactionId { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public bool IsPaid { get; set; } = false;
    }
}
