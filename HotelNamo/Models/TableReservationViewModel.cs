using System;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class TableReservationViewModel
    {
        [Required(ErrorMessage = "Please enter your full name")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter your phone number")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select the number of guests")]
        [Range(1, 10, ErrorMessage = "Number of guests must be between 1 and 10")]
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }

        [Required(ErrorMessage = "Please select your reservation date")]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Please select your reservation time")]
        [Display(Name = "Reservation Time")]
        public string ReservationTime { get; set; }

        [Required(ErrorMessage = "Please select a table")]
        [Display(Name = "Table Number")]
        public string TableNumber { get; set; }

        [Required(ErrorMessage = "Please select a dining venue")]
        [Display(Name = "Dining Venue")]
        public string Venue { get; set; }

        // Non-required fields for additional information
        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }

        [Display(Name = "Occasion")]
        public string Occasion { get; set; }

        [Display(Name = "Dietary Restrictions")]
        public string DietaryRestrictions { get; set; }

        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        [Display(Name = "Terms and Conditions")]
        public bool AgreeToTerms { get; set; }
    }
}