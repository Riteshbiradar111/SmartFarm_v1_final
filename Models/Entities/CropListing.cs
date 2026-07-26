using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CropListing")]
    public class CropListing
    {
        [Key]
        public int ListingId { get; set; }

        [Required]
        public int HarvestId { get; set; }

        [Required]
        public decimal PricePerUnit { get; set; }

        [Required]
        public decimal AvailableQuantity { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Available, Sold, Pending

        [Required]
        public DateTime ListedDate { get; set; } = DateTime.Now;

        // Path to uploaded crop image
        [StringLength(500)]
        public string? ImagePath { get; set; }

        // --- Purchase details ---
        public int? BuyerId { get; set; }

        public decimal? PurchasedQuantity { get; set; }

        public DateTime? PurchaseDate { get; set; }



        // Navigation properties
        [ForeignKey("HarvestId")]
        public virtual Harvest Harvest { get; set; } = null!;

        [ForeignKey("BuyerId")]
        public virtual Buyer? Buyer { get; set; }
    }
}
