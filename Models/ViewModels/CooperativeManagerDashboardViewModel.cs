using System;
using System.Collections.Generic;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class MemberFarmPerformanceItem
    {
        // Stores performance details of each farmer.10    
        // Used to display farmer performance on the dashboard.
        public int FarmerId { get; set; }
        public string FarmerName { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int TotalFarms { get; set; }
        public int TotalPlots { get; set; }
        public double TotalAreaAcres { get; set; }
        public string ActiveCrops { get; set; } = string.Empty;
        public decimal TotalHarvestQuantity { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public int OpenIssuesCount { get; set; }
        public string AssignedStaffName { get; set; } = string.Empty;
        public string ProduceStatus { get; set; } = "Active";
    }

    // ViewModel used for Cooperative Manager Dashboard.   
    // Combines dashboard statistics and farmer data.
    public class CooperativeManagerDashboardViewModel
    {
        public string ManagerFullName { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;

        // 8 Dynamic KPI Metrics
        public int TotalFarmers { get; set; }
        public int PendingFarmerIssues { get; set; }
        public int ActiveAssignments { get; set; }
        public int ResolvedCases { get; set; }
        public int PendingFieldVisits { get; set; }
        public int AgronomistReviewsPending { get; set; }
        public int TotalPestCases { get; set; }
        public int ActiveImprovementPlans { get; set; }

        // Member Farm Performance List (Real DB Data)
        public List<MemberFarmPerformanceItem> MemberFarmPerformance { get; set; } = new List<MemberFarmPerformanceItem>();

        // Cultivation plans / requests
        public List<CultivationRequest> CultivationPlans { get; set; } = new List<CultivationRequest>();
    }
}
