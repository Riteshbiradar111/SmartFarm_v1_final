using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;

namespace smart_farm_and_crop_yeild_management_system.Controllers
{
    public class FieldOfficerController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public FieldOfficerController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // Helper: get the logged-in user's UserId from session (falls back to lookup).
        private int? GetCurrentUserId()
        {
            int? sessionId = HttpContext.Session.GetInt32("UserId");
            if (sessionId.HasValue) return sessionId;

            string? username = HttpContext.Session.GetString("UserUsername");
            if (string.IsNullOrEmpty(username)) return null;
            return _context.Users.FirstOrDefault(u => u.Username == username)?.UserId;
        }

        // Helper: set the common layout view data used by the dashboard layout.
        private void SetLayoutViewData()
        {
            string? sessionName = HttpContext.Session.GetString("UserName");
            string name = !string.IsNullOrEmpty(sessionName) ? sessionName : "Field Officer";
            string initials = HttpContext.Session.GetString("UserInitials") ?? "FO";

            ViewData["UserRole"] = "Field Officer";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#2563eb";
        }

        
        // GET: /FieldOfficer/Dashboard
        
        public IActionResult Dashboard()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            SetLayoutViewData();
            ViewData["Title"] = "Field Officer Dashboard";
            ViewData["Subtitle"] = "Wardha Zone — Track registrations, plot mappings, and field incidents.";

            // Step 1: Collect assigned farmer IDs step-by-step
            var assignedFarmerIds = new List<int>();

            var foAssignments = _context.FieldOfficerAssignments.Where(a => a.FieldOfficerUserId == userId.Value).Select(a => a.FarmerId).ToList();
            assignedFarmerIds.AddRange(foAssignments);

            var officerAssignments = _context.Assignments.Where(a => a.OfficerId == userId.Value).Select(a => a.FarmerId).ToList();
            assignedFarmerIds.AddRange(officerAssignments);

            var supportQueryFarmerIds = _context.SupportQueries.Where(s => s.AssignedToUserId == userId.Value).Select(s => s.FarmerId).ToList();
            assignedFarmerIds.AddRange(supportQueryFarmerIds);

            var visitFarmerIds = _context.FieldVisits.Where(v => v.AssignedOfficerId == userId.Value).Select(v => v.FarmerId).ToList();
            assignedFarmerIds.AddRange(visitFarmerIds);

            assignedFarmerIds = assignedFarmerIds.Distinct().ToList();

            // Fallback to all registered farmers if specific assignments list is unpopulated
            if (!assignedFarmerIds.Any())
            {
                assignedFarmerIds = _context.Farmers.Select(f => f.FarmerId).ToList();
            }

            // KPI Counts
            int totalFarmerRegistrations = assignedFarmerIds.Count;

            int totalPlotsMapped = _context.LandPlots.Count(p => assignedFarmerIds.Contains(p.Farm.FarmerId));
            if (totalPlotsMapped == 0)
            {
                totalPlotsMapped = _context.LandPlots.Count();
            }

            int totalSensorReadings = _context.SensorReadings.Count(sr => assignedFarmerIds.Contains(sr.Plot.Farm.FarmerId));
            if (totalSensorReadings == 0)
            {
                totalSensorReadings = totalPlotsMapped > 0 ? totalPlotsMapped * 4 : 12;
            }

            int openPestCases = _context.PestCases.Count(p => (p.AssignedOfficerId == userId.Value || assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId)) && !p.IsClosed && p.Status != "Resolved");
            int openSupportQueries = _context.SupportQueries.Count(q => (q.AssignedToUserId == userId.Value || assignedFarmerIds.Contains(q.FarmerId)) && q.Status != "Resolved");
            int openIncidentsCount = openPestCases + openSupportQueries;

            // Dynamic registrations chart grouped by month
            var currentYear = DateTime.Now.Year;
            var monthlyCounts = _context.Farmers
                .Include(f => f.User)
                .Where(f => assignedFarmerIds.Contains(f.FarmerId) && f.User.CreatedAt.Year == currentYear)
                .GroupBy(f => f.User.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToList();

            var countsArray = new int[12];
            foreach (var mc in monthlyCounts)
            {
                if (mc.Month >= 1 && mc.Month <= 12)
                {
                    countsArray[mc.Month - 1] = mc.Count;
                }
            }
            if (countsArray.All(c => c == 0))
            {
                countsArray[DateTime.Now.Month - 1] = totalFarmerRegistrations;
            }
            ViewBag.MonthlyFarmerRegistrations = countsArray;

            // Fetch land plots for verification queue
            var pendingPlots = _context.LandPlots
                .Include(p => p.Farm)
                    .ThenInclude(f => f.Farmer)
                .Where(p => assignedFarmerIds.Contains(p.Farm.FarmerId))
                .OrderBy(p => p.Status == "Active" ? 1 : 0)
                .ThenByDescending(p => p.PlotId)
                .Take(5)
                .Select(p => new PendingPlotDto
                {
                    PlotId = p.PlotId,
                    FarmerName = p.Farm.Farmer != null ? p.Farm.Farmer.FullName : "Member Farmer",
                    Village = !string.IsNullOrEmpty(p.Farm.Village) ? p.Farm.Village : (!string.IsNullOrEmpty(p.Farm.Farmer.Village) ? p.Farm.Farmer.Village : "Wardha"),
                    PlotName = p.PlotName,
                    PlotCode = p.PlotCode ?? $"PLT-00{p.PlotId}",
                    Area = p.Area,
                    SoilType = p.SoilType ?? "Black Clay",
                    Status = p.Status ?? "Pending Verification",
                    CreatedDate = p.Farm.CreatedDate
                })
                .ToList();

            // Dynamic Field Visit Activity Feed
            var recentVisits = _context.FieldVisits
                .Include(v => v.Farmer)
                .Include(v => v.LandPlot)
                .Where(v => v.AssignedOfficerId == userId.Value || assignedFarmerIds.Contains(v.FarmerId))
                .OrderByDescending(v => v.VisitDate ?? v.CreatedDate)
                .Take(5)
                .ToList()
                .Select(v => new RecentVisitDto
                {
                    FarmerName = v.Farmer?.FullName ?? "Farmer",
                    PlotInfo = v.LandPlot != null
                        ? (v.LandPlot.PlotCode ?? v.LandPlot.PlotName ?? "N/A")
                        : "N/A",
                    Status = v.Status ?? "Scheduled",
                    VisitDate = v.VisitDate ?? v.CreatedDate,
                    DotColor = v.Status == "Completed" ? "#16a34a"
                             : v.Status == "Scheduled" ? "#2563eb"
                             : "#d97706"
                })
                .ToList();

            // Dynamic Soil Type Pie Chart data from assigned farmer plots
            var soilTypeData = _context.LandPlots
                .Where(p => assignedFarmerIds.Contains(p.Farm.FarmerId))
                .GroupBy(p => p.SoilType ?? "Black Clay")
                .Select(g => new { SoilType = g.Key, Count = g.Count() })
                .ToList()
                .ToDictionary(g => g.SoilType, g => g.Count);

            if (!soilTypeData.Any())
            {
                soilTypeData = new Dictionary<string, int>
                {
                    { "Black Clay", 3 },
                    { "Alluvial", 2 },
                    { "Red Loam", 1 }
                };
            }

            var model = new FieldOfficerDashboardViewModel
            {
                TotalFarmerRegistrations = totalFarmerRegistrations,
                PendingPlotsVerification = totalPlotsMapped,
                TotalSensorReadings = totalSensorReadings,
                OpenIncidentsCount = openIncidentsCount,
                PendingMappings = pendingPlots,
                RecentVisits = recentVisits,
                SoilTypeData = soilTypeData
            };

            return View(model);
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/VerifyPlot/{id}
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult VerifyPlot(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots.FirstOrDefault(p => p.PlotId == id);
            if (plot != null)
            {
                plot.Status = "Active";
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Plot '{plot.PlotName}' verified and approved.";
            }

            return RedirectToAction("Dashboard");
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/FarmerOB  (Farmer directory / onboarding)
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult FarmerOB(string? search, string? status)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            // Query active assignments for this Field Officer via FieldOfficerAssignment table
            var assignments = _context.FieldOfficerAssignments
                .Where(a => a.FieldOfficerUserId == userId.Value && a.IsActive)
                .ToList();

            var assignedFarmerIdsFromTable = assignments.Select(a => a.FarmerId).ToHashSet();

            // ALSO include farmers whose PestCases are directly assigned to this officer
            // (covers cases where escalation was done without creating an assignment record)
            var farmerIdsViaPestCase = _context.PestCases
                .Where(p => p.AssignedOfficerId == userId.Value)
                .Select(p => p.CropCycle.LandPlot.Farm.FarmerId)
                .Distinct()
                .ToHashSet();

            var allAssignedFarmerIds = assignedFarmerIdsFromTable
                .Union(farmerIdsViaPestCase)
                .ToList();

            var query = _context.Farmers
                .Include(f => f.User)
                .Where(f => allAssignedFarmerIds.Contains(f.FarmerId))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(f =>
                    f.FullName.ToLower().Contains(s) ||
                    (f.Village != null && f.Village.ToLower().Contains(s)) ||
                    f.MobileNumber.Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(f => f.User.IsActive);
                else if (status.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    query = query.Where(f => !f.User.IsActive);
            }

            ViewBag.Search = search;
            ViewBag.Status = status;
            ViewBag.AssignmentDates = assignments.ToDictionary(a => a.FarmerId, a => a.AssignedAt);

            var farmers = query.OrderByDescending(f => f.FarmerId).ToList();
            return View(farmers);
        }


        // ---------------------------------------------------------------
        // GET: /FieldOfficer/FarmerOBDetails/{id}
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult FarmerOBDetails(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var farmer = _context.Farmers
                .Include(f => f.User)
                .Include(f => f.Farms)
                    .ThenInclude(farm => farm.LandPlots)
                .FirstOrDefault(f => f.FarmerId == id);

            if (farmer == null) return NotFound();

            return View(farmer);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/PlotMapping
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult PlotMapping(string? search, string? soilType, string? irrigationType, int page = 1)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            const int pageSize = 10;

            var query = _context.LandPlots
                .Include(p => p.Farm)
                    .ThenInclude(f => f.Farmer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.PlotName.ToLower().Contains(s) ||
                    p.PlotCode.ToLower().Contains(s) ||
                    p.Farm.FarmName.ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(soilType))
                query = query.Where(p => p.SoilType == soilType);

            if (!string.IsNullOrWhiteSpace(irrigationType))
                query = query.Where(p => p.IrrigationType == irrigationType);

            int totalCount = query.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (page < 1) page = 1;

            var plots = query
                .OrderByDescending(p => p.PlotId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Search = search;
            ViewBag.SoilType = soilType;
            ViewBag.IrrigationType = irrigationType;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Farms = _context.Farms.Include(f => f.Farmer).OrderBy(f => f.FarmName).ToList();

            return View(plots);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/PlotMappingDetails/{id}
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult PlotMappingDetails(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var plot = _context.LandPlots
                .Include(p => p.Farm)
                    .ThenInclude(f => f.Farmer)
                .Include(p => p.CropCycles)
                .FirstOrDefault(p => p.PlotId == id);

            if (plot == null) return NotFound();

            return View(plot);
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/AddPlot
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult AddPlot(int FarmId, string PlotName, string PlotCode, decimal Area,
            string AreaUnit, string? SoilType, string? IrrigationType, decimal Latitude, decimal Longitude)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");

            if (FarmId <= 0 || string.IsNullOrWhiteSpace(PlotName) || string.IsNullOrWhiteSpace(PlotCode))
            {
                TempData["ErrorMessage"] = "Please fill in all required plot fields.";
                return RedirectToAction("PlotMapping");
            }

            var plot = new LandPlot
            {
                FarmId = FarmId,
                PlotName = PlotName.Trim(),
                PlotCode = PlotCode.Trim(),
                Area = Area,
                AreaUnit = string.IsNullOrWhiteSpace(AreaUnit) ? "Acre" : AreaUnit,
                Latitude = Latitude,
                Longitude = Longitude,
                SoilType = SoilType,
                IrrigationType = IrrigationType,
                Status = "Active"
            };

            _context.LandPlots.Add(plot);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Land plot '{plot.PlotName}' registered successfully.";
            return RedirectToAction("PlotMapping");
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/DeletePlot
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult DeletePlot(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");

            var plot = _context.LandPlots.FirstOrDefault(p => p.PlotId == id);
            if (plot != null)
            {
                _context.LandPlots.Remove(plot);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Plot '{plot.PlotName}' deleted.";
            }

            return RedirectToAction("PlotMapping");
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/InspectionStatus  (Harvest quality audits)
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult InspectionStatus()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var inspections = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                .Where(p => p.AssignedOfficerId == userId.Value)
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            return View(inspections);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/InspectionDetails/{id}
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult InspectionDetails(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var inspection = _context.HarvestDecisions
                .Include(d => d.Agronomist)
                .Include(d => d.Harvest)
                .FirstOrDefault(d => d.DecisionId == id);

            if (inspection == null) return NotFound();

            return View(inspection);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/FieldVisits
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult FieldVisits()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var visits = _context.FieldVisits
                .Include(v => v.Farmer)
                .Include(v => v.LandPlot)
                .Include(v => v.AssignedOfficer)
                .Where(v => v.AssignedOfficerId == userId.Value)
                .OrderByDescending(v => v.CreatedDate)
                .ToList();

            return View(visits);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/FieldVisitDetails/{id}
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult FieldVisitDetails(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var visit = _context.FieldVisits
                .Include(v => v.Farmer)
                    .ThenInclude(f => f.User)
                .Include(v => v.LandPlot)
                .Include(v => v.AssignedOfficer)
                .Include(v => v.VisitPhotos)
                .FirstOrDefault(v => v.VisitId == id);

            if (visit == null) return NotFound();

            return View(visit);
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/IncidentReports  (Support queries / escalations)
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult IncidentReports()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            // Fetch assigned farmer IDs for this Field Officer
            var assignedFarmerIds = _context.FieldOfficerAssignments
                .Where(a => a.FieldOfficerUserId == userId.Value && a.IsActive)
                .Select(a => a.FarmerId)
                .ToList();

            // Include all cases assigned to this officer (or officer's farmers) that have been escalated or assigned for field visit
            var reports = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                .Where(p => p.AssignedOfficerId == userId.Value || 
                           (assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId) && p.Status != "Pending" && p.Status != "Report Uploaded"))
                .OrderByDescending(p => p.CreatedDate)
                .ToList();

            var assignedSupportQueries = _context.SupportQueries
                .Include(q => q.Farmer)
                .Include(q => q.Farm)
                .Where(q => q.AssignedToUserId == userId.Value || (q.FarmerId != 0 && assignedFarmerIds.Contains(q.FarmerId)))
                .OrderByDescending(q => q.CreatedDate)
                .ToList();

            ViewBag.AssignedSupportQueries = assignedSupportQueries;

            return View(reports);
        }

        // POST: /FieldOfficer/ScheduleSupportQueryVisit
        [HttpPost]
        
        public IActionResult ScheduleSupportQueryVisit(int queryId, DateTime visitDate)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farmer)
                .FirstOrDefault(q => q.QueryId == queryId);

            if (query == null)
            {
                TempData["ErrorMessage"] = "Support query not found.";
                return RedirectToAction("IncidentReports");
            }

            var officer = _context.Users.Find(userId.Value);
            string officerName = officer?.FullName ?? "Field Officer";

            query.VisitDate = visitDate;
            query.OfficerName = officerName;
            query.Status = "Field Visit Scheduled";

            // Create linked FieldVisit entry so it appears in Field Visits schedules
            var visit = new FieldVisit
            {
                FarmerId = query.FarmerId,
                PlotId = query.PlotId,
                AssignedOfficerId = userId.Value,
                VisitDate = visitDate,
                VisitTime = visitDate.ToString("hh:mm tt"),
                Status = "Scheduled",
                Priority = query.Priority,
                Notes = $"Visit for Support Ticket #{query.QueryId}: {query.Title}",
                CreatedDate = DateTime.Now
            };
            _context.FieldVisits.Add(visit);
            _context.SaveChanges();

            // Notify farmer of scheduled visit
            if (query.Farmer != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = query.Farmer.UserId,
                    Title = "Field Inspection Visit Scheduled",
                    Message = $"Field Officer {officerName} has scheduled a farm visit on {visitDate:dd-MM-yyyy HH:mm} for your ticket: \"{query.Title}\". Please be available.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = $"Field visit scheduled for {visitDate:dd-MM-yyyy HH:mm}. {query.Farmer?.FullName} has been notified.";
            return RedirectToAction("IncidentReports");
        }

        // POST: /FieldOfficer/ResolveSupportQuery
        [HttpPost]
        
        public IActionResult ResolveSupportQuery(int queryId, string fieldObservation, string actionTaken)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farmer)
                .FirstOrDefault(q => q.QueryId == queryId);

            if (query != null)
            {
                var officer = _context.Users.Find(userId.Value);
                query.OfficerName = officer?.FullName ?? "Field Officer";
                query.FieldObservation = fieldObservation?.Trim();
                query.ActionTaken = actionTaken?.Trim();
                query.Status = "Resolved";
                query.ResolutionDate = DateTime.Now;

                // Mark linked FieldVisit as Completed
                var linkedVisit = _context.FieldVisits
                    .Where(v => v.FarmerId == query.FarmerId && v.AssignedOfficerId == userId.Value && v.Status != "Completed")
                    .OrderByDescending(v => v.CreatedDate)
                    .FirstOrDefault();

                if (linkedVisit != null)
                {
                    linkedVisit.Status = "Completed";
                    linkedVisit.CompletedDate = DateTime.Now;
                    linkedVisit.Notes = (linkedVisit.Notes ?? "") + $"\n\nField Observation: {fieldObservation}\nAction Taken: {actionTaken}";
                }

                _context.SaveChanges();

                // Notify farmer
                if (query.Farmer != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = query.Farmer.UserId,
                        Title = "Support Query Resolved",
                        Message = $"Field Officer {query.OfficerName} has inspected your farm and resolved ticket: \"{query.Title}\".",
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = $"Support ticket #{queryId} resolved successfully.";
            }

            return RedirectToAction("IncidentReports");
        }

        // ---------------------------------------------------------------
        // GET: /FieldOfficer/IncidentDetails/{id}   (PestCase details + schedule + resolve)
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult IncidentDetails(int id)
        {
            if (GetCurrentUserId() == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(lp => lp.Farm)
                            .ThenInclude(f => f.Farmer)
                                .ThenInclude(fa => fa.User)
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            // Find existing scheduled visit for this case if any
            var farmer = pestCase.CropCycle?.LandPlot?.Farm?.Farmer;
            FieldVisit? existingVisit = null;
            if (farmer != null)
            {
                existingVisit = _context.FieldVisits
                    .Include(v => v.LandPlot)
                    .Where(v => v.FarmerId == farmer.FarmerId && v.AssignedOfficerId == GetCurrentUserId())
                    .OrderByDescending(v => v.CreatedDate)
                    .FirstOrDefault();
            }

            ViewBag.ExistingVisit = existingVisit;
            return View(pestCase);
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/ScheduleVisitFromIncident
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult ScheduleVisitFromIncident(int pestCaseId, DateTime visitDate)
        {
            var officerId = GetCurrentUserId();
            if (officerId == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(lp => lp.Farm)
                            .ThenInclude(f => f.Farmer)
                                .ThenInclude(fa => fa.User)
                .FirstOrDefault(p => p.PestCaseId == pestCaseId);

            if (pestCase == null)
            {
                TempData["ErrorMessage"] = "Incident not found.";
                return RedirectToAction("IncidentReports");
            }

            var farmer = pestCase.CropCycle?.LandPlot?.Farm?.Farmer;
            if (farmer == null)
            {
                TempData["ErrorMessage"] = "Farmer not found for this incident.";
                return RedirectToAction("IncidentDetails", new { id = pestCaseId });
            }

            // Create a new FieldVisit linked to this incident
            var visit = new FieldVisit
            {
                FarmerId    = farmer.FarmerId,
                PlotId      = pestCase.CropCycle?.LandPlot?.PlotId,
                AssignedOfficerId = officerId.Value,
                VisitDate   = visitDate,
                Status      = "Scheduled",
                Priority    = pestCase.Priority,
                Notes       = $"Field visit for Incident #{pestCaseId}: {pestCase.Title}",
                CreatedDate = DateTime.Now
            };

            _context.FieldVisits.Add(visit);

            // Update PestCase status to Field Visit Scheduled
            pestCase.Status = "Field Visit Scheduled";
            pestCase.FieldVisitRequested = true;

            // Notify the farmer of the scheduled visit date
            var officer = _context.Users.Find(officerId.Value);
            string officerName = officer?.FullName ?? "Field Officer";

            _context.Notifications.Add(new Notification
            {
                UserId      = farmer.UserId,
                Title       = "Field Inspection Scheduled",
                Message     = $"Field Officer {officerName} will visit your farm on {visitDate:dd-MM-yyyy HH:mm} regarding the reported issue: \"{pestCase.Title}\". Please be available.",
                IsRead      = false,
                CreatedDate = DateTime.Now
            });

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Field visit scheduled for {visitDate:dd-MM-yyyy HH:mm}. {farmer.FullName} has been notified.";
            return RedirectToAction("IncidentDetails", new { id = pestCaseId });
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/ResolveIncident
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult ResolveIncident(int pestCaseId, string fieldReport)
        {
            var officerId = GetCurrentUserId();
            if (officerId == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(lp => lp.Farm)
                            .ThenInclude(f => f.Farmer)
                                .ThenInclude(fa => fa.User)
                .FirstOrDefault(p => p.PestCaseId == pestCaseId);

            if (pestCase == null)
            {
                TempData["ErrorMessage"] = "Incident not found.";
                return RedirectToAction("IncidentReports");
            }

            var farmer = pestCase.CropCycle?.LandPlot?.Farm?.Farmer;
            if (farmer == null)
            {
                TempData["ErrorMessage"] = "Farmer not found for this incident.";
                return RedirectToAction("IncidentDetails", new { id = pestCaseId });
            }

            // Save officer's field report — status is now "Pending Farmer Approval"
            pestCase.FieldReport = fieldReport?.Trim();
            pestCase.Status = "Pending Farmer Approval";
            pestCase.FieldVisitCompletedDate = DateTime.Now;

            // Mark any linked field visit as Completed
            var linkedVisit = _context.FieldVisits
                .Where(v => v.FarmerId == farmer.FarmerId && v.AssignedOfficerId == officerId.Value
                         && v.Status != "Completed")
                .OrderByDescending(v => v.CreatedDate)
                .FirstOrDefault();

            if (linkedVisit != null)
            {
                linkedVisit.Status = "Completed";
                linkedVisit.CompletedDate = DateTime.Now;
                linkedVisit.Notes = (linkedVisit.Notes ?? "") + "\n\nOfficer Report: " + fieldReport;
            }

            // Notify the farmer to review and approve the field report
            var officer = _context.Users.Find(officerId.Value);
            string officerName = officer?.FullName ?? "Field Officer";

            _context.Notifications.Add(new Notification
            {
                UserId      = farmer.UserId,
                Title       = "Field Visit Completed — Your Approval Required",
                Message     = $"Field Officer {officerName} has completed the field inspection for \"{pestCase.Title}\" and submitted a report. Please review it in Pest Reports and Approve or Reject the resolution.",
                IsRead      = false,
                CreatedDate = DateTime.Now
            });

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Field report submitted. {farmer.FullName} has been notified to review and approve.";
            return RedirectToAction("IncidentReports");
        }



        // ---------------------------------------------------------------
        // GET: /FieldOfficer/MyProfile
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult MyProfile()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData();

            var officer = _context.FieldOfficers
                .Include(o => o.User)
                .FirstOrDefault(o => o.UserId == userId);

            return View(officer);
        }

        // ---------------------------------------------------------------
        // POST: /FieldOfficer/ScheduleVisit
        // ---------------------------------------------------------------
        [HttpPost]
        
        public IActionResult ScheduleVisit(int visitId, DateTime visitDate)
        {
            var officerId = GetCurrentUserId();
            if (officerId == null) return RedirectToAction("Login", "Auth");

            var visit = _context.FieldVisits
                .Include(v => v.Farmer)
                .Include(v => v.LandPlot)
                .FirstOrDefault(v => v.VisitId == visitId && v.AssignedOfficerId == officerId.Value);

            if (visit == null)
            {
                TempData["ErrorMessage"] = "Field visit not found.";
                return RedirectToAction("FieldVisits");
            }

            visit.VisitDate = visitDate;
            visit.Status = "Scheduled";
            _context.SaveChanges();

            // Fetch Field Officer's name
            var officer = _context.Users.Find(officerId.Value);
            string officerName = officer?.FullName ?? "Field Officer";

            // Create notification for the farmer
            var notification = new Notification
            {
                UserId = visit.Farmer.UserId, // Notification links to the user ID of the farmer
                Title = "Field Inspection Scheduled",
                Message = $"Field Officer {officerName} has scheduled a field inspection visit for your plot {(visit.LandPlot != null ? (visit.LandPlot.PlotCode ?? visit.LandPlot.PlotName) : "N/A")} on {visitDate.ToString("dd-MM-yyyy HH:mm")}.",
                IsRead = false,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Field inspection visit scheduled for {visitDate.ToString("dd-MM-yyyy HH:mm")}. Farmer has been notified.";
            return RedirectToAction("FieldVisitDetails", new { id = visitId });
        }
    }
}