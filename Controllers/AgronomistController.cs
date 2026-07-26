using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;
using SmartFarmMVC.Models.ViewModels;

namespace smart_farm_and_crop_yeild_management_system.Controllers
{
    public class AgronomistController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public AgronomistController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // Helper: Check authentication and return current Agronomist entity
        private Agronomist? GetCurrentAgronomist()
        {
            string? username = HttpContext.Session.GetString("UserUsername");
            if (string.IsNullOrEmpty(username)) return null;

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null) return null;

            return _context.Agronomists.FirstOrDefault(a => a.UserId == user.UserId);
        }

        // Helper: Set common view data for layout headers
        private void SetLayoutViewData(Agronomist agronomist)
        {
            ViewData["UserRole"] = "Agronomist";
            ViewData["UserName"] = agronomist.FullName;
            ViewData["UserInitials"] = string.Join("", agronomist.FullName.Split(' ').Select(s => s[0])).ToUpper();
            ViewData["RoleColor"] = "#0891b2"; // Teal color for Agronomist
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/Dashboard
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Dashboard()
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId == agronomist.User.UserId)
                .Select(a => a.FarmerId)
                .ToList();

            // 1. Fetch metrics
            int activeIssuesCount = _context.SupportQueries.Count(q => (q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId)) && q.Status != "Resolved")
                + _context.PestCases.Count(p => (p.AssignedOfficerId == agronomist.User.UserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId))) && !p.IsClosed && p.Status != "Resolved");

            int resolvedCount = _context.SupportQueries.Count(q => (q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId)) && q.Status == "Resolved")
                + _context.PestCases.Count(p => (p.AssignedOfficerId == agronomist.User.UserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId))) && (p.IsClosed || p.Status == "Resolved"));

            int highPriorityCount = _context.SupportQueries.Count(q => (q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId)) && q.Status != "Resolved" && q.Priority == "High")
                + _context.PestCases.Count(p => (p.AssignedOfficerId == agronomist.User.UserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId))) && !p.IsClosed && p.Status != "Resolved" && p.Priority == "High");

            int totalAssignedCount = _context.SupportQueries.Count(q => q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId))
                + _context.PestCases.Count(p => p.AssignedOfficerId == agronomist.User.UserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId)));

            // 2. Query assigned Farmers
            var assignedFarmers = _context.Assignments
                .Include(a => a.Farmer)
                .Include(a => a.Farm)
                    .ThenInclude(f => f.LandPlots)
                        .ThenInclude(lp => lp.CropCycles)
                            .ThenInclude(cc => cc.Crop)
                .Where(a => a.OfficerId == agronomist.User.UserId)
                .ToList();

            var farmerDtos = assignedFarmers
                .GroupBy(a => a.FarmerId)
                .Select(g => {
                    var a = g.First();
                    var cropsList = a.Farm.LandPlots
                        .SelectMany(lp => lp.CropCycles)
                        .Select(cc => cc.Crop.CropName)
                        .Distinct()
                        .ToList();
                    
                    return new AssignedFarmerDto
                    {
                        FarmerId = a.FarmerId,
                        FarmerName = a.Farmer.FullName,
                        PhoneNumber = a.Farmer.MobileNumber,
                        FarmName = a.Farm.FarmName,
                        Location = (!string.IsNullOrEmpty(a.Farm.Village) ? a.Farm.Village : "N/A") + ", " + (!string.IsNullOrEmpty(a.Farm.District) ? a.Farm.District : "N/A"),
                        PrimaryCrops = cropsList.Any() ? string.Join(", ", cropsList) : "None logged"
                    };
                }).ToList();

            // 3. Query assigned Issues (Support queries and Pest cases)
            var supportQueries = _context.SupportQueries
                .Include(q => q.Farmer)
                .Where(q => (q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId)) && q.Status != "Resolved")
                .ToList();

            var pestCases = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                .Where(p => (p.AssignedOfficerId == agronomist.User.UserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId))) && !p.IsClosed && p.Status != "Resolved")
                .ToList();

            var issueDtos = new List<AssignedIssueDto>();

            foreach (var q in supportQueries)
            {
                issueDtos.Add(new AssignedIssueDto
                {
                    IssueId = q.QueryId,
                    Title = q.Title,
                    Description = q.Description,
                    Priority = q.Priority,
                    Status = q.Status,
                    IssueType = "Support Query",
                    CreatedDate = q.CreatedDate,
                    FarmerName = q.Farmer.FullName
                });
            }

            foreach (var p in pestCases)
            {
                issueDtos.Add(new AssignedIssueDto
                {
                    IssueId = p.PestCaseId,
                    Title = p.Title,
                    Description = p.Description,
                    Priority = p.Priority,
                    Status = p.Status,
                    IssueType = "Pest Case",
                    CreatedDate = p.CreatedDate,
                    FarmerName = p.CropCycle?.LandPlot?.Farm?.Farmer?.FullName ?? "Unknown Farmer"
                });
            }

            issueDtos = issueDtos.OrderByDescending(i => i.CreatedDate).ToList();

            // 4. Populate view model
            var model = new AgronomistDashboardViewModel
            {
                ActiveIssuesCount = activeIssuesCount,
                HighPriorityCount = highPriorityCount,
                ResolvedCount = resolvedCount,
                TotalAssignedCount = totalAssignedCount,
                AssignedFarmers = farmerDtos,
                AssignedIssues = issueDtos
            };

            ViewData["Title"] = "Agronomist Dashboard";
            ViewData["Subtitle"] = "Review assigned farmer issues and provide expert crop advisories.";

            return View(model);
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/AssignedIssues
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult AssignedIssues(string status = "Assigned")
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId == agronomist.User.UserId)
                .Select(a => a.FarmerId)
                .ToList();

            var issues = _context.SupportQueries
                .Include(q => q.Farmer)
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .Where(q => q.AssignedToUserId == agronomist.User.UserId || assignedFarmerIds.Contains(q.FarmerId))
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Assigned")
                {
                    // Farmer module assigns crop issues to the agronomist with status "Under Review" or "Pending"
                    issues = issues.Where(q => q.Status == "Assigned" || q.Status == "Under Review" || q.Status == "Pending");
                }
                else
                {
                    issues = issues.Where(q => q.Status == status);
                }
            }

            ViewBag.SelectedStatus = status;
            ViewData["Title"] = "Assigned Farmer Issues";
            ViewData["Subtitle"] = $"Inspect and prescribe recommendations for active support queries.";

            // Also surface pest cases assigned to this agronomist or assigned farmers.
            var pestCasesQuery = _context.PestCases
                .Include(p => p.CropCycle).ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle).ThenInclude(c => c.LandPlot).ThenInclude(lp => lp.Farm).ThenInclude(f => f.Farmer)
                .Where(p => p.AssignedOfficerId == agronomist.User.UserId || 
                           (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId)));

            if (status == "Resolved")
            {
                pestCasesQuery = pestCasesQuery.Where(p => p.IsClosed || p.Status == "Resolved");
            }
            else
            {
                pestCasesQuery = pestCasesQuery.Where(p => !p.IsClosed && p.Status != "Resolved");
            }

            ViewBag.PestCases = pestCasesQuery.OrderByDescending(p => p.CreatedDate).ToList();

            return View(issues.OrderByDescending(q => q.CreatedDate).ToList());
        }

        // ---------------------------------------------------------------
        // POST: /Agronomist/SubmitIssueRecommendation
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitIssueRecommendation(int queryId, string recommendation)
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farmer)
                .FirstOrDefault(q => q.QueryId == queryId);
            if (query == null) return NotFound();

            query.AgronomistRecommendation = recommendation;
            query.RecommendationDate = DateTime.Now;
            query.Status = "Resolved"; // Mark query as resolved once agronomist inputs prescription
            _context.SaveChanges();

            // Add notification for the Farmer
            var notification = new Notification
            {
                UserId = query.Farmer.UserId,
                Title = "Prescription Provided for Support Case",
                Message = $"Agronomist {agronomist.FullName} has posted an advisory on your ticket: '{query.Title}'. Advisories: {recommendation}.",
                IsRead = false,
                CreatedDate = DateTime.Now
            };
            _context.Notifications.Add(notification);

            // Notify the Cooperative Manager that the assigned issue has been resolved
            var coopManagerUser = _context.Users.FirstOrDefault(u => u.RoleId == 6 && u.IsActive); // Cooperative Manager
            if (coopManagerUser != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = coopManagerUser.UserId,
                    Title = "Farmer Issue Resolved by Agronomist",
                    Message = $"Agronomist {agronomist.FullName} has resolved the assigned issue '{query.Title}'. Recommendation: {recommendation}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Recommendation submitted successfully. Ticket resolved.";
            return RedirectToAction("AssignedIssues", new { status = "Resolved" });
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/FarmAnalysis
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult FarmAnalysis(string type = "Soil")
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            // Fetch assigned farmer IDs
            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId > 0)
                .Select(a => a.FarmerId)
                .Distinct()
                .ToList();

            // Query sensor readings for assigned member farmers only
            var sensorReadingsQuery = _context.SensorReadings
                .Include(r => r.Plot)
                    .ThenInclude(p => p.Farm)
                        .ThenInclude(f => f.Farmer)
                .AsQueryable();

            if (assignedFarmerIds.Any())
            {
                sensorReadingsQuery = sensorReadingsQuery.Where(r => assignedFarmerIds.Contains(r.Plot.Farm.FarmerId));
            }

            var sensorReadings = sensorReadingsQuery.ToList();

            ViewBag.AnalysisType = type;
            ViewData["Title"] = $"{type} Analysis Console";
            ViewData["Subtitle"] = $"Aggregate IoT metrics and field logs to evaluate environmental factors for assigned member farms.";

            return View(sensorReadings);
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/Analytics
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Analytics(string tab = "crop")
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            ViewBag.ActiveTab = tab;
            ViewData["Title"] = "Analytics & Trends Console";
            ViewData["Subtitle"] = "Review seasonal yield metrics and soil quality indices.";

            return View();
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/Profile
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Profile()
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            var viewModel = new FarmerProfileViewModel
            {
                FullName = agronomist.FullName,
                MobileNumber = agronomist.MobileNumber
            };

            // Populate ViewBag for additional fields
            ViewBag.Specialization = agronomist.Specialization;

            ViewData["Title"] = "My Profile";
            ViewData["Subtitle"] = "Update your expert profile, specialization details, and contact information.";

            return View(viewModel);
        }

        // ---------------------------------------------------------------
        // POST: /Agronomist/Profile
        // ---------------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(FarmerProfileViewModel model)
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.MobileNumber))
            {
                TempData["ErrorMessage"] = "Full Name and Mobile Number are required.";
                return RedirectToAction("Profile");
            }

            agronomist.FullName = model.FullName;
            agronomist.MobileNumber = model.MobileNumber;
            if (!string.IsNullOrEmpty(Request.Form["Specialization"]))
            {
                agronomist.Specialization = Request.Form["Specialization"]!;
            }

            // Update user details if password is changed
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == agronomist.UserId);
                if (user != null)
                {
                    user.PasswordHash = model.NewPassword;
                }
            }

            _context.SaveChanges();
            HttpContext.Session.SetString("UserName", agronomist.FullName);

            TempData["SuccessMessage"] = "Profile details updated successfully.";
            return RedirectToAction("Profile");
        }

        // ---------------------------------------------------------------
        // GET: /Agronomist/ReferenceLibrary
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult ReferenceLibrary()
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            ViewBag.PageTitle = "Reference Library";
            return View();
        }
    }
}