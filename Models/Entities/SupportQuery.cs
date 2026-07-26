using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("SupportQuery")]
    public class SupportQuery
    {
        [Key]
        public int QueryId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        [StringLength(100)]
        public string QueryType { get; set; } = null!; // General Farming Query, Field Visit Request, Irrigation Issue, Equipment Issue, Crop Issue, Other

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [Required]
        public int FarmId { get; set; }

        public int? PlotId { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = null!; // High, Medium, Low

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Under Review, Assigned, Field Visit Scheduled, Resolved

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int? AssignedToUserId { get; set; }

        public DateTime? ResolutionDate { get; set; }

        // --- Agronomist Recommendation ---
        [StringLength(1000)]
        public string? AgronomistRecommendation { get; set; }

        public DateTime? RecommendationDate { get; set; }

        // --- Field Officer Report ---
        public DateTime? VisitDate { get; set; }

        [StringLength(100)]
        public string? OfficerName { get; set; }

        [StringLength(1000)]
        public string? FieldObservation { get; set; }

        [StringLength(1000)]
        public string? ActionTaken { get; set; }

        [StringLength(255)]
        public string? ReportImagePath { get; set; }

        // --- Improvement Plan ---
        [StringLength(1000)]
        public string? ImprovementActions { get; set; }

        [StringLength(500)]
        public string? ImprovementExpectedBenefits { get; set; }

        [StringLength(50)]
        public string? ImprovementStatus { get; set; } = "Not Started"; // Not Started, In Progress, Completed

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        [ForeignKey("FarmId")]
        public virtual Farm Farm { get; set; } = null!;

        [ForeignKey("PlotId")]
        public virtual LandPlot? LandPlot { get; set; }

        [ForeignKey("AssignedToUserId")]
        public virtual User? AssignedToUser { get; set; }
    }
}
