using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    // This class maps to the Wishlist table in SQL Server.
    // It allows buyers to save favorite crops or farmers, and register for notifications.
    [Table("Wishlist")]
    public class Wishlist
    {
        // 1. Primary Key
        [Key]
        public int WishlistId { get; set; }

        // 2. Foreign Key linking to the Buyer who saved this item
        [Required]
        public int BuyerId { get; set; }

        // 3. Foreign Key linking to a specific Crop type (optional)
        public int? CropId { get; set; }

        // 4. Foreign Key linking to a saved Farmer profile (optional)
        public int? FarmerId { get; set; }

        // 5. If true, user gets notified when the crop is back in stock
        [Required]
        public bool NotifyWhenAvailable { get; set; } = false;

        // 6. Date when this item was added to the wishlist
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // --- Navigation Properties ---
        // These link this record to the associated Buyer, Crop, and Farmer tables.

        [ForeignKey("BuyerId")]
        public virtual Buyer Buyer { get; set; } = null!;

        [ForeignKey("CropId")]
        public virtual Crop? Crop { get; set; }

        [ForeignKey("FarmerId")]
        public virtual Farmer? Farmer { get; set; }
    }
}
