using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CropMonitoring")]
    public class CropMonitoring
    {
        [Key]
        public int MonitoringId { get; set; }

        [Required]
        public int CropCycleId { get; set; }

        [Required]
        public DateTime ObservationDate { get; set; }

        [Required]
        [StringLength(100)]
        public string GrowthStage { get; set; } = null!;

        [Required]
        public decimal PlantHeight { get; set; }

        [Required]
        [StringLength(100)]
        public string CropHealth { get; set; } = null!;

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(300)]
        public string? ImagePath { get; set; }

        // Navigation property
        [ForeignKey("CropCycleId")]
        public virtual CropCycle CropCycle { get; set; } = null!;
    }
}
