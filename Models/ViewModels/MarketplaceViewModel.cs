using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    public class MarketplaceViewModel
    {
        [Required(ErrorMessage = "Please select a harvest record.")]
        public int HarvestId { get; set; }

        [Required(ErrorMessage = "Price per Unit is required.")]
        [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0.")]
        public decimal PricePerUnit { get; set; }

        [Required(ErrorMessage = "Available Quantity is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal AvailableQuantity { get; set; }

        [Required(ErrorMessage = "Unit of measurement is required.")]
        [StringLength(50)]
        public string Unit { get; set; } = "Quintal";
    }
}
