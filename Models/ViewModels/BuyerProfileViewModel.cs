using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    // This viewmodel handles form validation for the Buyer's profile settings page.
    // It is kept simple and distinct from the Farmer profile viewmodel.
    public class BuyerProfileViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Company Name is required.")]
        [StringLength(150, ErrorMessage = "Company Name cannot exceed 150 characters.")]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile Number is required.")]
        [StringLength(15, ErrorMessage = "Mobile Number cannot exceed 15 characters.")]
        public string MobileNumber { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Business Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters.")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "District cannot exceed 100 characters.")]
        public string? District { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string? State { get; set; }

        [StringLength(10, ErrorMessage = "Pin Code cannot exceed 10 characters.")]
        public string? PinCode { get; set; }

        public string? ProfilePicturePath { get; set; }

        public Microsoft.AspNetCore.Http.IFormFile? ProfilePictureFile { get; set; }
    }
}
