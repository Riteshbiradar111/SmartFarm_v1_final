using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartFarmMVC.Models.ViewModels
{
    public class CropMonitoringViewModel
    {
        [Required(ErrorMessage = "Please select a crop cycle.")]
        public int CropCycleId { get; set; }

        [Required(ErrorMessage = "Observation Date is required.")]
        [DataType(DataType.Date)]
        public DateTime ObservationDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Growth Stage is required.")]
        [StringLength(100)]
        public string GrowthStage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plant Height is required.")]
        [Range(0.01, 500.00, ErrorMessage = "Plant Height must be greater than 0.")]
        public decimal PlantHeight { get; set; }

        [Required(ErrorMessage = "Crop Health is required.")]
        [StringLength(100)]
        public string CropHealth { get; set; } = "Good"; // Good, Excellent, Fair, Poor, Diseased

        [StringLength(500)]
        public string? Remarks { get; set; }

        public IFormFile? ImageFile { get; set; }
    }
}
