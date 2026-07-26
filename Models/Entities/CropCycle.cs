using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CropCycle")]
    public class CropCycle
    {
        [Key]
        public int CropCycleId { get; set; }

        [Required]
        public int PlotId { get; set; }

        [Required]
        public int CropId { get; set; }

        [Required]
        public DateTime SowingDate { get; set; }

        [Required]
        public DateTime ExpectedHarvestDate { get; set; }

        [StringLength(100)]
        public string? CurrentStage { get; set; }

        [StringLength(50)]
        public string? Status { get; set; }

        // Navigation properties
        [ForeignKey("PlotId")]
        public virtual LandPlot LandPlot { get; set; } = null!;

        [ForeignKey("CropId")]
        public virtual Crop Crop { get; set; } = null!;

        public virtual ICollection<CropMonitoring> CropMonitorings { get; set; } = new List<CropMonitoring>();
        public virtual ICollection<PestCase> PestCases { get; set; } = new List<PestCase>();
        public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();
    }
}
