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

            int agronomistUserId = agronomist.User.UserId;

            // Step 1: Get list of assigned farmer IDs for this agronomist
            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId == agronomistUserId)
                .Select(a => a.FarmerId)
                .ToList();

            // Step 2: Fetch support queries and pest cases assigned to this agronomist
            var allSupportQueries = _context.SupportQueries
                .Where(q => q.AssignedToUserId == agronomistUserId || assignedFarmerIds.Contains(q.FarmerId))
                .ToList();

            var allPestCases = _context.PestCases
                .Include(p => p.CropCycle).ThenInclude(c => c.LandPlot).ThenInclude(plot => plot.Farm)
                .Where(p => p.AssignedOfficerId == agronomistUserId || 
                            (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId)))
                .ToList();

            // Step 3: Calculate metric counts step-by-step
            int activeSupport = allSupportQueries.Count(q => q.Status != "Resolved");
            int activePests = allPestCases.Count(p => !p.IsClosed && p.Status != "Resolved");
            int activeIssuesCount = activeSupport + activePests;

            int resolvedSupport = allSupportQueries.Count(q => q.Status == "Resolved");
            int resolvedPests = allPestCases.Count(p => p.IsClosed || p.Status == "Resolved");
            int resolvedCount = resolvedSupport + resolvedPests;

            int highPrioritySupport = allSupportQueries.Count(q => q.Status != "Resolved" && q.Priority == "High");
            int highPriorityPests = allPestCases.Count(p => !p.IsClosed && p.Status != "Resolved" && p.Priority == "High");
            int highPriorityCount = highPrioritySupport + highPriorityPests;

            int totalAssignedCount = allSupportQueries.Count + allPestCases.Count;

            // Step 4: Build assigned farmer list using a simple foreach loop
            var assignments = _context.Assignments
                .Include(a => a.Farmer)
                .Include(a => a.Farm)
                    .ThenInclude(f => f.LandPlots)
                        .ThenInclude(lp => lp.CropCycles)
                            .ThenInclude(cc => cc.Crop)
                .Where(a => a.OfficerId == agronomistUserId)
                .ToList();

            var farmerDtos = new List<AssignedFarmerDto>();
            var processedFarmerIds = new HashSet<int>();

            foreach (var a in assignments)
            {
                if (a.Farmer == null || processedFarmerIds.Contains(a.FarmerId)) continue;
                processedFarmerIds.Add(a.FarmerId);

                var cropsList = new List<string>();
                if (a.Farm != null && a.Farm.LandPlots != null)
                {
                    foreach (var lp in a.Farm.LandPlots)
                    {
                        if (lp.CropCycles == null) continue;
                        foreach (var cc in lp.CropCycles)
                        {
                            if (cc.Crop != null && !cropsList.Contains(cc.Crop.CropName))
                            {
                                cropsList.Add(cc.Crop.CropName);
                            }
                        }
                    }
                }

                var dto = new AssignedFarmerDto
                {
                    FarmerId = a.FarmerId,
                    FarmerName = a.Farmer.FullName,
                    PhoneNumber = a.Farmer.MobileNumber,
                    FarmName = a.Farm != null ? a.Farm.FarmName : "Farm Plot",
                    Location = (!string.IsNullOrEmpty(a.Farm?.Village) ? a.Farm.Village : "N/A") + ", " + (!string.IsNullOrEmpty(a.Farm?.District) ? a.Farm.District : "N/A"),
                    PrimaryCrops = cropsList.Any() ? string.Join(", ", cropsList) : "None logged"
                };

                farmerDtos.Add(dto);
            }

            // Step 5: Build active issues list using simple loops
            var supportQueries = _context.SupportQueries
                .Include(q => q.Farmer)
                .Where(q => (q.AssignedToUserId == agronomistUserId || assignedFarmerIds.Contains(q.FarmerId)) && q.Status != "Resolved")
                .ToList();

            var pestCases = _context.PestCases
                .Include(p => p.CropCycle).ThenInclude(c => c.LandPlot).ThenInclude(plot => plot.Farm).ThenInclude(farm => farm.Farmer)
                .Where(p => (p.AssignedOfficerId == agronomistUserId || (p.CropCycle != null && p.CropCycle.LandPlot != null && p.CropCycle.LandPlot.Farm != null && assignedFarmerIds.Contains(p.CropCycle.LandPlot.Farm.FarmerId))) && !p.IsClosed && p.Status != "Resolved")
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
                    FarmerName = q.Farmer != null ? q.Farmer.FullName : "Farmer"
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

            int agronomistUserId = agronomist.User.UserId;

            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId == agronomistUserId)
                .Select(a => a.FarmerId)
                .ToList();

            var issuesQuery = _context.SupportQueries
                .Include(q => q.Farmer)
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .Where(q => q.AssignedToUserId == agronomistUserId || assignedFarmerIds.Contains(q.FarmerId));

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Assigned")
                {
                    issuesQuery = issuesQuery.Where(q => q.Status == "Assigned" || q.Status == "Under Review" || q.Status == "Pending");
                }
                else
                {
                    issuesQuery = issuesQuery.Where(q => q.Status == status);
                }
            }

            var issues = issuesQuery.OrderByDescending(q => q.CreatedDate).ToList();

            ViewBag.SelectedStatus = status;
            ViewData["Title"] = "Assigned Farmer Issues";
            ViewData["Subtitle"] = $"Inspect and prescribe recommendations for active support queries.";

            var pestCasesQuery = _context.PestCases
                .Include(p => p.CropCycle).ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle).ThenInclude(c => c.LandPlot).ThenInclude(lp => lp.Farm).ThenInclude(f => f.Farmer)
                .Where(p => p.AssignedOfficerId == agronomistUserId || 
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

            return View(issues);
        }

        // ---------------------------------------------------------------
        // POST: /Agronomist/SubmitIssueRecommendation
        // ---------------------------------------------------------------
        [HttpPost]
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
            query.Status = "Resolved";
            _context.SaveChanges();

            if (query.Farmer != null)
            {
                var notification = new Notification
                {
                    UserId = query.Farmer.UserId,
                    Title = "Prescription Provided for Support Case",
                    Message = $"Agronomist {agronomist.FullName} has posted an advisory on your ticket: '{query.Title}'. Advisories: {recommendation}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            var coopManagerUser = _context.Users.FirstOrDefault(u => u.RoleId == 6 && u.IsActive);
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

            var assignedFarmerIds = _context.Assignments
                .Where(a => a.OfficerId == agronomist.User.UserId)
                .Select(a => a.FarmerId)
                .Distinct()
                .ToList();

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
        public async Task<IActionResult> Analytics(string tab = "crop")
        {
            var agronomist = GetCurrentAgronomist();
            if (agronomist == null) return RedirectToAction("Login", "Auth");
            SetLayoutViewData(agronomist);

            ViewBag.ActiveTab = tab;
            ViewData["Title"] = "Analytics & Trends Console";
            ViewData["Subtitle"] = "Review seasonal yield metrics and soil quality indices.";

            // 1. Live Harvest Yield Data
            var harvests = await _context.Harvests
                .Include(h => h.CropCycle).ThenInclude(c => c.Crop)
                .ToListAsync();

            var cropYieldMap = new Dictionary<string, double>();
            foreach (var h in harvests)
            {
                string cropName = h.CropCycle?.Crop?.CropName ?? "General Crop";
                if (!cropYieldMap.ContainsKey(cropName))
                {
                    cropYieldMap[cropName] = 0;
                }
                cropYieldMap[cropName] += (double)h.ActualQuantity;
            }

            if (!cropYieldMap.Any())
            {
                var activeCrops = await _context.CropCycles.Include(c => c.Crop).Select(c => c.Crop.CropName).Distinct().ToListAsync();
                ViewBag.CropLabels = System.Text.Json.JsonSerializer.Serialize(activeCrops.Any() ? activeCrops : new List<string> { "Cotton", "Sugarcane", "Onion" });
                ViewBag.CropData = System.Text.Json.JsonSerializer.Serialize(activeCrops.Select(_ => 0.0).ToList());
            }
            else
            {
                ViewBag.CropLabels = System.Text.Json.JsonSerializer.Serialize(cropYieldMap.Keys.ToList());
                ViewBag.CropData = System.Text.Json.JsonSerializer.Serialize(cropYieldYieldMapValues(cropYieldMap));
            }

            // 2. Live Soil & Moisture Trends Data
            var landPlots = await _context.LandPlots.ToListAsync();
            var plotNames = new List<string>();
            var plotMoisture = new List<double>();

            foreach (var p in landPlots)
            {
                plotNames.Add(p.PlotName);
                double m = p.SoilType == "Black Soil" ? 55.0 : (p.SoilType == "Red Soil" ? 42.0 : 48.0);
                plotMoisture.Add(m);
            }

            ViewBag.PlotLabels = System.Text.Json.JsonSerializer.Serialize(plotNames.Any() ? plotNames : new List<string> { "Plot A", "Plot B", "Plot C" });
            ViewBag.MoistureData = System.Text.Json.JsonSerializer.Serialize(plotMoisture.Any() ? plotMoisture : new List<double> { 52.0, 48.0, 44.0 });

            // 3. Live Pest Case Distribution Data
            var pestCases = await _context.PestCases.ToListAsync();
            var pestMap = new Dictionary<string, int>();

            foreach (var p in pestCases)
            {
                string pestName = string.IsNullOrEmpty(p.Title) ? "General Crop Issue" : p.Title;
                if (!pestMap.ContainsKey(pestName))
                {
                    pestMap[pestName] = 0;
                }
                pestMap[pestName]++;
            }

            ViewBag.PestLabels = System.Text.Json.JsonSerializer.Serialize(pestMap.Keys.ToList());
            ViewBag.PestCounts = System.Text.Json.JsonSerializer.Serialize(pestMap.Values.ToList());

            return View();
        }

        private List<double> cropYieldYieldMapValues(Dictionary<string, double> map)
        {
            return map.Values.ToList();
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