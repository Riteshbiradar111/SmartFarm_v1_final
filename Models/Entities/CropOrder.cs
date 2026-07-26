using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("CropOrder")]
    public class CropOrder
    {
        [Key]
        public int OrderId { get; set; }

        public int? ListingId { get; set; }

        public int? HarvestId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int FarmerId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerUnit { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? AcceptedDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [StringLength(100)]
        public string? InvoiceNumber { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? GST { get; set; }

        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        [StringLength(500)]
        public string? SpecialInstructions { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(BuyerId))]
        public virtual Buyer? Buyer { get; set; }

        [ForeignKey(nameof(FarmerId))]
        public virtual Farmer? Farmer { get; set; }

        [ForeignKey(nameof(ListingId))]
        public virtual CropListing? CropListing { get; set; }

        [ForeignKey(nameof(HarvestId))]
        public virtual Harvest? Harvest { get; set; }
    }
}
