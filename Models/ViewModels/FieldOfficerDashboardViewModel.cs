using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Field Officer Dashboard
    /// </summary>
    public class FieldOfficerDashboardViewModel
    {
        // KPI Statistics
        public int TotalFarmerRegistrations { get; set; }
        public int PendingPlotsVerification { get; set; }
        public int TotalSensorReadings { get; set; }
        public int OpenIncidentsCount { get; set; }

        // Pending Mappings List
        public List<PendingPlotDto> PendingMappings { get; set; } = new List<PendingPlotDto>();

        // Recent Field Visit Activity Feed (replaces hardcoded dummy items)
        public List<RecentVisitDto> RecentVisits { get; set; } = new List<RecentVisitDto>();

        // Soil type distribution for pie chart (key = soil type label, value = count)
        public Dictionary<string, int> SoilTypeData { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// DTO representing a pending plot mapping request
    /// </summary>
    public class PendingPlotDto
    {
        public int PlotId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public string PlotName { get; set; } = string.Empty;
        public string PlotCode { get; set; } = string.Empty;
        public decimal Area { get; set; }
        public string SoilType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for the Field Visit Activity Feed on the dashboard
    /// </summary>
    public class RecentVisitDto
    {
        public string FarmerName { get; set; } = string.Empty;
        public string PlotInfo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? VisitDate { get; set; }
        public string DotColor { get; set; } = "#2D6A4F";
    }
}
