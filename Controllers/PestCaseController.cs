using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    public class PestCaseController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PestCaseController(SmartFarmDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Helper to check user session
        private string? GetSessionRole() => HttpContext.Session.GetString("UserRole");
        private string? GetSessionUsername() => HttpContext.Session.GetString("UserUsername");

        // GET: /PestCase
        // Shows relevant pest cases based on the logged-in user's role
        public IActionResult Index()
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            IQueryable<PestCase> query = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                .Include(p => p.AssignedOfficer);

            // Filter records based on role
            if (role == "Farmer")
            {
                var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
                if (farmer == null) return RedirectToAction("Login", "Auth");
                query = query.Where(p => p.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId);
            }
            else if (role == "Field Officer")
            {
                var officer = _context.Users.FirstOrDefault(u => u.Username == username);
                if (officer == null) return RedirectToAction("Login", "Auth");
                query = query.Where(p => p.AssignedOfficerId == officer.UserId);
            }
            // Agronomist, Admin, and Cooperative Manager see all cases for approval/inspections

            var cases = query.OrderByDescending(p => p.CreatedDate).ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = role;

            return View(cases);
        }

        // GET: /PestCase/Details/{id}
        public IActionResult Details(int id)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.Crop)
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null)
            {
                TempData["ErrorMessage"] = "Pest Case report not found.";
                return RedirectToAction("Index");
            }

            // Bind Field Officers list for Cooperative Manager selection dropdown
            if (role == "Cooperative Manager")
            {
                ViewBag.FieldOfficers = _context.Users.Where(u => u.RoleId == 5).ToList(); // RoleId 5 = Field Officer
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = role;

            return View(pestCase);
        }

        // GET: /PestCase/Create
        public IActionResult Create(int? cycleId)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                    .ThenInclude(p => p.Farm)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            if (cycles.Count == 0)
            {
                TempData["ErrorMessage"] = "You must have an active crop cycle to report pest incidents.";
                return RedirectToAction("Index", "CropCycle");
            }

            ViewBag.Cycles = cycles;

            var model = new PestCaseViewModel();
            if (cycleId.HasValue)
            {
                model.CropCycleId = cycleId.Value;
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(model);
        }

        // POST: /PestCase/Create
        [HttpPost]
        public IActionResult Create(PestCaseViewModel model)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var cycles = _context.CropCycles
                .Include(c => c.Crop)
                .Include(c => c.LandPlot)
                    .ThenInclude(p => p.Farm)
                .Where(c => c.LandPlot.Farm.FarmerId == farmer.FarmerId && c.Status == "Active")
                .ToList();

            ViewBag.Cycles = cycles;

            if (!ModelState.IsValid)
            {
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }

            try
            {
                string? relativePath = null;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ImageFile.CopyTo(fileStream);
                    }

                    relativePath = "/uploads/" + uniqueFileName;
                }

                // Auto-assign the Agronomist that the Cooperative Manager assigned to this farmer,
                // so the case is routed to them and the farmer can see the assigned agronomist's name.
                var agronomistUserIds = _context.Agronomists.Select(a => a.UserId).ToList();
                var agronomistAssignment = _context.Assignments
                    .Where(a => a.FarmerId == farmer.FarmerId && agronomistUserIds.Contains(a.OfficerId))
                    .OrderByDescending(a => a.AssignedDate)
                    .FirstOrDefault();

                var pestCase = new PestCase
                {
                    CropCycleId = model.CropCycleId,
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),
                    Priority = model.Priority,
                    Status = "Pending",
                    ImagePath = relativePath,
                    CreatedDate = DateTime.Now,
                    AssignedOfficerId = agronomistAssignment?.OfficerId
                };

                _context.PestCases.Add(pestCase);
                _context.SaveChanges();

                // Notify the assigned agronomist that a new case awaits their review.
                if (pestCase.AssignedOfficerId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = pestCase.AssignedOfficerId.Value,
                        Title = "New Pest Case Assigned",
                        Message = $"A new pest case '{pestCase.Title}' from {farmer.FullName} has been assigned to you for review.",
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = "Pest incident reported successfully. Agronomist will review it.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error reporting pest incident: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // POST: /PestCase/AssignOfficer/{id}
        // Action for Cooperative Manager to assign a Field Officer
        [HttpPost]
        public IActionResult AssignOfficer(int id, int officerId)
        {
            var role = GetSessionRole();
            if (role != "Cooperative Manager") return Unauthorized();

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                                .ThenInclude(farmer => farmer.User)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            try
            {
                pestCase.AssignedOfficerId = officerId;
                pestCase.Status = "Field Visit";
                _context.SaveChanges();

                // Notify assigned field officer
                var notification = new Notification
                {
                    UserId = officerId,
                    Title = "Field Visit Assignment",
                    Message = $"You have been assigned to visit farmer for pest case #{pestCase.PestCaseId} - {pestCase.Title}.",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);

                // Notify farmer
                var farmerNotification = new Notification
                {
                    UserId = pestCase.CropCycle.LandPlot.Farm.Farmer.UserId,
                    Title = "Field Officer Assigned",
                    Message = $"A field officer has been assigned to visit your farm for pest case #{pestCase.PestCaseId}.",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(farmerNotification);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Field Officer assigned successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error assigning officer: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/CompleteFieldVisit/{id}
        // Action for Field Officer to mark visit as completed
        [HttpPost]
        public IActionResult CompleteFieldVisit(int id, string visitNotes)
        {
            var role = GetSessionRole();
            if (role != "Field Officer") return Unauthorized();

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                                .ThenInclude(farmer => farmer.User)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            if (string.IsNullOrWhiteSpace(visitNotes))
            {
                TempData["ErrorMessage"] = "Visit notes cannot be empty.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.FieldReport = visitNotes.Trim();
                pestCase.FieldVisitCompletedDate = DateTime.Now;
                pestCase.Status = "Field Visit"; // Ensure the status is set to "Field Visit" so the farmer can close the query
                _context.SaveChanges();

                // Notify farmer
                var notification = new Notification
                {
                    UserId = pestCase.CropCycle.LandPlot.Farm.Farmer.UserId,
                    Title = "Field Visit Completed",
                    Message = $"Field officer has completed the visit for pest case #{pestCase.PestCaseId} - {pestCase.Title}. You can now close the query.",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Field visit marked as completed. Farmer has been notified.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error completing visit: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/ApproveAdvisory/{id}
        [HttpPost]
        public IActionResult ApproveAdvisory(int id)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                .FirstOrDefault(p => p.PestCaseId == id && p.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (pestCase == null) return NotFound();

            pestCase.FarmerResponseToReport = "Approved";
            pestCase.FarmerResponseDate = DateTime.Now;
            pestCase.IsClosed = true;
            pestCase.ClosedDate = DateTime.Now;
            pestCase.Status = "Resolved";

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Advisory approved. Pest Case has been resolved and closed.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/RejectAdvisory/{id}
        // Auto-escalates case to assigned Field Officer
        [HttpPost]
        public IActionResult RejectAdvisory(int id)
        {
            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == GetSessionUsername());
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                .FirstOrDefault(p => p.PestCaseId == id && p.CropCycle.LandPlot.Farm.FarmerId == farmer.FarmerId);

            if (pestCase == null) return NotFound();

            pestCase.FarmerResponseToReport = "Not Approved";
            pestCase.FarmerResponseDate = DateTime.Now;
            pestCase.Status = "ESCALATED TO OFFICER";

            // Lookup the assigned Field Officer (must be a Field-Officer-role assignment, not the agronomist)
            var fieldOfficerUserIds = _context.FieldOfficers.Select(o => o.UserId).ToList();
            var assignment = _context.Assignments
                .FirstOrDefault(a => a.FarmerId == farmer.FarmerId && fieldOfficerUserIds.Contains(a.OfficerId));
            if (assignment != null && assignment.OfficerId > 0)
            {
                pestCase.AssignedOfficerId = assignment.OfficerId;
            }
            else
            {
                // Default to first active field officer if unassigned
                var firstOfficer = _context.FieldOfficers.FirstOrDefault();
                if (firstOfficer != null)
                {
                    pestCase.AssignedOfficerId = firstOfficer.UserId;
                }
            }

            _context.SaveChanges();

            // Create notification for Field Officer
            if (pestCase.AssignedOfficerId.HasValue)
            {
                var notification = new Notification
                {
                    UserId = pestCase.AssignedOfficerId.Value,
                    Title = "Escalated Pest Case",
                    Message = $"Pest Case #{pestCase.PestCaseId} for {farmer.FullName} was escalated for an on-site field inspection.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Advisory marked as Not Approved. Case has been automatically escalated to your Field Officer for inspection.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/FarmerCloseQuery/{id}
        // Farmer closes the query after field visit
        [HttpPost]
        public IActionResult FarmerCloseQuery(int id)
        {
            var role = GetSessionRole();
            if (role != "Farmer") return Unauthorized();

            var pestCase = _context.PestCases.Find(id);
            if (pestCase == null) return NotFound();

            if (pestCase.Status != "Field Visit" || !pestCase.FieldVisitCompletedDate.HasValue)
            {
                TempData["ErrorMessage"] = "Cannot close query at this stage. Field visit must be completed first.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.Status = "Resolved";
                pestCase.ResolvedDate = DateTime.Now;
                pestCase.IsClosed = true;
                pestCase.ClosedDate = DateTime.Now;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Thank you! Your query has been successfully closed.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error closing query: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/SubmitReport/{id}
        // Action for Agronomist to upload report and send to farmer
        [HttpPost]
        public IActionResult SubmitReport(int id, string reportText, string recommendationText)
        {
            var role = GetSessionRole();
            if (role != "Agronomist") return Unauthorized();

            var pestCase = _context.PestCases
                .Include(p => p.CropCycle)
                    .ThenInclude(c => c.LandPlot)
                        .ThenInclude(plot => plot.Farm)
                            .ThenInclude(farm => farm.Farmer)
                                .ThenInclude(farmer => farmer.User)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            if (string.IsNullOrWhiteSpace(reportText) || string.IsNullOrWhiteSpace(recommendationText))
            {
                TempData["ErrorMessage"] = "Both report and recommendation content are required.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.FieldReport = reportText.Trim();
                pestCase.Recommendation = recommendationText.Trim();
                pestCase.Status = "Report Uploaded";
                pestCase.ReportUploadedDate = DateTime.Now;
                _context.SaveChanges();

                // Send notification to farmer
                var notification = new Notification
                {
                    UserId = pestCase.CropCycle.LandPlot.Farm.Farmer.UserId,
                    Title = "Pest Report Available",
                    Message = $"Agronomist has uploaded a report for your pest case #{pestCase.PestCaseId} - {pestCase.Title}. Please review and take action.",
                    CreatedDate = DateTime.Now,
                    IsRead = false
                };
                _context.Notifications.Add(notification);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Report uploaded successfully. Farmer has been notified.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error submitting report: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/FarmerApproveReport/{id}
        // Farmer approves the report and closes the query
        [HttpPost]
        public IActionResult FarmerApproveReport(int id)
        {
            var role = GetSessionRole();
            if (role != "Farmer") return Unauthorized();

            var pestCase = _context.PestCases.Find(id);
            if (pestCase == null) return NotFound();

            if (pestCase.Status != "Report Uploaded")
            {
                TempData["ErrorMessage"] = "Cannot approve report at this stage.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.FarmerResponseToReport = "Approved";
                pestCase.FarmerResponseDate = DateTime.Now;
                pestCase.Status = "Resolved";
                pestCase.ResolvedDate = DateTime.Now;
                pestCase.IsClosed = true;
                pestCase.ClosedDate = DateTime.Now;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Thank you! Your query has been successfully closed.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error approving report: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/FarmerRequestAssistance/{id}
        // Farmer requests further field assistance
        [HttpPost]
        public IActionResult FarmerRequestAssistance(int id)
        {
            var role = GetSessionRole();
            if (role != "Farmer") return Unauthorized();

            var pestCase = _context.PestCases.Find(id);
            if (pestCase == null) return NotFound();

            if (pestCase.Status != "Report Uploaded")
            {
                TempData["ErrorMessage"] = "Cannot request assistance at this stage.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.FarmerResponseToReport = "NeedsAssistance";
                pestCase.FarmerResponseDate = DateTime.Now;
                pestCase.FieldVisitRequested = true;
                pestCase.Status = "Field Visit Requested";
                _context.SaveChanges();

                // Notify Cooperative Manager to assign field officer
                var cooperativeManager = _context.Users.FirstOrDefault(u => u.RoleId == 4); // RoleId 4 = Cooperative Manager
                if (cooperativeManager != null)
                {
                    var notification = new Notification
                    {
                        UserId = cooperativeManager.UserId,
                        Title = "Field Visit Required",
                        Message = $"Farmer has requested field assistance for pest case #{pestCase.PestCaseId} - {pestCase.Title}. Please assign a field officer.",
                        CreatedDate = DateTime.Now,
                        IsRead = false
                    };
                    _context.Notifications.Add(notification);
                    _context.SaveChanges();
                }

                TempData["SuccessMessage"] = "Field assistance requested. A field officer will be assigned soon.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error requesting assistance: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/SubmitRecommendation/{id}
        // This method is deprecated - use SubmitReport instead
        [HttpPost]
        [Obsolete("Use SubmitReport instead which combines report and recommendation")]
        public IActionResult SubmitRecommendation(int id, string recommendationText)
        {
            var role = GetSessionRole();
            if (role != "Agronomist") return Unauthorized();

            var pestCase = _context.PestCases.Find(id);
            if (pestCase == null) return NotFound();

            if (string.IsNullOrWhiteSpace(recommendationText))
            {
                TempData["ErrorMessage"] = "Recommendation content cannot be empty.";
                return RedirectToAction("Details", new { id = id });
            }

            try
            {
                pestCase.Recommendation = recommendationText.Trim();
                pestCase.Status = "Resolved";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Treatment recommendation sent to the farmer successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error submitting recommendation: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/AcceptVisitSchedule/{id}
        // Farmer accepts & confirms scheduled visit date from Field Officer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AcceptVisitSchedule(int id)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (role != "Farmer" || string.IsNullOrEmpty(username)) return Unauthorized();

            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            pestCase.Status = "Field Visit Confirmed";
            _context.SaveChanges();

            // Notify assigned officer
            if (pestCase.AssignedOfficerId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = pestCase.AssignedOfficerId.Value,
                    Title = "Farmer Confirmed Visit Schedule",
                    Message = $"Farmer {farmer.FullName} accepted and confirmed the scheduled visit date for Pest Incident #{pestCase.PestCaseId}.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "Scheduled visit date confirmed! The Field Officer has been notified.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/ApproveFieldReport/{id}
        // Farmer approves field officer report -> Marks issue Resolved
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveFieldReport(int id)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (role != "Farmer" || string.IsNullOrEmpty(username)) return Unauthorized();

            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            pestCase.Status = "Resolved";
            pestCase.IsClosed = true;
            pestCase.ResolvedDate = DateTime.Now;
            pestCase.ClosedDate = DateTime.Now;
            pestCase.FarmerResponseToReport = "Approved";
            pestCase.FarmerResponseDate = DateTime.Now;

            _context.SaveChanges();

            // Notify assigned officer
            if (pestCase.AssignedOfficerId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = pestCase.AssignedOfficerId.Value,
                    Title = "Field Report Approved — Case Resolved",
                    Message = $"Farmer {farmer.FullName} approved your field inspection report for incident #{pestCase.PestCaseId}. The issue is now marked as Resolved.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                _context.SaveChanges();
            }

            TempData["SuccessMessage"] = "You have approved the field report. The pest incident is now marked as Resolved.";
            return RedirectToAction("Details", new { id = id });
        }

        // POST: /PestCase/RejectFieldReport/{id}
        // Farmer rejects field officer report -> Escalates back for re-visit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectFieldReport(int id, string rejectionReason)
        {
            var role = GetSessionRole();
            var username = GetSessionUsername();
            if (role != "Farmer" || string.IsNullOrEmpty(username)) return Unauthorized();

            var farmer = _context.Farmers.FirstOrDefault(f => f.User.Username == username);
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var pestCase = _context.PestCases
                .Include(p => p.AssignedOfficer)
                .FirstOrDefault(p => p.PestCaseId == id);

            if (pestCase == null) return NotFound();

            pestCase.Status = "ESCALATED TO OFFICER";
            pestCase.FarmerResponseToReport = "Rejected";
            pestCase.FarmerResponseDate = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(rejectionReason))
            {
                pestCase.FieldReport = (pestCase.FieldReport ?? "") + $"\n\n[Farmer Rejection Reason: {rejectionReason.Trim()}]";
            }

            _context.SaveChanges();

            // Notify officer
            if (pestCase.AssignedOfficerId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = pestCase.AssignedOfficerId.Value,
                    Title = "Field Report Rejected — Re-visit Required",
                    Message = $"Farmer {farmer.FullName} rejected the field report for incident #{pestCase.PestCaseId}. Reason: {rejectionReason}. Please schedule a follow-up visit.",
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
                _context.SaveChanges();
            }

            TempData["WarningMessage"] = "You rejected the field report. The Field Officer has been notified to conduct a re-visit.";
            return RedirectToAction("Details", new { id = id });
        }
    }
}
