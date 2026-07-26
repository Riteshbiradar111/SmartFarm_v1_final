using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        public int FarmId { get; set; }

        [Required]
        public int OfficerId { get; set; }

        [Required]
        [StringLength(200)]
        public string Task { get; set; } = null!;

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Overdue

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime? CompletedDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        [ForeignKey("FarmId")]
        public virtual Farm Farm { get; set; } = null!;

        [ForeignKey("OfficerId")]
        public virtual User Officer { get; set; } = null!;
    }
}
