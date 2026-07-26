using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Agronomist")]
    public class Agronomist
    {
        [Key]
        public int AgronomistId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = null!;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
