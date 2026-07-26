using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Farmer")]
    public class Farmer
    {
        [Key]
        public int FarmerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = null!;

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? Village { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(50)]
        public string? Taluka { get; set; }

        [StringLength(20)]
        public string? EmergencyContact { get; set; }

        [StringLength(255)]
        public string? ProfilePicturePath { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        // Navigation property for Farmer's Farms
        public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
    }
}
