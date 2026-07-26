using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    public class HarvestViewModel
    {
        [Required(ErrorMessage = "Please select a crop cycle.")]
        public int CropCycleId { get; set; }

        [Required(ErrorMessage = "Harvest Date is required.")]
        [DataType(DataType.Date)]
        public DateTime HarvestDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Expected Quantity is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Expected Quantity must be greater than 0.")]
        public decimal ExpectedQuantity { get; set; }

        [Required(ErrorMessage = "Actual Quantity is required.")]
        [Range(0.01, 100000.00, ErrorMessage = "Actual Quantity must be greater than 0.")]
        public decimal ActualQuantity { get; set; }

        [Required(ErrorMessage = "Unit of measurement is required.")]
        [StringLength(50)]
        public string Unit { get; set; } = "Quintal"; // Quintal, Kg, Ton, Liter

        [StringLength(50)]
        public string Status { get; set; } = "Stored"; // Stored, Listed, Processed
    }
}
