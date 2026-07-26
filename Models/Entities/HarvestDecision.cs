using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("HarvestDecision")]
    public class HarvestDecision
    {
        [Key]
        public int DecisionId { get; set; }

        [Required]
        public int HarvestId { get; set; }

        [Required]
        public int AgronomistId { get; set; }

        [Required]
        [StringLength(1000)]
        public string CropHealthAnalysis { get; set; } = null!;

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
        public string Decision { get; set; } = null!; // Approved, Reinspection Required

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("HarvestId")]
        public virtual Harvest Harvest { get; set; } = null!;

        [ForeignKey("AgronomistId")]
        public virtual Agronomist Agronomist { get; set; } = null!;
    }
}
