using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    public class FarmViewModel
    {
        [Required(ErrorMessage = "Farm Name is required.")]
        [StringLength(150, ErrorMessage = "Farm Name cannot exceed 150 characters.")]
        public string FarmName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Village name is required.")]
        [StringLength(100, ErrorMessage = "Village cannot exceed 100 characters.")]
        public string Village { get; set; } = string.Empty;

        [Required(ErrorMessage = "Taluka is required.")]
        [StringLength(100, ErrorMessage = "Taluka cannot exceed 100 characters.")]
        public string Taluka { get; set; } = string.Empty;

        [Required(ErrorMessage = "District is required.")]
        [StringLength(100, ErrorMessage = "District cannot exceed 100 characters.")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required.")]
        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters.")]
        public string State { get; set; } = string.Empty;

        [Required(ErrorMessage = "Pincode is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be exactly 6 digits.")]
        public string Pincode { get; set; } = string.Empty;
    }
}
