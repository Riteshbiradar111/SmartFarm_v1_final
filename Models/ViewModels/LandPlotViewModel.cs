using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    public class LandPlotViewModel
    {
        [Required(ErrorMessage = "Please select a farm.")]
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Plot Name is required.")]
        [StringLength(100, ErrorMessage = "Plot Name cannot exceed 100 characters.")]
        public string PlotName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Area is required.")]
        [Range(0.01, 10000.00, ErrorMessage = "Area must be greater than 0.")]
        public decimal Area { get; set; }

        [Required(ErrorMessage = "Area Unit is required.")]
        [StringLength(50)]
        public string AreaUnit { get; set; } = "Acres";

        [Required(ErrorMessage = "Latitude is required (please click on the map).")]
        [Range(-90.000000, 90.000000, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required (please click on the map).")]
        [Range(-180.000000, 180.000000, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal Longitude { get; set; }

        [Required(ErrorMessage = "Soil Type is required.")]
        [StringLength(100)]
        public string SoilType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Irrigation Type is required.")]
        [StringLength(100)]
        public string IrrigationType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Plot Code is required.")]
        [StringLength(50, ErrorMessage = "Plot Code cannot exceed 50 characters.")]
        public string PlotCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string Status { get; set; } = "Active";
    }
}
