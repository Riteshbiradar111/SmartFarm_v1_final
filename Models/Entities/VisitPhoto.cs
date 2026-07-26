using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("VisitPhoto")]
    public class VisitPhoto
    {
        [Key]
        public int PhotoId { get; set; }

        [Required]
        public int VisitId { get; set; }

        [StringLength(1000)]
        public string? FilePath { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("VisitId")]
        public virtual FieldVisit FieldVisit { get; set; } = null!;
    }
}
