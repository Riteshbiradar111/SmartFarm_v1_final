using System;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class FarmerHarvestViewModel
    {
        public string FarmerName { get; set; } = string.Empty;
        public string PlotName { get; set; } = string.Empty;
        public string CropName { get; set; } = string.Empty;
        public DateTime SowingDate { get; set; }
        public DateTime ExpectedHarvestDate { get; set; }
        public decimal ActualYield { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
