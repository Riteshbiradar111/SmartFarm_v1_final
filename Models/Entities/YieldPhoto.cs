using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("YieldPhoto")]
    public class YieldPhoto
    {
        [Key]
        public int PhotoId { get; set; }

        [Required]
        public int YieldId { get; set; }

        [StringLength(1000)]
        public string? FilePath { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("YieldId")]
        public virtual YieldRecord YieldRecord { get; set; } = null!;
    }
}
