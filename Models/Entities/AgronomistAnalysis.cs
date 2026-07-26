using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("AgronomistAnalysis")]
    public class AgronomistAnalysis
    {
        [Key]
        public int AnalysisId { get; set; }

        [Required]
        public int RequestId { get; set; }

        [Required]
        public int AgronomistId { get; set; }

        [Required]
        [StringLength(1000)]
        public string SoilAnalysis { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string WeatherAnalysis { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string CropAnalysis { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string PestAnalysis { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string DiseaseAnalysis { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Recommendation { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Decision { get; set; } = null!; // Approved, Approved With Suggestions, Needs Improvement, Rejected

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RequestId")]
        public virtual CultivationRequest CultivationRequest { get; set; } = null!;

        [ForeignKey("AgronomistId")]
        public virtual Agronomist Agronomist { get; set; } = null!;
    }
}
