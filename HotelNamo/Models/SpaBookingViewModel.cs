using System;
using System.ComponentModel.DataAnnotations;

namespace HotelNamo.Models
{
    public class SpaBookingViewModel
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
        [Display(Name = "Number of Guests")]
        public int NumberOfGuests { get; set; }

        [Required(ErrorMessage = "Please select your preferred date")]
        [DataType(DataType.Date)]
        [Display(Name = "Preferred Date")]
        public DateTime PreferredDate { get; set; }

        [Required(ErrorMessage = "Please select your preferred time")]
        [Display(Name = "Preferred Time")]
        public string PreferredTime { get; set; }

        [Required(ErrorMessage = "Please select a treatment")]
        [Display(Name = "Treatment")]
        public string Treatment { get; set; }

        [Display(Name = "Special Requests")]
        public string SpecialRequests { get; set; }

        [Required(ErrorMessage = "You must agree to the terms and conditions")]
        [Display(Name = "Terms and Conditions")]
        public bool AgreeToTerms { get; set; }
    }
}