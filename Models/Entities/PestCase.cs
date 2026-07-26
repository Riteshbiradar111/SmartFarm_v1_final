using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("PestCase")]
    public class PestCase
    {
        [Key]
        public int PestCaseId { get; set; }

        [Required]
        public int CropCycleId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = null!;

        [StringLength(300)]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = null!; // High, Medium, Low

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Report Uploaded, Field Visit, Resolved

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // New workflow columns matching database schema
        public DateTime? ReportUploadedDate { get; set; }

        public DateTime? FieldVisitCompletedDate { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [StringLength(50)]
        public string? FarmerResponseToReport { get; set; }

        public DateTime? FarmerResponseDate { get; set; }

        [Required]
        public bool IsClosed { get; set; } = false;

        public DateTime? ClosedDate { get; set; }

        // --- Field Visit / recommendation workflow extensions ---
        public bool FieldVisitRequested { get; set; } = false;

        public int? AssignedOfficerId { get; set; }

        [StringLength(1000)]
        public string? FieldReport { get; set; }

        [StringLength(1000)]
        public string? Recommendation { get; set; }



        // Navigation properties
        [ForeignKey("CropCycleId")]
        public virtual CropCycle CropCycle { get; set; } = null!;

        public virtual User? AssignedOfficer { get; set; }
    }
}
