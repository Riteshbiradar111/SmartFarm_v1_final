using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Crop")]
    public class Crop
    {
        [Key]
        public int CropId { get; set; }

        [Required]
        [StringLength(100)]
        public string CropName { get; set; } = null!;

        [StringLength(50)]
        public string? Season { get; set; }

        [Required]
        public int DurationDays { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property for Crop Cycles
        public virtual ICollection<CropCycle> CropCycles { get; set; } = new List<CropCycle>();
    }
}
