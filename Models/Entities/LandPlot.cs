using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("LandPlot")]
    public class LandPlot
    {
        [Key]
        public int PlotId { get; set; }

        [Required]
        public int FarmId { get; set; }

        [Required]
        [StringLength(100)]
        public string PlotName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string PlotCode { get; set; } = null!;

        [Required]
        public decimal Area { get; set; }

        [Required]
        [StringLength(50)]
        public string AreaUnit { get; set; } = null!;

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        [StringLength(100)]
        public string? SoilType { get; set; }

        [StringLength(100)]
        public string? IrrigationType { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        // Navigation properties
        [ForeignKey("FarmId")]
        public virtual Farm Farm { get; set; } = null!;

        public virtual ICollection<CropCycle> CropCycles { get; set; } = new List<CropCycle>();
    }
}
