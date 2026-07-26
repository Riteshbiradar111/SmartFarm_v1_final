using System;
using System.ComponentModel.DataAnnotations;

namespace SmartFarmMVC.Models.ViewModels
{
    // This viewmodel handles form validation when placing a standard order or pre-order.
    public class CropOrderViewModel
    {
        // 1. Marketplace listing ID (optional, set if buying from catalog)
        public int? ListingId { get; set; }

        // 2. Harvest ID (optional, set if pre-ordering a ready harvest)
        public int? HarvestId { get; set; }

        // 3. Purchase quantity requested by the buyer (must be greater than 0)
        [Required(ErrorMessage = "Please enter the quantity you wish to purchase.")]
        [Range(0.01, 999999.99, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal Quantity { get; set; }

        // 4. Delivery address (required)
        [Required(ErrorMessage = "Please enter your delivery address.")]
        [StringLength(500)]
        public string? DeliveryAddress { get; set; }

        // 5. Special delivery instructions (optional)
        [StringLength(500)]
        public string? SpecialInstructions { get; set; }
    }
}
