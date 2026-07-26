using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    /// <summary>
    /// ViewModel for Admin Dashboard - aggregates all dashboard data
    /// </summary>
    public class AdminDashboardViewModel
    {
        // KPI Statistics
        public int TotalUsers { get; set; }
        public int UsersAddedThisMonth { get; set; }
        public int ActiveFarms { get; set; }
        public int TotalStates { get; set; }
        public int ActiveCrops { get; set; }
        public int PendingApprovals { get; set; }

        // Pending User Approvals
        public List<PendingUserApprovalDto> PendingUserApprovals { get; set; } = new List<PendingUserApprovalDto>();

        // System Audit Logs
        public List<SystemAuditLogDto> RecentAuditLogs { get; set; } = new List<SystemAuditLogDto>();

        // Chart Data - User Growth (Last 12 Months)
        public UserGrowthChartData UserGrowthData { get; set; } = new UserGrowthChartData();

        // Chart Data - User Distribution by Role
        public UserDistributionChartData UserDistributionData { get; set; } = new UserDistributionChartData();
    }

    /// <summary>
    /// Pending user approval row data
    /// </summary>
    public class PendingUserApprovalDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string RoleBadgeClass { get; set; } = string.Empty; // CSS class for role badge color
        public DateTime CreatedAt { get; set; }
        public string JoinedDateFormatted { get; set; } = string.Empty;
    }

    /// <summary>
    /// System audit log entry
    /// </summary>
    public class SystemAuditLogDto
    {
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "blue"; // green, orange, blue, red
    }

    /// <summary>
    /// User growth chart data (12 months)
    /// </summary>
    public class UserGrowthChartData
    {
        public List<string> Labels { get; set; } = new List<string>(); // Month names
        public List<int> Data { get; set; } = new List<int>(); // User counts
    }

    /// <summary>
    /// User distribution by role (pie/donut chart)
    /// </summary>
    public class UserDistributionChartData
    {
        public List<string> Labels { get; set; } = new List<string>(); // Role names
        public List<int> Data { get; set; } = new List<int>(); // User counts per role
        public List<string> BackgroundColors { get; set; } = new List<string>(); // Chart colors

        public UserDistributionChartData()
        {
            // Default colors matching the screenshot
            BackgroundColors = new List<string>
            {
                "#2D6A4F", // Farmer - Dark Green
                "#17a2b8", // Agronomist - Cyan
                "#4169E1", // Field Officer - Royal Blue
                "#9B59B6", // Coop Manager - Purple
                "#FF8C00", // Buyer - Orange
                "#dc2626"  // Admin - Red
            };
        }
    }
}
