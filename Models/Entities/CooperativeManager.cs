using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CooperativeManager")]
    public class CooperativeManager
    {
        [Key]
        public int ManagerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string CooperativeName { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = null!;

        [StringLength(100)]
        public string? Region { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
