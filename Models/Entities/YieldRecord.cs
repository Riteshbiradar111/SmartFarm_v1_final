using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("YieldRecord")]
    public class YieldRecord
    {
        [Key]
        public int YieldId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        public int? PlotId { get; set; }

        public int? CropId { get; set; }

        public int? SubmittedByUserId { get; set; }

        [Column(TypeName = "decimal(12,4)")]
        public decimal? Area { get; set; }

        [Column(TypeName = "decimal(14,2)")]
        public decimal? EstimatedYield { get; set; }

        [Required]
        [StringLength(20)]
        public string Unit { get; set; } = "kg";

        [Required]
        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Submitted";

        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        [ForeignKey("PlotId")]
        public virtual LandPlot? LandPlot { get; set; }

        [ForeignKey("CropId")]
        public virtual Crop? Crop { get; set; }

        [ForeignKey("SubmittedByUserId")]
        public virtual User? SubmittedByUser { get; set; }

        // Inverse navigation
        public virtual ICollection<YieldPhoto> YieldPhotos { get; set; } = new List<YieldPhoto>();
        public virtual ICollection<YieldValidation> YieldValidations { get; set; } = new List<YieldValidation>();
    }
}
