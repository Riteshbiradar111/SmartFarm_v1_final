using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartFarmMVC.Models.ViewModels
{
    public class PestCaseViewModel
    {
        [Required(ErrorMessage = "Please select a crop cycle.")]
        public int CropCycleId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required.")]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public IFormFile? ImageFile { get; set; }
    }
}
