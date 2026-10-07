using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HotelNamo.Models
{
    public class Booking
    {
        public int Id { get; set; }
        [BindNever]
         public string UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public bool IsConfirmed { get; set; }
        public Room Room { get; set; }  // Navigation property
        public ApplicationUser User { get; set; } // Navigation property

        // Add these properties (make them nullable if you only set them at check-in/out):
        public DateTime? ActualCheckInTime { get; set; }
        public DateTime? ActualCheckOutTime { get; set; }
    }

}
    