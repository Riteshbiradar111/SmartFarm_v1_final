using System;

namespace SmartFarmMVC.Models.ViewModels
{
    // This viewmodel holds search and filter parameters submitted from the marketplace index view.
    public class MarketplaceSearchViewModel
    {
        //  Text search by Crop name or keyword
        public string? CropName { get; set; }

        //  Text search by Farmer's name
        public string? FarmerName { get; set; }

        //  Text search by specific Village
        public string? Village { get; set; }

        //  Text search by specific District
        public string? District { get; set; }

        //  Category filter pill (e.g. Grains, Vegetables, Fruits, Cash Crops)
        public string? Category { get; set; }

        //  Max price limit filter
        public decimal? MaxPrice { get; set; }

        //  Min quantity available filter
        public decimal? MinQuantity { get; set; }
    }
}
