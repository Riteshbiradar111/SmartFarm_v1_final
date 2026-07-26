using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    public class CropCycleViewModel
    {
        [Required(ErrorMessage = "Please select a plot.")]
        public int PlotId { get; set; }

        [Required(ErrorMessage = "Please select a crop.")]
        public int CropId { get; set; }

        [Required(ErrorMessage = "Sowing Date is required.")]
        [DataType(DataType.Date)]
        public DateTime SowingDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Expected Harvest Date is required.")]
        [DataType(DataType.Date)]
        public DateTime ExpectedHarvestDate { get; set; } = DateTime.Today.AddDays(90);

        [Required(ErrorMessage = "Current Stage is required.")]
        [StringLength(100)]
        public string CurrentStage { get; set; } = "Sowing"; // Sowing, Vegetative, Flowering, Yielding, Harvesting

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // Active, Completed, Failed
    }
}
