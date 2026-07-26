using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Report")]
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [StringLength(200)]
        public string ReportName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string ReportType { get; set; } = null!; // Crop Report, Farm Report, Revenue Report, Yield Report, Assignment Report

        [Required]
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(100)]
        public string GeneratedBy { get; set; } = null!; // System, Admin, Analyst, Finance, etc.

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Generated"; // Generated, Exported, Pending

        [StringLength(100)]
        public string? RelatedModule { get; set; } // User, Farm, Crop, Assignment, etc.

        public int? RelatedEntityId { get; set; } // Reference to the related entity (optional)

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? ExportedDate { get; set; }

        public bool IsExported { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
