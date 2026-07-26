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
    // This controller manages the main dashboard pages, profiling, price statistics, and reports for the Buyer role.
    // It is written with clear, linear logic and step-by-step comments for easy explanation during interviews.
    public class BuyerController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public BuyerController(SmartFarmDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Helper method to retrieve the logged-in Buyer record from the database based on session
        private Buyer? GetActiveBuyer()
        {
            // 1. Read the user session username
            var username = HttpContext.Session.GetString("UserUsername");
            if (string.IsNullOrEmpty(username)) return null;

            // 2. Fetch the corresponding Buyer record including User login details
            return _context.Buyers
                .Include(b => b.User)
                .FirstOrDefault(b => b.User.Username == username);
        }

        // GET: /Buyer/Dashboard
        public IActionResult Dashboard()
        {
            // 1. Fetch active buyer context
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 2. Query statistics from SQL Server
            // Calculate total spending (Sum of TotalAmount for completed orders)
            decimal totalSpent = _context.CropOrders
                .Where(o => o.BuyerId == buyer.BuyerId && o.Status == "Delivered")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0.00m;

            // Count active shipments (orders in progress)
            int activeShipmentsCount = _context.CropOrders
                .Count(o => o.BuyerId == buyer.BuyerId && o.Status != "Delivered");

            // Count completed orders
            int completedOrdersCount = _context.CropOrders
                .Count(o => o.BuyerId == buyer.BuyerId && o.Status == "Delivered");

            // Count total wishlist items saved by this buyer
            int wishlistCount = _context.Wishlists
                .Count(w => w.BuyerId == buyer.BuyerId);

            // 3. Fetch latest 5 orders for the summary grid
            var recentOrders = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            // 4. Fetch latest 5 notifications matching this buyer
            var notifications = _context.Notifications
                .Where(n => n.UserId == buyer.UserId)
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .ToList();

            // Compute buyer sequential order map
            var allBuyerOrdersAscending = _context.CropOrders
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.OrderId)
                .Select(o => o.OrderId)
                .ToList();

            var orderNumberMap = new Dictionary<int, int>();
            for (int i = 0; i < allBuyerOrdersAscending.Count; i++)
            {
                orderNumberMap[allBuyerOrdersAscending[i]] = i + 1;
            }
            ViewBag.OrderNumberMap = orderNumberMap;

            // 5. Query 4 recent marketplace crop listings to showcase as highlights
            var highlights = _context.CropListings
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(l => l.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(p => p.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                .Where(l => l.Status == "Available" && l.AvailableQuantity > 0)
                .OrderByDescending(l => l.ListedDate)
                .Take(4)
                .ToList();

            // 6. Bind viewbag parameters for Razor view
            ViewBag.BuyerName = buyer.FullName;
            ViewBag.CompanyName = buyer.CompanyName;
            ViewBag.TotalSpent = totalSpent;
            ViewBag.ActiveCount = activeShipmentsCount;
            ViewBag.CompletedCount = completedOrdersCount;
            ViewBag.WishlistCount = wishlistCount;
            ViewBag.RecentOrders = recentOrders;
            ViewBag.Notifications = notifications;
            ViewBag.Highlights = highlights;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View();
        }

        // GET: /Buyer/Profile
        [HttpGet("Buyer/Profile")]
        [HttpGet("Buyer/MyProfile")]
        public IActionResult Profile()
        {
            // 1. Fetch active buyer record
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 2. Setup ViewModel to bind to form
            var model = new BuyerProfileViewModel
            {
                FullName = buyer.FullName,
                CompanyName = buyer.CompanyName ?? string.Empty,
                MobileNumber = buyer.MobileNumber,
                Address = buyer.BusinessAddress,
                City = buyer.City,
                District = buyer.District,
                State = buyer.State,
                PinCode = buyer.PinCode,
                ProfilePicturePath = buyer.ProfilePicturePath
            };

            HttpContext.Session.SetString("UserProfilePicture", buyer.ProfilePicturePath ?? "");

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(model);
        }

        // POST: /Buyer/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(BuyerProfileViewModel model)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Buyer";
                return View(model);
            }

            try
            {
                // Detach the User entity to prevent it from being tracked
                if (buyer.User != null)
                {
                    _context.Entry(buyer.User).State = EntityState.Unchanged;
                }

                // Handle Profile Picture File upload
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    // Validate extension
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = System.IO.Path.GetExtension(model.ProfilePictureFile.FileName).ToLower();
                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProfilePictureFile", "Only image files (.jpg, .jpeg, .png, .gif) are allowed.");
                        ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                        ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                        ViewData["UserRole"] = "Buyer";
                        return View(model);
                    }

                    // Prepare target directory
                    var uploadsDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                    if (!System.IO.Directory.Exists(uploadsDir))
                    {
                        System.IO.Directory.CreateDirectory(uploadsDir);
                    }

                    // Create unique file name
                    var uniqueFileName = $"buyer_{buyer.BuyerId}_{DateTime.Now.Ticks}{extension}";
                    var filePath = System.IO.Path.Combine(uploadsDir, uniqueFileName);

                    // Save new profile picture file
                    using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                    {
                        model.ProfilePictureFile.CopyTo(stream);
                    }

                    // Clean up/delete old picture from disk if it exists
                    if (!string.IsNullOrEmpty(buyer.ProfilePicturePath))
                    {
                        var oldFilePath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", buyer.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Set database path property
                    buyer.ProfilePicturePath = "/uploads/profiles/" + uniqueFileName;
                    HttpContext.Session.SetString("UserProfilePicture", buyer.ProfilePicturePath);
                }

                // Update Buyer record values
                buyer.FullName = model.FullName.Trim();
                buyer.CompanyName = model.CompanyName?.Trim();
                buyer.MobileNumber = model.MobileNumber.Trim();
                buyer.BusinessAddress = model.Address?.Trim();
                buyer.City = model.City?.Trim();
                buyer.District = string.IsNullOrWhiteSpace(model.District) ? "" : model.District.Trim(); // Set empty string if null
                buyer.State = model.State?.Trim();
                buyer.PinCode = model.PinCode?.Trim();

                // Mark only the Buyer entity as modified
                _context.Entry(buyer).State = EntityState.Modified;
                _context.SaveChanges();

                // Update session name
                HttpContext.Session.SetString("UserName", buyer.FullName);

                TempData["SuccessMessage"] = "Profile details updated successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                // Get inner exception details for better debugging
                var errorMessage = ex.InnerException != null 
                    ? $"{ex.Message} - Inner: {ex.InnerException.Message}" 
                    : ex.Message;

                ViewData["ErrorMessage"] = "Error updating profile details: " + errorMessage;
                ViewBag.CompanyName = model.CompanyName;
                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Buyer";
                return View(model);
            }
        }

        // POST: /Buyer/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // Validate inputs
            if (string.IsNullOrWhiteSpace(currentPassword) || 
                string.IsNullOrWhiteSpace(newPassword) || 
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ErrorMessage"] = "All password fields are required.";
                return RedirectToAction("Profile");
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirm password do not match.";
                return RedirectToAction("Profile");
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "New password must be at least 6 characters long.";
                return RedirectToAction("Profile");
            }

            try
            {
                // Get the associated User record
                var user = _context.Users.FirstOrDefault(u => u.UserId == buyer.UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User account not found.";
                    return RedirectToAction("Profile");
                }

                // Verify current password
                if (user.PasswordHash != currentPassword) // Note: In production, use proper password hashing (BCrypt, PBKDF2, etc.)
                {
                    TempData["ErrorMessage"] = "Current password is incorrect.";
                    return RedirectToAction("Profile");
                }

                // Update password in database
                user.PasswordHash = newPassword; // Note: In production, hash the password before saving
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error changing password: " + ex.Message;
                return RedirectToAction("Profile");
            }
        }

        // GET: /Buyer/Reports
        public IActionResult Reports()
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Query all orders placed by this buyer (both active and completed)
            var orders = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(o => o.Farmer)
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(orders);
        }

        // GET: /Buyer/Transactions
        public IActionResult Transactions(string status = "")
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Query all transactions (orders) for this buyer with full navigation properties
            var query = _context.CropOrders
                .Include(o => o.CropListing)
                    .ThenInclude(l => l.Harvest)
                        .ThenInclude(h => h.CropCycle)
                            .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.Crop)
                .Include(o => o.Harvest)
                    .ThenInclude(h => h.CropCycle)
                        .ThenInclude(c => c.LandPlot)
                            .ThenInclude(p => p.Farm)
                                .ThenInclude(f => f.Farmer)
                .Include(o => o.Farmer)
                .Where(o => o.BuyerId == buyer.BuyerId)
                .AsQueryable();

            // 2. Apply status filter if provided
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(o => o.Status == status);
            }

            // 3. Execute query and order by date descending
            var transactions = query.OrderByDescending(o => o.OrderDate).ToList();

            // 4. Calculate summary statistics
            ViewBag.TotalTransactions = transactions.Count;
            // Total paid = sum of all orders (buyer pays when placing order, not when delivered)
            ViewBag.TotalSpent = transactions
                .Sum(o => (decimal?)o.TotalAmount) ?? 0m;
            // Pending = any status except Delivered (Request Sent, Farmer Accepted, Ready)
            ViewBag.PendingOrders = transactions.Count(o => o.Status != "Delivered");
            ViewBag.CompletedOrders = transactions.Count(o => o.Status == "Delivered");

            // Compute buyer sequential order map
            var allBuyerOrdersAscending = _context.CropOrders
                .Where(o => o.BuyerId == buyer.BuyerId)
                .OrderBy(o => o.OrderDate)
                .ThenBy(o => o.OrderId)
                .Select(o => o.OrderId)
                .ToList();

            var orderNumberMap = new Dictionary<int, int>();
            for (int i = 0; i < allBuyerOrdersAscending.Count; i++)
            {
                orderNumberMap[allBuyerOrdersAscending[i]] = i + 1;
            }
            ViewBag.OrderNumberMap = orderNumberMap;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(transactions);
        }

        // GET: /Buyer/Wishlist
        public IActionResult Wishlist()
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) return RedirectToAction("Login", "Auth");

            // 1. Fetch saved wishlist items
            var wishlist = _context.Wishlists
                .Include(w => w.Crop)
                .Include(w => w.Farmer)
                .Where(w => w.BuyerId == buyer.BuyerId)
                .ToList();

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Buyer";

            return View(wishlist);
        }

        // GET: /Buyer/GetNotifications - AJAX endpoint for fetching real-time notifications
        [HttpGet]
        [Route("Buyer/GetNotifications")]
        public IActionResult GetNotifications()
        {
            try
            {
                var buyer = GetActiveBuyer();
                if (buyer == null)
                {
                    return Json(new { success = false, message = "Not authenticated. Please login again." });
                }

                // Fetch latest 10 notifications for this buyer - get raw data first
                var notificationData = _context.Notifications
                    .Where(n => n.UserId == buyer.UserId)
                    .OrderByDescending(n => n.CreatedDate)
                    .Take(10)
                    .Select(n => new
                    {
                        NotificationId = n.NotificationId,
                        Title = n.Title,
                        Message = n.Message,
                        IsRead = n.IsRead,
                        CreatedDate = n.CreatedDate
                    })
                    .ToList(); // Execute query first, then format client-side

                // Now format the data client-side (not in SQL)
                var notifications = notificationData.Select(n => new
                {
                    notificationId = n.NotificationId,
                    title = n.Title,
                    message = n.Message,
                    isRead = n.IsRead,
                    createdDate = n.CreatedDate.ToString("MMM dd, yyyy hh:mm tt"),
                    timeAgo = GetTimeAgo(n.CreatedDate) // Now safe to call
                }).ToList();

                var unreadCount = _context.Notifications
                    .Count(n => n.UserId == buyer.UserId && !n.IsRead);

                return Json(new
                {
                    success = true,
                    notifications = notifications,
                    unreadCount = unreadCount
                });
            }
            catch (Exception ex)
            {
                // Log error for debugging
                Console.WriteLine($"GetNotifications Error: {ex.Message}");
                return Json(new { success = false, message = $"Error loading notifications: {ex.Message}" });
            }
        }

        // POST: /Buyer/MarkNotificationAsRead - Mark a notification as read
        [HttpPost]
        public IActionResult MarkNotificationAsRead(int notificationId)
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) 
                return Json(new { success = false, message = "Not authenticated" });

            var notification = _context.Notifications
                .FirstOrDefault(n => n.NotificationId == notificationId && n.UserId == buyer.UserId);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Notification not found" });
        }

        // POST: /Buyer/MarkAllNotificationsAsRead - Mark all notifications as read
        [HttpPost]
        public IActionResult MarkAllNotificationsAsRead()
        {
            var buyer = GetActiveBuyer();
            if (buyer == null) 
                return Json(new { success = false, message = "Not authenticated" });

            var unreadNotifications = _context.Notifications
                .Where(n => n.UserId == buyer.UserId && !n.IsRead)
                .ToList();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();

            return Json(new { success = true, count = unreadNotifications.Count });
        }

        // Helper method to calculate time ago
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} week{((int)(timeSpan.TotalDays / 7) > 1 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} month{((int)(timeSpan.TotalDays / 30) > 1 ? "s" : "")} ago";

            return $"{(int)(timeSpan.TotalDays / 365)} year{((int)(timeSpan.TotalDays / 365) > 1 ? "s" : "")} ago";
        }
    }
}
