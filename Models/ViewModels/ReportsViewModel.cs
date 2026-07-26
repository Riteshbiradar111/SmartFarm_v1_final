using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class ReportsViewModel
    {
        // KPI Statistics
        public int TotalReports { get; set; }
        public int GeneratedThisMonth { get; set; }
        public int ExportedReports { get; set; }
        public int PendingReports { get; set; }

        // Report List
        public List<ReportDto> Reports { get; set; } = new List<ReportDto>();

        // Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? ReportTypeFilter { get; set; }
    }

    public class ReportDto
    {
        public int ReportId { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public string ReportType { get; set; } = string.Empty;
        public string ReportTypeBadgeClass { get; set; } = string.Empty;
        public DateTime GeneratedDate { get; set; }
        public string GeneratedDateFormatted { get; set; } = string.Empty;
        public string GeneratedBy { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? RelatedModule { get; set; }
        public bool IsExported { get; set; }
    }
}
