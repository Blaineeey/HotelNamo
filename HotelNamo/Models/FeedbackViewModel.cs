using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class FeedbackViewModel
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int RoomId { get; set; }

        // Room information to display to the user
        public string RoomNumber { get; set; }

        public string RoomCategory { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        [Required(ErrorMessage = "Please provide a rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Please share your experience")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Review must be between 10 and 500 characters")]
        public string Review { get; set; }
    }

    public class RoomReviewsViewModel
    {
        public int RoomId { get; set; }

        public string RoomNumber { get; set; }

        public string RoomCategory { get; set; }

        public decimal AverageRating { get; set; }

        public int TotalReviews { get; set; }

        public List<Feedback> Reviews { get; set; }

        // Pagination properties
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public bool HasPreviousPage => CurrentPage > 1;

        public bool HasNextPage => CurrentPage < TotalPages;
    }
}