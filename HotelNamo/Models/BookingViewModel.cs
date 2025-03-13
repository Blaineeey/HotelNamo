using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class BookingViewModel
    {
        [Required]
        public int RoomId { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required, DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        public string? SpecialRequests { get; set; }
    }
}
