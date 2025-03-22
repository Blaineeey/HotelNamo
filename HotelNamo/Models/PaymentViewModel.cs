
    using System.ComponentModel.DataAnnotations;

    namespace HotelNamo.Models // Adjust namespace to match your project
    {
        public class PaymentViewModel
        {
            [Required]
            public int BookingId { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            public decimal Amount { get; set; }

            [Required]
            public string PaymentMethod { get; set; } // e.g. "CreditCard", "PayPal", etc.
        }
    }
