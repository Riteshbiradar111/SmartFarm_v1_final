using System;

namespace SmartFarmMVC.Models.ViewModels
{
    // This viewmodel holds search and filter parameters submitted from the marketplace index view.
    public class MarketplaceSearchViewModel
    {
        // 1. Text search by Crop name or keyword
        public string? CropName { get; set; }

        // 2. Text search by Farmer's name
        public string? FarmerName { get; set; }

        // 3. Text search by specific Village
        public string? Village { get; set; }

        // 4. Text search by specific District
        public string? District { get; set; }

        // 5. Category filter pill (e.g. Grains, Vegetables, Fruits, Cash Crops)
        public string? Category { get; set; }

        // 6. Max price limit filter
        public decimal? MaxPrice { get; set; }

        // 7. Min quantity available filter
        public decimal? MinQuantity { get; set; }
    }
}
