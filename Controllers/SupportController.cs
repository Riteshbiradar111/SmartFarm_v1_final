using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace SmartFarmMVC.Controllers
{
    public class SupportController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public SupportController(SmartFarmDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Helper check to validate Farmer Session and retrieve Farmer record
        private Smart_Farm_and_Crop_Yeild_Management_System.Models.Farmer? GetActiveFarmer()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("UserUsername");

            if (role != "Farmer" || string.IsNullOrEmpty(username))
            {
                return null;
            }

            return _context.Farmers.FirstOrDefault(f => f.User.Username == username);
        }

        // GET: /Support
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var queries = _context.SupportQueries
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .Include(q => q.AssignedToUser)
                .Where(q => q.FarmerId == farmer.FarmerId)
                .OrderByDescending(q => q.CreatedDate)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(queries);
        }

        // GET: /Support/Create
        public IActionResult Create()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            var plots = _context.LandPlots.Where(p => p.Farm.FarmerId == farmer.FarmerId).ToList();

            if (farms.Count == 0)
            {
                TempData["ErrorMessage"] = "You must create a farm first before raising support queries.";
                return RedirectToAction("Index");
            }

            ViewBag.Farms = farms;
            ViewBag.Plots = plots;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(new SupportQueryViewModel());
        }

        // POST: /Support/Create
        [HttpPost]
         
        public async Task<IActionResult> Create(SupportQueryViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var farms = _context.Farms.Where(f => f.FarmerId == farmer.FarmerId).ToList();
            var plots = _context.LandPlots.Where(p => p.Farm.FarmerId == farmer.FarmerId).ToList();

            ViewBag.Farms = farms;
            ViewBag.Plots = plots;

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
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "support");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(fileStream);
                    }

                    relativePath = "/uploads/support/" + uniqueFileName;
                }

                // Create new ticket with Pending Assignment status for Cooperative Manager to assign
                var query = new SupportQuery
                {
                    FarmerId = farmer.FarmerId,
                    QueryType = model.QueryType,
                    Title = model.Title.Trim(),
                    Description = model.Description.Trim(),
                    FarmId = model.FarmId,
                    PlotId = model.PlotId,
                    Priority = model.Priority,
                    ImagePath = relativePath,
                    Status = "Pending Assignment",
                    CreatedDate = DateTime.Now,
                    AssignedToUserId = null
                };

                _context.SupportQueries.Add(query);
                await _context.SaveChangesAsync();

                // Notify Cooperative Manager(s) to assign an officer or agronomist
                var managers = _context.Users.Where(u => u.RoleId == 6 && u.IsActive).ToList();
                foreach (var mgr in managers)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = mgr.UserId,
                        Title = "New Support Ticket Raised",
                        Message = $"Farmer {farmer.FullName} raised ticket #{query.QueryId}: \"{query.Title}\". Please assign an Agronomist or Field Officer.",
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    });
                }
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Support query submitted successfully! It has been routed to the Cooperative Manager for staff assignment.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error raising support query: " + ex.Message;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                return View(model);
            }
        }

        // GET: /Support/Details/{id}
        public IActionResult Details(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .Include(q => q.AssignedToUser)
                .FirstOrDefault(q => q.QueryId == id && q.FarmerId == farmer.FarmerId);

            if (query == null)
            {
                TempData["ErrorMessage"] = "Support ticket not found.";
                return RedirectToAction("Index");
            }

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";

            return View(query);
        }

        // POST: /Support/ConfirmVisitSchedule
        [HttpPost]
         
        public async Task<IActionResult> ConfirmVisitSchedule(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries.FirstOrDefault(q => q.QueryId == id && q.FarmerId == farmer.FarmerId);
            if (query != null)
            {
                query.Status = "Field Visit Confirmed";
                await _context.SaveChangesAsync();

                // Also update any linked FieldVisit
                var linkedVisit = _context.FieldVisits
                    .Where(v => v.FarmerId == farmer.FarmerId && v.Status == "Scheduled")
                    .OrderByDescending(v => v.CreatedDate)
                    .FirstOrDefault();

                if (linkedVisit != null)
                {
                    linkedVisit.Notes = (linkedVisit.Notes ?? "") + " [Confirmed by Farmer]";
                    await _context.SaveChangesAsync();
                }

                // Notify assigned Field Officer
                if (query.AssignedToUserId.HasValue)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = query.AssignedToUserId.Value,
                        Title = "Field Visit Schedule Accepted",
                        Message = $"Farmer {farmer.FullName} accepted your field visit schedule for ticket #{query.QueryId} (Scheduled: {query.VisitDate:dd-MM-yyyy HH:mm}).",
                        IsRead = false,
                        CreatedDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Field visit schedule confirmed successfully! The Field Officer has been notified.";
            }

            return RedirectToAction("Details", new { id = id });
        }

        // POST: /Support/UpdateImprovementStatus
        [HttpPost]
         
        public async Task<IActionResult> UpdateImprovementStatus(int id, string status)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries.FirstOrDefault(q => q.QueryId == id && q.FarmerId == farmer.FarmerId);
            if (query != null)
            {
                query.ImprovementStatus = status;
                if (status == "Completed" && query.Status != "Resolved")
                {
                    query.Status = "Resolved";
                    query.ResolutionDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Improvement plan implementation status updated successfully!";
            }

            return RedirectToAction("Details", new { id = id });
        }

        // GET: /Support/DownloadRecommendation/{id}
        public IActionResult DownloadRecommendation(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .FirstOrDefault(q => q.QueryId == id && q.FarmerId == farmer.FarmerId);

            if (query == null || string.IsNullOrEmpty(query.AgronomistRecommendation))
            {
                return NotFound();
            }

            return View("PrintRecommendation", query);
        }

        // GET: /Support/DownloadReport/{id}
        public IActionResult DownloadReport(int id)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            var query = _context.SupportQueries
                .Include(q => q.Farm)
                .Include(q => q.LandPlot)
                .FirstOrDefault(q => q.QueryId == id && q.FarmerId == farmer.FarmerId);

            if (query == null || !query.VisitDate.HasValue)
            {
                return NotFound();
            }

            return View("PrintReport", query);
        }
    }
}
