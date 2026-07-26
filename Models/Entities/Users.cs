using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = null!; // Aligned with DB: PasswordHash

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = null!;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsDeleted { get; set; } = false;

        [Required]
        public bool IsBlocked { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Aligned with DB: CreatedAt

        public DateTime? LastLogin { get; set; }

        // Navigation property to Role
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        // Navigation properties for associated Farmers/Buyers
        public virtual ICollection<Farmer> Farmers { get; set; } = new List<Farmer>();
        public virtual ICollection<Buyer> Buyers { get; set; } = new List<Buyer>();
    }
}
