using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("FieldOfficerAssignment")]
    public class FieldOfficerAssignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int FieldOfficerUserId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.Now;

        [Required]
        public bool IsActive { get; set; } = true;

        // Navigation properties
        [ForeignKey("FieldOfficerUserId")]
        public virtual User FieldOfficer { get; set; } = null!;

        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;
    }
}
