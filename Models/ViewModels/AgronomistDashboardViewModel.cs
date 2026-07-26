using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Agronomist Dashboard
    /// </summary>
    public class AgronomistDashboardViewModel
    {
        // KPI Statistics
        public int ActiveIssuesCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int ResolvedCount { get; set; }
        public int TotalAssignedCount { get; set; }

        // Lists
        public List<AssignedFarmerDto> AssignedFarmers { get; set; } = new List<AssignedFarmerDto>();
        public List<AssignedIssueDto> AssignedIssues { get; set; } = new List<AssignedIssueDto>();
    }

    /// <summary>
    /// DTO representing a farmer assigned to an agronomist
    /// </summary>
    public class AssignedFarmerDto
    {
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FarmName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string PrimaryCrops { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO representing an issue assigned to an agronomist
    /// </summary>
    public class AssignedIssueDto
    {
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // High, Medium, Low
        public string Status { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty; // "Pest Case" or "Support Query"
        public DateTime CreatedDate { get; set; }
        public string FarmerName { get; set; } = string.Empty;
    }
}
