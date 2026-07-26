using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Buyer")]
    public class Buyer
    {
        [Key]
        public int BuyerId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        [StringLength(150)]
        public string? CompanyName { get; set; }

        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = null!;

        [StringLength(200)]
        public string? BusinessAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? PinCode { get; set; }

        [StringLength(250)]
        public string? ProfilePicturePath { get; set; }

        // Navigation property to User
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
