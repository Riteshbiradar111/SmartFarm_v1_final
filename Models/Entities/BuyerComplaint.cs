using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("BuyerComplaint")]
    public class BuyerComplaint
    {
        [Key]
        public int ComplaintId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        [StringLength(100)]
        public string ComplaintType { get; set; } = null!;

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = null!;

        [StringLength(255)]
        public string? ImagePath { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(1000)]
        public string? ResolutionText { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ResolvedDate { get; set; }

        // Navigation properties
        [ForeignKey("BuyerId")]
        public virtual Buyer Buyer { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual CropOrder CropOrder { get; set; } = null!;
    }
}
