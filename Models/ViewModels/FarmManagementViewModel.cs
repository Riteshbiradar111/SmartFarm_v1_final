using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class FarmManagementViewModel
    {
        // Statistics
        public int TotalFarms { get; set; }
        public int ActiveFarms { get; set; }
        public int InactiveFarms { get; set; }
        public decimal AverageFarmSize { get; set; }

        // Farm List
        public List<FarmDto> Farms { get; set; } = new List<FarmDto>();

        // Search and Filters
        public string? SearchTerm { get; set; }
        public string? StateFilter { get; set; }
        public string? DistrictFilter { get; set; }
        public string? CropFilter { get; set; }
    }

    public class FarmDto
    {
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerInitials { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public decimal AreaHa { get; set; }
        public string AreaFormatted { get; set; } = string.Empty;
        public string MainCrop { get; set; } = string.Empty;
        public string SoilType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedDateFormatted { get; set; } = string.Empty;
    }

    public class CreateFarmViewModel
    {
        [Required(ErrorMessage = "Farm name is required")]
        [StringLength(150)]
        public string FarmName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Owner is required")]
        public int FarmerId { get; set; }

        [StringLength(100)]
        public string? Village { get; set; }

        [StringLength(100)]
        public string? Taluka { get; set; }

        [Required(ErrorMessage = "District is required")]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [StringLength(10)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string? Pincode { get; set; }
    }

    public class EditFarmViewModel
    {
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Farm name is required")]
        [StringLength(150)]
        public string FarmName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Owner is required")]
        public int FarmerId { get; set; }

        [StringLength(100)]
        public string? Village { get; set; }

        [StringLength(100)]
        public string? Taluka { get; set; }

        [Required(ErrorMessage = "District is required")]
        [StringLength(100)]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "State is required")]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [StringLength(10)]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Pincode must be 6 digits")]
        public string? Pincode { get; set; }
    }

    public class ViewFarmViewModel
    {
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string Taluka { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public decimal TotalAreaHa { get; set; }
        public int TotalPlots { get; set; }
        public string MainCrop { get; set; } = string.Empty;
        public string SoilTypes { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedDateFormatted { get; set; } = string.Empty;
    }

    public class FarmerOption
    {
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
    }

    public class FarmOption
    {
        public int FarmId { get; set; }
        public string FarmName { get; set; } = string.Empty;
        public int FarmerId { get; set; }
    }
}
