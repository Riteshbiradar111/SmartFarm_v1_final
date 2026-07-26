using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Harvest")]
    public class Harvest
    {
        [Key]
        public int HarvestId { get; set; }

        [Required]
        public int CropCycleId { get; set; }

        [Required]
        public DateTime HarvestDate { get; set; }

        [Required]
        public decimal ExpectedQuantity { get; set; }

        [Required]
        public decimal ActualQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = null!;

        [StringLength(50)]
        public string? Status { get; set; }

        // Navigation properties
        [ForeignKey("CropCycleId")]
        public virtual CropCycle CropCycle { get; set; } = null!;

        public virtual ICollection<CropListing> CropListings { get; set; } = new List<CropListing>();
    }
}
