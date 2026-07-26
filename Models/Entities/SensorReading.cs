using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("SensorReading")]
    public class SensorReading
    {
        [Key]
        public int ReadingId { get; set; }

        [Required]
        public int PlotId { get; set; }

        [Required]
        public decimal SoilMoisture { get; set; }

        [Required]
        public decimal SoilPH { get; set; }

        [Required]
        public decimal Nitrogen { get; set; }

        [Required]
        public decimal Phosphorus { get; set; }

        [Required]
        public decimal Potassium { get; set; }

        [Required]
        public decimal ElectricalConductivity { get; set; }

        [Required]
        public decimal OrganicCarbon { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        // Navigation property to LandPlot
        [ForeignKey("PlotId")]
        public virtual LandPlot Plot { get; set; } = null!;
    }
}
