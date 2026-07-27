using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class CooperativeManagerController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public CooperativeManagerController(SmartFarmDbContext context)
        {
            _context = context;
        }

        private bool IsCooperativeManager()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "Cooperative Manager" || role == "Admin";
        }

        private int? GetCurrentUserId()
        {
            var userIdVal = HttpContext.Session.GetInt32("UserId");
            if (userIdVal != null) return userIdVal;
            var username = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(username)) return null;
            var user = _context.Users.FirstOrDefault(u => u.Username == username || u.Email == username);
            return user?.UserId;
        }

        // GET: /CooperativeManager/Dashboard
        public IActionResult Dashboard()
        {
            if (!IsCooperativeManager())
            {
                return RedirectToAction("Login", "Auth");
            }

            var currentUserId = GetCurrentUserId();
            var manager = _context.CooperativeManagers.FirstOrDefault(cm => cm.UserId == currentUserId);
            string managerFullName = manager?.FullName ?? HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            string managerRegion = manager?.Region ?? "Wardha Region";

            ViewData["Title"] = "Cooperative Manager Dashboard";
            ViewData["Subtitle"] = "Cooperative Operations — Live data from SQL Server";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = managerFullName;
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";
            ViewData["RoleColor"] = "#7c3aed";

            // Live SQL Calculations
            int totalFarmers = _context.Farmers.Count();
            int pendingFarmerIssues = _context.SupportQueries.Count(q => q.Status != "Resolved");
            int activeAssignments = _context.Assignments.Count(a => a.Status == "Pending" || a.Status == "Active" || a.Status == "In Progress");
            int resolvedCases = _context.SupportQueries.Count(s => s.Status == "Resolved") + _context.PestCases.Count(p => p.Status == "Resolved" || p.IsClosed);
            int pendingFieldVisits = _context.FieldVisits.Count(v => v.Status == "Pending" || v.Status == "Scheduled" || v.Status == "InProgress");
            int agronomistReviewsPending = _context.PestCases.Count(p => p.Status == "Pending" || p.Status == "Under Review" || p.Status == "Assigned");
            int totalPestCases = _context.PestCases.Count();
            int activeImprovementPlans = _context.SupportQueries.Count(s => s.ImprovementStatus == "In Progress");

            // Dynamic Member Farm Performance Calculation
            var farmersList = _context.Farmers
                .Include(f => f.Farms)
                    .ThenInclude(farm => farm.LandPlots)
                        .ThenInclude(plot => plot.CropCycles)
                            .ThenInclude(cycle => cycle.Crop)
                .Include(f => f.Farms)
                    .ThenInclude(farm => farm.LandPlots)
                        .ThenInclude(plot => plot.CropCycles)
                            .ThenInclude(cycle => cycle.Harvests)
                .ToList();

            // Pre-fetch sales by farmer
            var farmerSalesMap = _context.CropOrders
                .Where(o => o.Status == "Delivered" || o.Status == "PAID_ESCROW" || o.Status == "Paid" || o.Status == "Farmer Accepted")
                .GroupBy(o => o.FarmerId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));

            // Pre-fetch open issues by farmer
            var farmerOpenIssuesMap = _context.SupportQueries
                .Where(q => q.Status != "Resolved")
                .GroupBy(q => q.FarmerId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build staff lookup dictionary across Agronomists, FieldOfficers, and Users
            var agronomistsList = _context.Agronomists.ToList();
            var fieldOfficersList = _context.FieldOfficers.ToList();

            var staffNameByUserId = new Dictionary<int, string>();
            foreach (var a in agronomistsList)
            {
                staffNameByUserId[a.UserId] = a.FullName;
            }
            foreach (var o in fieldOfficersList)
            {
                staffNameByUserId[o.UserId] = o.FullName;
            }

            var allAssignments = _context.Assignments.ToList();
            var allFoAssignments = _context.FieldOfficerAssignments.Include(a => a.FieldOfficer).ToList();

            var officerAssignmentsMap = new Dictionary<int, string>();

            foreach (var f in farmersList)
            {
                var staffNames = new List<string>();

                // 1. Check Assignments table (set by Cooperative Manager / Admin)
                var farmerAssigns = allAssignments.Where(a => a.FarmerId == f.FarmerId).ToList();
                foreach (var a in farmerAssigns)
                {
                    if (staffNameByUserId.TryGetValue(a.OfficerId, out string sName))
                    {
                        if (!staffNames.Contains(sName)) staffNames.Add(sName);
                    }
                    else
                    {
                        var user = _context.Users.FirstOrDefault(u => u.UserId == a.OfficerId);
                        if (user != null)
                        {
                            var uName = !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username;
                            if (!staffNames.Contains(uName)) staffNames.Add(uName);
                        }
                    }
                }

                // 2. Check FieldOfficerAssignments table
                var farmerFoAssigns = allFoAssignments.Where(a => a.FarmerId == f.FarmerId).ToList();
                foreach (var fa in farmerFoAssigns)
                {
                    if (fa.FieldOfficer != null && !staffNames.Contains(fa.FieldOfficer.FullName))
                    {
                        staffNames.Add(fa.FieldOfficer.FullName);
                    }
                }

                if (staffNames.Any())
                {
                    officerAssignmentsMap[f.FarmerId] = string.Join(", ", staffNames);
                }
            }

            var memberFarmPerformanceList = new List<MemberFarmPerformanceItem>();

            foreach (var f in farmersList)
            {
                var allPlots = new List<LandPlot>();
                foreach (var farm in f.Farms)
                {
                    if (farm.LandPlots != null)
                    {
                        allPlots.AddRange(farm.LandPlots);
                    }
                }

                var allCycles = new List<CropCycle>();
                foreach (var p in allPlots)
                {
                    if (p.CropCycles != null)
                    {
                        allCycles.AddRange(p.CropCycles);
                    }
                }

                var activeCropsList = new List<string>();
                string lastCropName = "General Crops";

                foreach (var c in allCycles)
                {
                    if (c.Crop != null)
                    {
                        lastCropName = c.Crop.CropName;
                        if (c.Status != "Harvested" && !activeCropsList.Contains(c.Crop.CropName))
                        {
                            activeCropsList.Add(c.Crop.CropName);
                        }
                    }
                }

                string activeCropsStr = activeCropsList.Any()
                    ? string.Join(", ", activeCropsList)
                    : lastCropName;

                double totalArea = 0;
                foreach (var p in allPlots)
                {
                    totalArea += (double)p.Area;
                }

                decimal totalHarvest = 0;
                foreach (var c in allCycles)
                {
                    if (c.Harvests != null)
                    {
                        foreach (var h in c.Harvests)
                        {
                            totalHarvest += (decimal)h.ActualQuantity;
                        }
                    }
                }

                farmerSalesMap.TryGetValue(f.FarmerId, out decimal salesRev);
                farmerOpenIssuesMap.TryGetValue(f.FarmerId, out int openIssues);
                officerAssignmentsMap.TryGetValue(f.FarmerId, out string staffName);

                string produceStatus = salesRev > 0 ? "Revenue Generated" : (totalHarvest > 0 ? "Harvest Completed" : (activeCropsList.Any() ? "In Cultivation" : "Registered"));

                memberFarmPerformanceList.Add(new MemberFarmPerformanceItem
                {
                    FarmerId = f.FarmerId,
                    FarmerName = f.FullName,
                    MobileNumber = f.MobileNumber ?? "N/A",
                    Location = !string.IsNullOrEmpty(f.Village) ? $"{f.Village}, {f.District}" : (f.District ?? "Wardha Region"),
                    TotalFarms = f.Farms.Count,
                    TotalPlots = allPlots.Count,
                    TotalAreaAcres = Math.Round(totalArea, 1),
                    ActiveCrops = activeCropsStr,
                    TotalHarvestQuantity = Math.Round(totalHarvest, 1),
                    TotalSalesRevenue = salesRev,
                    OpenIssuesCount = openIssues,
                    AssignedStaffName = string.IsNullOrEmpty(staffName) ? "Unassigned" : staffName,
                    ProduceStatus = produceStatus
                });
            }

            memberFarmPerformanceList = memberFarmPerformanceList
                .OrderByDescending(p => p.TotalHarvestQuantity)
                .ThenByDescending(p => p.TotalAreaAcres)
                .ToList();

            var cultivationPlans = _context.CultivationRequests
                .Include(cr => cr.Farmer)
                .Include(cr => cr.Crop)
                .OrderByDescending(cr => cr.CreatedDate)
                .Take(10)
                .ToList();

            var viewModel = new CooperativeManagerDashboardViewModel
            {
                ManagerFullName = managerFullName,
                Region = managerRegion,
                TotalFarmers = totalFarmers,
                PendingFarmerIssues = pendingFarmerIssues,
                ActiveAssignments = activeAssignments,
                ResolvedCases = resolvedCases,
                PendingFieldVisits = pendingFieldVisits,
                AgronomistReviewsPending = agronomistReviewsPending,
                TotalPestCases = totalPestCases,
                ActiveImprovementPlans = activeImprovementPlans,
                MemberFarmPerformance = memberFarmPerformanceList,
                CultivationPlans = cultivationPlans
            };

            return View(viewModel);
        }

        // GET: /CooperativeManager/MemberFarms
        public IActionResult MemberFarms(string? searchFarmer, string? district, string? village)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Member Farms Directory";
            ViewData["Subtitle"] = "All registered member farmers in the cooperative";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var farmersQuery = _context.Farmers.AsQueryable();

            if (!string.IsNullOrEmpty(searchFarmer))
            {
                farmersQuery = farmersQuery.Where(f => f.FullName.Contains(searchFarmer) || f.MobileNumber.Contains(searchFarmer));
            }

            if (!string.IsNullOrEmpty(district) && district != "All Districts")
            {
                farmersQuery = farmersQuery.Where(f => f.District == district);
            }

            if (!string.IsNullOrEmpty(village) && village != "All Villages")
            {
                farmersQuery = farmersQuery.Where(f => f.Village == village);
            }

            var farmersList = farmersQuery.ToList();

            ViewBag.Districts = _context.Farmers.Select(f => f.District).Where(d => !string.IsNullOrEmpty(d)).Distinct().ToList();
            ViewBag.Villages = _context.Farmers.Select(f => f.Village).Where(v => !string.IsNullOrEmpty(v)).Distinct().ToList();
            
            ViewBag.Agronomists = _context.Agronomists.ToList();
            ViewBag.FieldOfficers = _context.FieldOfficers.ToList();

            // Build a combined staff lookup keyed by UserId (Assignment.OfficerId is a FK to User).
            // Used both to render the assignment dropdown and to display the assigned name on cards.
            var staffLookup = new Dictionary<int, string>();
            foreach (var a in (List<Agronomist>)ViewBag.Agronomists)
            {
                staffLookup[a.UserId] = $"{a.FullName} (Agronomist)";
            }
            foreach (var o in (List<FieldOfficer>)ViewBag.FieldOfficers)
            {
                staffLookup[o.UserId] = $"{o.FullName} (Field Officer)";
            }
            ViewBag.StaffLookup = staffLookup;

            ViewBag.Assignments = _context.Assignments.ToList();

            return View(farmersList);
        }

        // POST: /CooperativeManager/AssignStaff
        [HttpPost]
        public IActionResult AssignStaff(int farmerId, int? officerId, string? notes)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            var farmer = _context.Farmers.FirstOrDefault(f => f.FarmerId == farmerId);
            if (farmer == null)
            {
                TempData["ErrorMessage"] = "Farmer record not found.";
                return RedirectToAction("MemberFarms");
            }

            var farm = _context.Farms.FirstOrDefault(f => f.FarmerId == farmerId);
            int farmId = farm != null ? farm.FarmId : 1;

            if (officerId == null || officerId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a staff member to assign.";
                return RedirectToAction("MemberFarms");
            }

            int assignedOfficerId = officerId.Value;

            // Determine the role of the selected staff member so agronomist and field officer
            // assignments are kept as separate rows (one per role) instead of overwriting each other.
            bool isAgronomist = _context.Agronomists.Any(a => a.UserId == assignedOfficerId);
            var sameRoleOfficerIds = isAgronomist
                ? _context.Agronomists.Select(a => a.UserId).ToList()
                : _context.FieldOfficers.Select(o => o.UserId).ToList();

            var assignment = _context.Assignments
                .FirstOrDefault(a => a.FarmerId == farmerId && sameRoleOfficerIds.Contains(a.OfficerId));
            if (assignment == null)
            {
                assignment = new Assignment
                {
                    FarmerId = farmerId,
                    FarmId = farmId,
                    OfficerId = assignedOfficerId,
                    Task = notes ?? "General Farm Inspection & Guidance",
                    AssignedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(7),
                    Priority = "Medium",
                    Status = "Pending",
                    Notes = notes ?? "Assigned by Cooperative Manager"
                };
                _context.Assignments.Add(assignment);
            }
            else
            {
                assignment.OfficerId = assignedOfficerId;
                assignment.AssignedDate = DateTime.Now;
                assignment.Status = "Pending";
                if (!string.IsNullOrEmpty(notes))
                {
                    assignment.Notes = notes;
                    assignment.Task = notes;
                }
            }

            _context.SaveChanges();

            // Route the farmer's open support queries to the assigned staff member so they
            // appear in the Agronomist/Field Officer "Assigned Issues" list. The staff pages
            // read from SupportQuery.AssignedToUserId (a FK to User), and assignedOfficerId is
            // the staff member's UserId.
            var openQueries = _context.SupportQueries
                .Where(q => q.FarmerId == farmerId && q.Status != "Resolved")
                .ToList();
            foreach (var q in openQueries)
            {
                q.AssignedToUserId = assignedOfficerId;
                if (q.Status == "Pending")
                {
                    q.Status = "Assigned";
                }
            }
            if (openQueries.Count > 0)
            {
                _context.SaveChanges();
            }

            // If an Agronomist was assigned, route the farmer's open pest cases to them so the
            // cases appear in the Agronomist's "Assigned Issues" list and the farmer sees the name.
            if (isAgronomist)
            {
                var openPestCases = _context.PestCases
                    .Where(p => p.CropCycle.LandPlot.Farm.FarmerId == farmerId
                                && !p.IsClosed
                                && (p.AssignedOfficerId == null || p.Status == "Pending"))
                    .ToList();
                foreach (var pc in openPestCases)
                {
                    pc.AssignedOfficerId = assignedOfficerId;
                }
                if (openPestCases.Count > 0)
                {
                    _context.SaveChanges();
                }
            }
            if (openQueries.Count > 0)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = assignedOfficerId,
                    Title = "New Farmer Issues Assigned",
                    Message = $"You have been assigned {openQueries.Count} support issue(s) for farmer {farmer.FullName}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                _context.SaveChanges();
            }

            // Create Notification
            var notification = new Notification
            {
                UserId = farmer.UserId,
                Title = "Field Officer Assigned",
                Message = "Your farm has been assigned an officer for inspection. Check your dashboard for updates.",
                IsRead = false,
                CreatedDate = DateTime.Now
            };
            _context.Notifications.Add(notification);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Staff assigned successfully for farmer {farmer.FullName}.";
            return RedirectToAction("MemberFarms");
        }

        // GET: /CooperativeManager/Assignments
        public IActionResult Assignments(string? search)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Farm Assignments Directory";
            ViewData["Subtitle"] = "Monitor and assign field tasks to officers";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var assignmentsQuery = _context.Assignments
                .Include(a => a.Farmer)
                .Include(a => a.Farm)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                assignmentsQuery = assignmentsQuery.Where(a => a.Farmer.FullName.Contains(search) || a.Task.Contains(search));
            }

            return View(assignmentsQuery.ToList());
        }

        // GET: /CooperativeManager/SupportCases
        public IActionResult SupportCases()
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Support Cases Queue";
            ViewData["Subtitle"] = "Manage and assign farmer queries and support tickets";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var supportTickets = _context.SupportQueries
                .Include(sq => sq.Farmer)
                .Include(sq => sq.AssignedToUser)
                .OrderByDescending(sq => sq.CreatedDate)
                .ToList();

            // Fetch active Agronomists (RoleId = 4) and Field Officers (RoleId = 5)
            var staffUsers = _context.Users
                .Where(u => (u.RoleId == 4 || u.RoleId == 5) && u.IsActive)
                .OrderBy(u => u.RoleId)
                .ThenBy(u => u.FullName)
                .ToList();

            ViewBag.StaffUsers = staffUsers;

            return View(supportTickets);
        }

        // POST: /CooperativeManager/AssignSupportTicket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignSupportTicket(int queryId, int assignedUserId)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farmer)
                .FirstOrDefault(q => q.QueryId == queryId);

            if (query == null)
            {
                TempData["ErrorMessage"] = "Support ticket not found.";
                return RedirectToAction("SupportCases");
            }

            var assignedUser = _context.Users.FirstOrDefault(u => u.UserId == assignedUserId);
            if (assignedUser == null)
            {
                TempData["ErrorMessage"] = "Staff member not found.";
                return RedirectToAction("SupportCases");
            }

            query.AssignedToUserId = assignedUserId;
            query.Status = "Assigned";
            _context.SaveChanges();

            string roleTitle = assignedUser.RoleId == 4 ? "Agronomist" : "Field Officer";

            // Notify assigned staff member
            _context.Notifications.Add(new Notification
            {
                UserId = assignedUserId,
                Title = "New Ticket Assigned",
                Message = $"Cooperative Manager assigned you farmer ticket #{query.QueryId}: \"{query.Title}\" from {query.Farmer?.FullName}. Please review in your assigned tasks.",
                IsRead = false,
                CreatedDate = DateTime.Now
            });

            // Notify Farmer
            if (query.Farmer != null)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = query.Farmer.UserId,
                    Title = "Support Ticket Assigned",
                    Message = $"Your support ticket \"{query.Title}\" has been assigned to {roleTitle} {assignedUser.FullName}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Ticket #{queryId} successfully assigned to {roleTitle} {assignedUser.FullName}.";
            return RedirectToAction("SupportCases");
        }

        // GET: /CooperativeManager/PestCases
        public IActionResult PestCases()
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Pest Incidents Registry";
            ViewData["Subtitle"] = "Monitor crop pest infestations and track field inspections";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var pestCases = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(lp => lp.Farm)
                            .ThenInclude(f => f.Farmer)
                .Include(p => p.AssignedOfficer)
                .OrderByDescending(p => p.PestCaseId)
                .ToList();

            return View(pestCases);
        }

        // GET: /CooperativeManager/ImprovementPlans
        public IActionResult ImprovementPlans()
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Farm Improvement Plans";
            ViewData["Subtitle"] = "Proactive crop yield optimization plans for cooperative member farmers";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var plans = _context.CultivationRequests
                .Include(cr => cr.Farmer)
                .Include(cr => cr.Crop)
                .OrderByDescending(cr => cr.CreatedDate)
                .ToList();

            ViewBag.Farmers = _context.Farmers.ToList();
            ViewBag.Crops = _context.Crops.ToList();

            return View(plans);
        }

        // POST: /CooperativeManager/CreateImprovementPlan
        [HttpPost]
        public IActionResult CreateImprovementPlan(int farmerId, int cropId, decimal targetArea)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            var farm = _context.Farms.FirstOrDefault(f => f.FarmerId == farmerId);
            int farmId = farm != null ? farm.FarmId : 1;

            var plot = _context.LandPlots.FirstOrDefault(p => p.FarmId == farmId);
            int plotId = plot != null ? plot.PlotId : 1;

            var request = new CultivationRequest
            {
                FarmerId = farmerId,
                FarmId = farmId,
                PlotId = plotId,
                CropId = cropId,
                CultivationArea = targetArea,
                SowingDate = DateTime.Now.AddDays(10),
                SoilPH = 6.5m,
                MoistureLevel = 45.0m,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            _context.CultivationRequests.Add(request);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Improvement plan created successfully.";
            return RedirectToAction("ImprovementPlans");
        }

        // GET: /CooperativeManager/Reports
        public IActionResult Reports()
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Operational Reports";
            ViewData["Subtitle"] = "Generate printable analytics and audit logs across cooperative operations";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            ViewBag.TotalFarmers = _context.Farmers.Count();
            ViewBag.TotalHarvests = _context.Harvests.Count();
            ViewBag.TotalSales = _context.CropOrders.Where(o => o.Status == "Delivered").Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.TotalPestCases = _context.PestCases.Count();

            return View();
        }

        // GET: /CooperativeManager/MyProfile
        public IActionResult MyProfile()
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Cooperative Manager Profile";
            ViewData["Subtitle"] = "View and manage your account credentials and personal details";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var username = HttpContext.Session.GetString("UserUsername");
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            var manager = user != null ? _context.CooperativeManagers.FirstOrDefault(cm => cm.UserId == user.UserId) : _context.CooperativeManagers.FirstOrDefault();

            return View(manager);
        }

        // GET: /CooperativeManager/FarmerHarvest
        public IActionResult FarmerHarvest(string searchFarmer)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            ViewData["Title"] = "Farmer Harvest Registry";
            ViewData["Subtitle"] = "Monitor crop planting cycles, sowing records, and actual yields.";
            ViewData["UserRole"] = "Cooperative Manager";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "Cooperative Manager";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "CM";

            var query = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                    .ThenInclude(p => p.Farm)
                        .ThenInclude(f => f.Farmer)
                .Include(c => c.Harvests)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchFarmer))
            {
                query = query.Where(c => c.LandPlot.Farm.Farmer.FullName.Contains(searchFarmer) || 
                                         c.Crop.CropName.Contains(searchFarmer));
            }

            var harvestLogs = query.Select(c => new FarmerHarvestViewModel
            {
                FarmerName = c.LandPlot.Farm.Farmer.FullName,
                PlotName = c.LandPlot.PlotName,
                CropName = c.Crop.CropName,
                SowingDate = c.SowingDate,
                ExpectedHarvestDate = c.ExpectedHarvestDate,
                ActualYield = c.Harvests.Sum(h => h.ActualQuantity),
                Unit = c.Harvests.Select(h => h.Unit).FirstOrDefault() ?? "qtl",
                Status = c.Status ?? "Active"
            }).ToList();

            return View(harvestLogs);
        }

        // POST: /CooperativeManager/UpdateAssignmentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAssignmentStatus(int assignmentId, string status)
        {
            if (!IsCooperativeManager()) return RedirectToAction("Login", "Auth");

            var assignment = _context.Assignments.FirstOrDefault(a => a.AssignmentId == assignmentId);
            if (assignment != null)
            {
                assignment.Status = status;
                if (status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                {
                    assignment.CompletedDate = DateTime.Now;
                }
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Assignment status updated to '{status.ToUpper()}' successfully.";
            }

            return RedirectToAction("Assignments");
        }
    }
}