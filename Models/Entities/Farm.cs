using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("Farm")]
    public class Farm
    {
        [Key]
        public int FarmId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        [StringLength(150)]
        public string FarmName { get; set; } = null!;

        [StringLength(100)]
        public string? Village { get; set; }

        [StringLength(100)]
        public string? Taluka { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(10)]
        public string? Pincode { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("FarmerId")]
        public virtual Farmer Farmer { get; set; } = null!;

        public virtual ICollection<LandPlot> LandPlots { get; set; } = new List<LandPlot>();
    }
}
