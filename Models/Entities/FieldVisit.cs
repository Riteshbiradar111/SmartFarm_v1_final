using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("FieldVisit")]
    public class FieldVisit
    {
        [Key]
        public int VisitId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        public int? PlotId { get; set; }

        [Required]
        public int AssignedOfficerId { get; set; }

        public DateTime? VisitDate { get; set; }

        [StringLength(50)]
        public string? VisitTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Scheduled, InProgress, Completed

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        public string? Notes { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        [ForeignKey("PlotId")]
        public virtual LandPlot? LandPlot { get; set; }

        [ForeignKey("AssignedOfficerId")]
        public virtual User AssignedOfficer { get; set; } = null!;

        // Inverse navigation
        public virtual ICollection<VisitPhoto> VisitPhotos { get; set; } = new List<VisitPhoto>();
    }
}
