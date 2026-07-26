using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("YieldValidation")]
    public class YieldValidation
    {
        [Key]
        public int ValidationId { get; set; }

        [Required]
        public int YieldId { get; set; }

        [Required]
        public int FieldOfficerUserId { get; set; }

        [Required]
        public DateTime ValidationDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string ValidationStatus { get; set; } = null!; // Valid, NeedsCorrection, Rejected

        public string? Comments { get; set; }

        // Navigation properties
        [ForeignKey("YieldId")]
        public virtual YieldRecord YieldRecord { get; set; } = null!;

        [ForeignKey("FieldOfficerUserId")]
        public virtual User FieldOfficer { get; set; } = null!;
    }
}
