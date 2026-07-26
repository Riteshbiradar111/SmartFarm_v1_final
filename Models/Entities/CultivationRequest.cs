using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CultivationRequest")]
    public class CultivationRequest
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        public int FarmId { get; set; }

        [Required]
        public int PlotId { get; set; }

        [Required]
        public int CropId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal CultivationArea { get; set; }

        [Required]
        public DateTime SowingDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(4, 2)")]
        public decimal SoilPH { get; set; }

        [Required]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal MoistureLevel { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Under Analysis, Approved, Approved With Suggestions, Needs Improvement, Rejected, Resubmitted (superseded by a newer request)

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        [ForeignKey("FarmId")]
        public virtual Farm Farm { get; set; } = null!;

        [ForeignKey("PlotId")]
        public virtual LandPlot LandPlot { get; set; } = null!;

        [ForeignKey("CropId")]
        public virtual Crop Crop { get; set; } = null!;
    }
}
