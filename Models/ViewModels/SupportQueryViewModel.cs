using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SmartFarmMVC.Models.ViewModels
{
    public class SupportQueryViewModel
    {
        [Required(ErrorMessage = "Query type is required.")]
        [Display(Name = "Query Type")]
        public string QueryType { get; set; } = null!;

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Farm is required.")]
        [Display(Name = "Farm")]
        public int FarmId { get; set; }

        [Display(Name = "Land Plot (Optional)")]
        public int? PlotId { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        public string Priority { get; set; } = "Medium"; // High, Medium, Low

        [Display(Name = "Attach Image (Optional)")]
        public IFormFile? ImageFile { get; set; }
    }
}
