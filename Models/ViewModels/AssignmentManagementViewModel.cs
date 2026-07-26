using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class AssignmentManagementViewModel
    {
        // Statistics
        public int TotalAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public int CompletedAssignments { get; set; }
        public int OverdueAssignments { get; set; }

        // Assignment List
        public List<AssignmentDto> Assignments { get; set; } = new List<AssignmentDto>();

        // Search and Filters
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? OfficerFilter { get; set; }
    }

    public class AssignmentDto
    {
        public int AssignmentId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string FarmerInitials { get; set; } = string.Empty;
        public string OfficerName { get; set; } = string.Empty;
        public string OfficerInitials { get; set; } = string.Empty;
        public string FarmName { get; set; } = string.Empty;
        public string Task { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public string AssignedDateFormatted { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string DueDateFormatted { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string PriorityBadgeClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }

    public class CreateAssignmentViewModel
    {
        [Required(ErrorMessage = "Farmer is required")]
        public int FarmerId { get; set; }

        [Required(ErrorMessage = "Farm is required")]
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Officer is required")]
        public int OfficerId { get; set; }

        [Required(ErrorMessage = "Task description is required")]
        [StringLength(200)]
        public string Task { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assigned date is required")]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium";

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class EditAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Farmer is required")]
        public int FarmerId { get; set; }

        [Required(ErrorMessage = "Farm is required")]
        public int FarmId { get; set; }

        [Required(ErrorMessage = "Officer is required")]
        public int OfficerId { get; set; }

        [Required(ErrorMessage = "Task description is required")]
        [StringLength(200)]
        public string Task { get; set; } = string.Empty;

        [Required(ErrorMessage = "Assigned date is required")]
        public DateTime AssignedDate { get; set; }

        [Required(ErrorMessage = "Due date is required")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        [StringLength(50)]
        public string Priority { get; set; } = string.Empty;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Notes { get; set; }
    }

    public class ViewAssignmentViewModel
    {
        public int AssignmentId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string FarmerPhone { get; set; } = string.Empty;
        public string OfficerName { get; set; } = string.Empty;
        public string OfficerEmail { get; set; } = string.Empty;
        public string FarmName { get; set; } = string.Empty;
        public string FarmLocation { get; set; } = string.Empty;
        public string Task { get; set; } = string.Empty;
        public DateTime AssignedDate { get; set; }
        public string AssignedDateFormatted { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public string DueDateFormatted { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? CompletedDateFormatted { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedDateFormatted { get; set; } = string.Empty;
    }

    public class OfficerOption
    {
        public int OfficerId { get; set; }
        public string OfficerName { get; set; } = string.Empty;
    }
}
