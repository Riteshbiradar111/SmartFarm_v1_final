using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly SmartFarmDbContext _context;

        public AdminController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            string? sessionName = HttpContext.Session.GetString("UserName");
            string name = !string.IsNullOrEmpty(sessionName) ? sessionName : "System Admin";
            string initials = HttpContext.Session.GetString("UserInitials") ?? "SA";

            ViewData["Title"] = "Admin Dashboard";
            ViewData["Subtitle"] = "System overview � SmartFarm Platform. All systems operational.";
            ViewData["UserRole"] = "Admin";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#dc2626";

            // Build the ViewModel with real data
            var viewModel = await BuildDashboardViewModelAsync();

            return View(viewModel);
        }

        /// <summary>
        /// Builds the Admin Dashboard ViewModel with data from database
        /// </summary>
        private async Task<AdminDashboardViewModel> BuildDashboardViewModelAsync()
        {
            var model = new AdminDashboardViewModel();

            // ===== KPI STATISTICS =====

            // Total Users (exclude deleted users)
            model.TotalUsers = _context.Users.Count(u => !u.IsDeleted);

            // Users added this month (exclude deleted users)
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            model.UsersAddedThisMonth = _context.Users
                .Count(u => u.CreatedAt >= firstDayOfMonth && !u.IsDeleted);

            // Active Farms
            model.ActiveFarms = _context.Farms.Count();

            // Total States (distinct states from farms)
            model.TotalStates = _context.Farms
                .Select(f => f.State)
                .Distinct()
                .Count();

            // Active Crops (CropCycles with Status = 'Active')
            model.ActiveCrops = _context.CropCycles
                .Count(cc => cc.Status == "Active");

            // Pending Approvals - REMOVED (backend query disabled per instructions)
            model.PendingApprovals = 0;

            // ===== PENDING USER APPROVALS =====
            // Query removed as per requirements (the approvals UI card has been deleted)
            model.PendingUserApprovals = new List<PendingUserApprovalDto>();

            // ===== SYSTEM AUDIT LOGS (Recent Notifications or Events) =====
            model.RecentAuditLogs = GetRecentAuditLogs();

            // ===== USER GROWTH CHART (Last 12 Months) =====
            model.UserGrowthData = GetUserGrowthChartData();

            // ===== USER DISTRIBUTION BY ROLE (Donut Chart) =====
            model.UserDistributionData = GetUserDistributionChartData();

            return model;
        }

        /// <summary>
        /// Returns CSS class for role badge color
        /// </summary>
        private string GetRoleBadgeClass(string roleName)
        {
            return roleName?.ToUpper() switch
            {
                "FARMER" => "badge-farmer",
                "AGRONOMIST" => "badge-agronomist",
                "FIELD OFFICER" => "badge-officer",
                "COOPERATIVE MANAGER" => "badge-coop",
                "BUYER" => "badge-buyer",
                "ADMIN" => "badge-admin",
                _ => "badge-secondary"
            };
        }

        /// <summary>
        /// Gets recent audit logs (simulated from Notifications or recent user registrations)
        /// </summary>
        private List<SystemAuditLogDto> GetRecentAuditLogs()
        {
            var logs = new List<SystemAuditLogDto>();

            // Get recent user registrations
            var recentUsers = _context.Users
                .Include(u => u.Role)
                .Include(u => u.Farmers)
                .OrderByDescending(u => u.CreatedAt)
                .Take(3)
                .ToList();

            foreach (var user in recentUsers)
            {
                string fullName = user.Username;
                if (user.Farmers.Any())
                    fullName = user.Farmers.First().FullName;

                if (user.IsActive)
                {
                    logs.Add(new SystemAuditLogDto
                    {
                        Message = $"Approved registration for {fullName} ({user.Role.RoleName}).",
                        TimeAgo = GetTimeAgo(user.CreatedAt),
                        BadgeColor = "green"
                    });
                }
                else
                {
                    logs.Add(new SystemAuditLogDto
                    {
                        Message = $"New user registration: {fullName} pending approval.",
                        TimeAgo = GetTimeAgo(user.CreatedAt),
                        BadgeColor = "orange"
                    });
                }
            }

            // Add system events (hardcoded examples matching screenshot)
            logs.Add(new SystemAuditLogDto
            {
                Message = "Database backup completed successfully at 02:00 AM.",
                TimeAgo = "6 hours ago",
                BadgeColor = "blue"
            });

            logs.Add(new SystemAuditLogDto
            {
                Message = "Sensor anomaly alert triggered: Farm Yadav Plot A (Soil pH reading of 9.1).",
                TimeAgo = "8 hours ago",
                BadgeColor = "red"
            });

            logs.Add(new SystemAuditLogDto
            {
                Message = "Master Data Updated: Add new cotton varieties to system database tables.",
                TimeAgo = "1 day ago",
                BadgeColor = "blue"
            });

            return logs.Take(7).ToList();
        }

        /// <summary>
        /// Calculates user growth data for the last 12 months
        /// </summary>
        private UserGrowthChartData GetUserGrowthChartData()
        {
            var data = new UserGrowthChartData();
            var now = DateTime.Now;

            // Generate labels for last 12 months
            for (int i = 11; i >= 0; i--)
            {
                var month = now.AddMonths(-i);
                data.Labels.Add(month.ToString("MMM"));
            }

            // Calculate cumulative user count for each month (exclude deleted users)
            for (int i = 11; i >= 0; i--)
            {
                var monthEnd = new DateTime(now.Year, now.Month, 1).AddMonths(-i + 1).AddDays(-1);
                var userCount = _context.Users.Count(u => u.CreatedAt <= monthEnd && !u.IsDeleted);
                data.Data.Add(userCount);
            }

            return data;
        }

        /// <summary>
        /// Calculates user distribution by role
        /// </summary>
        private UserDistributionChartData GetUserDistributionChartData()
        {
            var data = new UserDistributionChartData();

            // Exclude deleted users from distribution
            var distribution = _context.Users
                .Include(u => u.Role)
                .Where(u => !u.IsDeleted)
                .GroupBy(u => u.Role.RoleName)
                .Select(g => new { RoleName = g.Key, Count = g.Count() })
                .OrderBy(x => x.RoleName)
                .ToList();

            // Populate labels and data in consistent order
            var roleOrder = new[] { "Farmer", "Agronomist", "Field Officer", "Cooperative Manager", "Buyer", "Admin" };

            foreach (var roleName in roleOrder)
            {
                var roleData = distribution.FirstOrDefault(d => d.RoleName == roleName);
                if (roleData != null)
                {
                    data.Labels.Add(roleData.RoleName);
                    data.Data.Add(roleData.Count);
                }
            }

            return data;
        }

        /// <summary>
        /// Helper to convert DateTime to "time ago" format
        /// </summary>
        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;

            if (timeSpan.TotalMinutes < 1) return "just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} {((int)timeSpan.TotalMinutes == 1 ? "minute" : "minutes")} ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} {((int)timeSpan.TotalHours == 1 ? "hour" : "hours")} ago";
            if (timeSpan.TotalDays < 30) return $"{(int)timeSpan.TotalDays} {((int)timeSpan.TotalDays == 1 ? "day" : "days")} ago";
            if (timeSpan.TotalDays < 365) return $"{(int)(timeSpan.TotalDays / 30)} {((int)(timeSpan.TotalDays / 30) == 1 ? "month" : "months")} ago";
            return $"{(int)(timeSpan.TotalDays / 365)} {((int)(timeSpan.TotalDays / 365) == 1 ? "year" : "years")} ago";
        }

        // ===================================================================
        // USER MANAGEMENT SECTION
        // ===================================================================

        // GET: /Admin/UserManagement
        public async Task<IActionResult> UserManagement(string? searchTerm, string? roleFilter, string? statusFilter)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Set ViewData for layout
            ViewData["Title"] = "User Management";
            ViewData["Subtitle"] = "Manage all SmartFarm users.";
            ViewData["UserRole"] = "Admin";
            ViewData["UserName"] = HttpContext.Session.GetString("UserName") ?? "System Admin";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "SA";
            ViewData["RoleColor"] = "#dc2626";

            var model = await BuildUserManagementViewModelAsync(searchTerm, roleFilter, statusFilter);
            return View(model);
        }

        private async Task<UserManagementViewModel> BuildUserManagementViewModelAsync(string? searchTerm, string? roleFilter, string? statusFilter)
        {
            var model = new UserManagementViewModel
            {
                SearchTerm = searchTerm,
                RoleFilter = roleFilter,
                StatusFilter = statusFilter
            };

            // Statistics (exclude deleted users)
            model.TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted);
            model.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted && !u.IsBlocked);
            model.PendingApproval = await _context.Users.CountAsync(u => !u.IsActive && !u.IsDeleted && !u.IsBlocked);
            model.BlockedUsers = await _context.Users.CountAsync(u => u.IsBlocked && !u.IsDeleted);

            // Build query - exclude deleted users
            var query = _context.Users.Include(u => u.Role).Include(u => u.Farmers).Include(u => u.Buyers).Where(u => !u.IsDeleted).AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => 
                    u.Username.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm) ||
                    (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                    (u.Phone != null && u.Phone.Contains(searchTerm)));
            }

            // Role filter
            if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All Roles")
            {
                query = query.Where(u => u.Role.RoleName == roleFilter);
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All Status")
            {
                if (statusFilter == "ACTIVE")
                {
                    query = query.Where(u => u.IsActive && !u.IsBlocked);
                }
                else if (statusFilter == "PENDING")
                {
                    query = query.Where(u => !u.IsActive && !u.IsBlocked);
                }
                else if (statusFilter == "BLOCKED")
                {
                    query = query.Where(u => u.IsBlocked);
                }
            }

            // Get all users (no pagination)
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            // Fetch assignments and profiles once to avoid N+1 database queries
            var farmers = users.SelectMany(u => u.Farmers).ToList();
            var farmerIds = farmers.Select(f => f.FarmerId).ToList();
            var assignments = await _context.Assignments
                .Include(a => a.Officer)
                .Where(a => farmerIds.Contains(a.FarmerId))
                .ToListAsync();

            var fieldOfficers = await _context.FieldOfficers.ToListAsync();
            var agronomists = await _context.Agronomists.ToListAsync();

            // Map to DTO
            model.Users = users.Select(u => 
            {
                var farmer = u.Farmers.FirstOrDefault();
                string fieldOfficerName = "N/A";
                string agronomistName = "N/A";

                if (farmer != null)
                {
                    var farmerAssignments = assignments.Where(a => a.FarmerId == farmer.FarmerId).ToList();
                    
                    // Field Officer: check if officer is in the FieldOfficers list
                    var foAssignment = farmerAssignments.FirstOrDefault(a => fieldOfficers.Any(fo => fo.UserId == a.OfficerId));
                    if (foAssignment != null)
                    {
                        var fo = fieldOfficers.FirstOrDefault(f => f.UserId == foAssignment.OfficerId);
                        if (fo != null)
                        {
                            fieldOfficerName = fo.FullName;
                        }
                    }

                    // Agronomist: check if officer is in the Agronomists list
                    var agroAssignment = farmerAssignments.FirstOrDefault(a => agronomists.Any(agro => agro.UserId == a.OfficerId));
                    if (agroAssignment != null)
                    {
                        var agro = agronomists.FirstOrDefault(a => a.UserId == agroAssignment.OfficerId);
                        if (agro != null)
                        {
                            agronomistName = agro.FullName;
                        }
                    }
                }

                return new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    FullName = GetUserFullName(u),
                    ProfileInitials = GetInitials(GetUserFullName(u)),
                    RoleName = u.Role.RoleName,
                    RoleBadgeClass = GetRoleBadgeClass(u.Role.RoleName),
                    Status = u.IsBlocked ? "BLOCKED" : (u.IsActive ? "ACTIVE" : "PENDING"),
                    StatusBadgeClass = u.IsBlocked ? "badge badge-red" : (u.IsActive ? "badge badge-green" : "badge badge-yellow"),
                    JoinDate = u.CreatedAt,
                    JoinDateFormatted = u.CreatedAt.ToString("yyyy-MM-dd"),
                    LastLogin = u.LastLogin,
                    LastLoginFormatted = u.LastLogin.HasValue ? u.LastLogin.Value.ToString("yyyy-MM-dd HH:mm") : "Never",
                    AssignedFieldOfficer = fieldOfficerName,
                    AssignedAgronomist = agronomistName
                };
            }).ToList();

            return model;
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserViewModel model)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrEmpty(msg));

                var errorMessage = errors.Any() 
                    ? string.Join(". ", errors) 
                    : "Please fill in all required fields correctly.";

                return Json(new { success = false, message = errorMessage });
            }

            try
            {
                // Check if username already exists (exclude deleted users)
                if (await _context.Users.AnyAsync(u => u.Username == model.Username && !u.IsDeleted))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                // Check if email already exists (exclude deleted users)
                if (await _context.Users.AnyAsync(u => u.Email == model.Email && !u.IsDeleted))
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                // Create new user
                var user = new User
                {
                    Username = model.Username,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    FullName = model.FullName,
                    Phone = model.Phone,
                    RoleId = model.RoleId,
                    IsActive = true,
                    IsDeleted = false,
                    IsBlocked = false,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create the role-specific profile record so the user can access its dashboard.
                // (Dashboards like Agronomist look up a matching profile row and redirect to Login if missing.)
                if (model.RoleId == 4) // Agronomist
                {
                    _context.Agronomists.Add(new Agronomist
                    {
                        UserId = user.UserId,
                        FullName = model.FullName,
                        MobileNumber = string.IsNullOrWhiteSpace(model.Phone) ? "N/A" : model.Phone,
                        Specialization = "General",
                        CreatedDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();
                }
                else if (model.RoleId == 5) // Field Officer
                {
                    _context.FieldOfficers.Add(new FieldOfficer
                    {
                        UserId = user.UserId,
                        FullName = model.FullName,
                        MobileNumber = string.IsNullOrWhiteSpace(model.Phone) ? "N/A" : model.Phone
                    });
                    await _context.SaveChangesAsync();
                }
                else if (model.RoleId == 6) // Cooperative Manager
                {
                    _context.CooperativeManagers.Add(new CooperativeManager
                    {
                        UserId = user.UserId,
                        FullName = model.FullName,
                        CooperativeName = "N/A",
                        MobileNumber = string.IsNullOrWhiteSpace(model.Phone) ? "N/A" : model.Phone
                    });
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "User created successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error creating user: {ex.Message}" });
            }
        }

        // GET: /Admin/ViewUser/{id}
        [HttpGet]
        public async Task<IActionResult> ViewUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var userDto = new
                {
                    userId = user.UserId,
                    username = user.Username,
                    email = user.Email,
                    phone = user.Phone ?? "Not provided",
                    fullName = GetUserFullName(user),
                    roleName = user.Role.RoleName,
                    status = user.IsActive ? "ACTIVE" : "PENDING",
                    joinDate = user.CreatedAt.ToString("yyyy-MM-dd"),
                    lastLogin = user.LastLogin.HasValue ? user.LastLogin.Value.ToString("yyyy-MM-dd HH:mm") : "Never"
                };

                return Json(new { success = true, user = userDto });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error fetching user: {ex.Message}" });
            }
        }

        // GET: /Admin/EditUser/{id}
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserId == id && !u.IsDeleted);

                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var roles = await GetRolesAsync();

                var editModel = new
                {
                    userId = user.UserId,
                    username = user.Username,
                    email = user.Email,
                    phone = user.Phone,
                    fullName = user.FullName ?? GetUserFullName(user),
                    roleId = user.RoleId,
                    isActive = user.IsActive,
                    roles = roles.Select(r => new { id = r.RoleId, name = r.RoleName })
                };

                return Json(new { success = true, user = editModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error fetching user: {ex.Message}" });
            }
        }

        // POST: /Admin/EditUser
        [HttpPost]
        public async Task<IActionResult> EditUser([FromBody] EditUserViewModel model)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(msg => !string.IsNullOrEmpty(msg));

                var errorMessage = errors.Any() 
                    ? string.Join(". ", errors) 
                    : "Please fill in all required fields correctly.";

                return Json(new { success = false, message = errorMessage });
            }

            try
            {
                var user = await _context.Users.FindAsync(model.UserId);
                if (user == null || user.IsDeleted)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Check if username is changed and already exists (exclude deleted users)
                if (user.Username != model.Username && await _context.Users.AnyAsync(u => u.Username == model.Username && !u.IsDeleted))
                {
                    return Json(new { success = false, message = "Username already exists" });
                }

                // Check if email is changed and already exists (exclude deleted users)
                if (user.Email != model.Email && await _context.Users.AnyAsync(u => u.Email == model.Email && !u.IsDeleted))
                {
                    return Json(new { success = false, message = "Email already exists" });
                }

                // Update user details
                user.Username = model.Username;
                user.Email = model.Email;
                user.FullName = model.FullName;
                user.Phone = model.Phone;
                user.RoleId = model.RoleId;
                user.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating user: {ex.Message}" });
            }
        }

        // POST: /Admin/DeleteUser/{id}
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Prevent deleting admin user
                var userRole = await _context.Roles.FindAsync(user.RoleId);
                if (userRole?.RoleName == "Admin")
                {
                    return Json(new { success = false, message = "Cannot delete admin user" });
                }

                // Soft delete by setting IsDeleted to true
                user.IsDeleted = true;
                user.IsActive = false; // Also deactivate
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error deleting user: {ex.Message}" });
            }
        }

        // POST: /Admin/ApproveUser/{id}
        [HttpPost]
        public async Task<IActionResult> ApproveUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (user.IsActive)
                {
                    return Json(new { success = false, message = "User is already active" });
                }

                // Approve user by setting IsActive to true
                user.IsActive = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User approved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error approving user: {ex.Message}" });
            }
        }

        // POST: /Admin/BlockUser/{id}
        [HttpPost]
        public async Task<IActionResult> BlockUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                // Prevent blocking admin user
                var userRole = await _context.Roles.FindAsync(user.RoleId);
                if (userRole?.RoleName == "Admin")
                {
                    return Json(new { success = false, message = "Cannot block admin user" });
                }

                // Restrict blocking to Field Officer, Agronomist, and Cooperative Manager roles
                if (userRole?.RoleName == "Farmer" || userRole?.RoleName == "Buyer")
                {
                    return Json(new { success = false, message = "Blocking is restricted for Farmer and Buyer roles" });
                }

                if (user.IsBlocked)
                {
                    return Json(new { success = false, message = "User is already blocked" });
                }

                // Block user by setting IsBlocked to true and IsActive to false
                user.IsBlocked = true;
                user.IsActive = false;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User blocked successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error blocking user: {ex.Message}" });
            }
        }

        // POST: /Admin/UnblockUser/{id}
        [HttpPost]
        public async Task<IActionResult> UnblockUser(int id)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.IsDeleted)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                if (!user.IsBlocked)
                {
                    return Json(new { success = false, message = "User is not blocked" });
                }

                // Unblock user by setting IsBlocked to false and IsActive to true
                user.IsBlocked = false;
                user.IsActive = true;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "User unblocked successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error unblocking user: {ex.Message}" });
            }
        }

        // GET: /Admin/ExportUsers
        [HttpGet]
        public async Task<IActionResult> ExportUsers(string? searchTerm, string? roleFilter, string? statusFilter)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Build query with same filters as UserManagement
                var query = _context.Users.Include(u => u.Role).AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(u =>
                        u.Username.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm) ||
                        (u.FullName != null && u.FullName.Contains(searchTerm)) ||
                        (u.Phone != null && u.Phone.Contains(searchTerm)));
                }

                // Role filter
                if (!string.IsNullOrWhiteSpace(roleFilter) && roleFilter != "All Roles")
                {
                    query = query.Where(u => u.Role.RoleName == roleFilter);
                }

                // Status filter
                if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "All Status")
                {
                    if (statusFilter == "ACTIVE")
                    {
                        query = query.Where(u => u.IsActive);
                    }
                    else if (statusFilter == "PENDING")
                    {
                        query = query.Where(u => !u.IsActive);
                    }
                }

                var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();

                // Generate CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Username,Full Name,Email,Phone,Role,Status,Join Date,Last Login");

                foreach (var user in users)
                {
                    csv.AppendLine($"\"{user.Username}\",\"{GetUserFullName(user)}\",\"{user.Email}\",\"{user.Phone ?? "N/A"}\",\"{user.Role.RoleName}\",\"{(user.IsActive ? "ACTIVE" : "PENDING")}\",\"{user.CreatedAt:yyyy-MM-dd}\",\"{(user.LastLogin.HasValue ? user.LastLogin.Value.ToString("yyyy-MM-dd HH:mm") : "Never")}\"");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"SmartFarm_Users_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error exporting users: {ex.Message}";
                return RedirectToAction("UserManagement");
            }
        }

        // GET: /Admin/GetRoles
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                var roles = await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
                var roleList = roles.Select(r => new { roleId = r.RoleId, roleName = r.RoleName }).ToList();
                return Json(new { success = true, roles = roleList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error loading roles: {ex.Message}" });
            }
        }

        private string GetUserFullName(User user)
        {
            if (!string.IsNullOrEmpty(user.FullName))
                return user.FullName;

            // Try to get from Farmer
            var farmer = user.Farmers.FirstOrDefault();
            if (farmer != null)
                return farmer.FullName;

            // Try to get from Buyer
            var buyer = user.Buyers.FirstOrDefault();
            if (buyer != null)
                return buyer.FullName;

            return user.Username;
        }

        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "U";

            var parts = fullName.Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0].Substring(0, 2).ToUpper();

            return fullName.Substring(0, Math.Min(2, fullName.Length)).ToUpper();
        }

        /// <summary>
        /// Simple password hashing using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Get all roles for dropdown
        /// </summary>
        private async Task<List<Role>> GetRolesAsync()
        {
            return await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
        }

        #region Farm Management

        // GET: /Admin/FarmManagement
        public async Task<IActionResult> FarmManagement(string? searchTerm, string? stateFilter, string? districtFilter, string? cropFilter)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            string? sessionName = HttpContext.Session.GetString("UserName");
            string name = !string.IsNullOrEmpty(sessionName) ? sessionName : "System Admin";
            string initials = HttpContext.Session.GetString("UserInitials") ?? "SA";

            ViewData["Title"] = "Farm Management";
            ViewData["Subtitle"] = "Overview and manage registered farms.";
            ViewData["UserRole"] = "Admin";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#dc2626";

            // Build the ViewModel with real data
            var viewModel = await BuildFarmManagementViewModelAsync(searchTerm, stateFilter, districtFilter, cropFilter);

            return View(viewModel);
        }

        /// <summary>
        /// Builds the Farm Management ViewModel with data from database
        /// </summary>
        private async Task<FarmManagementViewModel> BuildFarmManagementViewModelAsync(string? searchTerm, string? stateFilter, string? districtFilter, string? cropFilter)
        {
            var model = new FarmManagementViewModel
            {
                SearchTerm = searchTerm,
                StateFilter = stateFilter,
                DistrictFilter = districtFilter,
                CropFilter = cropFilter
            };

            // Get all farms with related data
            var farmsQuery = _context.Farms
                .Include(f => f.Farmer)
                    .ThenInclude(fr => fr.User)
                .Include(f => f.LandPlots)
                    .ThenInclude(lp => lp.CropCycles)
                        .ThenInclude(cc => cc.Crop)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                farmsQuery = farmsQuery.Where(f =>
                    f.FarmName.ToLower().Contains(searchTerm) ||
                    f.Farmer.FullName.ToLower().Contains(searchTerm) ||
                    (f.District != null && f.District.ToLower().Contains(searchTerm)) ||
                    (f.State != null && f.State.ToLower().Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(stateFilter))
            {
                farmsQuery = farmsQuery.Where(f => f.State == stateFilter);
            }

            if (!string.IsNullOrWhiteSpace(districtFilter))
            {
                farmsQuery = farmsQuery.Where(f => f.District == districtFilter);
            }

            var farms = await farmsQuery.OrderByDescending(f => f.CreatedDate).ToListAsync();

            // Apply crop filter after loading (need to check nested CropCycles)
            if (!string.IsNullOrWhiteSpace(cropFilter))
            {
                farms = farms.Where(f =>
                    f.LandPlots.Any(lp =>
                        lp.CropCycles.Any(cc => cc.Crop.CropName == cropFilter)
                    )
                ).ToList();
            }

            // Calculate statistics
            model.TotalFarms = farms.Count;

            // Map farms to DTOs
            foreach (var farm in farms)
            {
                // Calculate total area from land plots
                decimal totalArea = farm.LandPlots.Sum(lp =>
                {
                    // Convert area to hectares if needed
                    if (lp.AreaUnit?.ToLower() == "acres")
                        return lp.Area * 0.404686m; // Convert acres to hectares
                    else if (lp.AreaUnit?.ToLower() == "hectares" || lp.AreaUnit?.ToLower() == "ha")
                        return lp.Area;
                    else
                        return lp.Area; // Assume hectares by default
                });

                // Get main crop from most recent active crop cycle
                var mainCrop = farm.LandPlots
                    .SelectMany(lp => lp.CropCycles)
                    .Where(cc => cc.Status == "Active" || cc.Status == "Growing")
                    .OrderByDescending(cc => cc.SowingDate)
                    .Select(cc => cc.Crop.CropName)
                    .FirstOrDefault() ?? "N/A";

                // Get soil types (distinct)
                var soilType = farm.LandPlots
                    .Where(lp => !string.IsNullOrEmpty(lp.SoilType))
                    .Select(lp => lp.SoilType)
                    .Distinct()
                    .FirstOrDefault() ?? "N/A";

                // Determine status based on land plots
                var hasActivePlots = farm.LandPlots.Any(lp => lp.Status == "Active" || lp.Status == "Growing");
                string status = hasActivePlots ? "ACTIVE" : "INACTIVE";

                // Build location string
                string location = "";
                if (!string.IsNullOrEmpty(farm.Village))
                    location = farm.Village;
                if (!string.IsNullOrEmpty(farm.District))
                    location += (location.Length > 0 ? ", " : "") + farm.District;

                // Get owner initials
                string ownerInitials = GetInitials(farm.Farmer.FullName);

                var farmDto = new FarmDto
                {
                    FarmId = farm.FarmId,
                    FarmName = farm.FarmName,
                    OwnerName = farm.Farmer.FullName,
                    OwnerInitials = ownerInitials,
                    Location = location,
                    District = farm.District ?? "N/A",
                    State = farm.State ?? "N/A",
                    AreaHa = Math.Round(totalArea, 2),
                    AreaFormatted = $"{Math.Round(totalArea, 2)} ha",
                    MainCrop = mainCrop,
                    SoilType = soilType,
                    Status = status,
                    StatusBadgeClass = status == "ACTIVE" ? "badge-success" : "badge-secondary",
                    CreatedDate = farm.CreatedDate,
                    CreatedDateFormatted = farm.CreatedDate.ToString("MMM dd, yyyy")
                };

                model.Farms.Add(farmDto);

                // Update statistics
                if (status == "ACTIVE")
                    model.ActiveFarms++;
                else
                    model.InactiveFarms++;
            }

            // Calculate average farm size
            if (model.TotalFarms > 0)
            {
                model.AverageFarmSize = Math.Round(model.Farms.Average(f => f.AreaHa), 1);
            }

            return model;
        }

        // GET: /Admin/GetStates - Returns distinct states for filter dropdown
        [HttpGet]
        public async Task<IActionResult> GetStates()
        {
            var states = await _context.Farms
                .Where(f => !string.IsNullOrEmpty(f.State))
                .Select(f => f.State)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return Json(states);
        }

        // GET: /Admin/GetDistricts - Returns distinct districts for filter dropdown, optionally filtered by state
        [HttpGet]
        public async Task<IActionResult> GetDistricts(string? state)
        {
            var query = _context.Farms.Where(f => !string.IsNullOrEmpty(f.District));

            if (!string.IsNullOrWhiteSpace(state))
            {
                query = query.Where(f => f.State == state);
            }

            var districts = await query
                .Select(f => f.District)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return Json(districts);
        }

        // GET: /Admin/GetCrops - Returns all crops for filter dropdown
        [HttpGet]
        public async Task<IActionResult> GetCrops()
        {
            var crops = await _context.Crops
                .OrderBy(c => c.CropName)
                .Select(c => c.CropName)
                .ToListAsync();

            return Json(crops);
        }

        // GET: /Admin/GetFarmers - Returns all farmers for farm owner dropdown
        [HttpGet]
        public async Task<IActionResult> GetFarmers()
        {
            var farmers = await _context.Farmers
                .Include(f => f.User)
                .OrderBy(f => f.FullName)
                .Select(f => new FarmerOption
                {
                    FarmerId = f.FarmerId,
                    FarmerName = f.FullName
                })
                .ToListAsync();

            return Json(farmers);
        }

        // GET: /Admin/ViewFarm - Returns detailed farm information
        [HttpGet]
        public async Task<IActionResult> ViewFarm(int id)
        {
            var farm = await _context.Farms
                .Include(f => f.Farmer)
                    .ThenInclude(fr => fr.User)
                .Include(f => f.LandPlots)
                    .ThenInclude(lp => lp.CropCycles)
                        .ThenInclude(cc => cc.Crop)
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
            {
                return NotFound(new { success = false, message = "Farm not found" });
            }

            // Calculate total area from land plots
            decimal totalArea = farm.LandPlots.Sum(lp =>
            {
                if (lp.AreaUnit?.ToLower() == "acres")
                    return lp.Area * 0.404686m;
                else
                    return lp.Area;
            });

            // Get main crop
            var mainCrop = farm.LandPlots
                .SelectMany(lp => lp.CropCycles)
                .Where(cc => cc.Status == "Active" || cc.Status == "Growing")
                .OrderByDescending(cc => cc.SowingDate)
                .Select(cc => cc.Crop.CropName)
                .FirstOrDefault() ?? "N/A";

            // Get all distinct soil types
            var soilTypes = string.Join(", ", farm.LandPlots
                .Where(lp => !string.IsNullOrEmpty(lp.SoilType))
                .Select(lp => lp.SoilType)
                .Distinct());

            // Build full address
            var addressParts = new List<string>();
            if (!string.IsNullOrEmpty(farm.Village)) addressParts.Add(farm.Village);
            if (!string.IsNullOrEmpty(farm.Taluka)) addressParts.Add(farm.Taluka);
            if (!string.IsNullOrEmpty(farm.District)) addressParts.Add(farm.District);
            if (!string.IsNullOrEmpty(farm.State)) addressParts.Add(farm.State);
            if (!string.IsNullOrEmpty(farm.Pincode)) addressParts.Add(farm.Pincode);
            string fullAddress = string.Join(", ", addressParts);

            // Determine status
            var hasActivePlots = farm.LandPlots.Any(lp => lp.Status == "Active" || lp.Status == "Growing");
            string status = hasActivePlots ? "ACTIVE" : "INACTIVE";

            var viewModel = new ViewFarmViewModel
            {
                FarmId = farm.FarmId,
                FarmName = farm.FarmName,
                OwnerName = farm.Farmer.FullName,
                OwnerPhone = farm.Farmer.MobileNumber,
                OwnerEmail = farm.Farmer.User.Email,
                Village = farm.Village ?? "N/A",
                Taluka = farm.Taluka ?? "N/A",
                District = farm.District ?? "N/A",
                State = farm.State ?? "N/A",
                Pincode = farm.Pincode ?? "N/A",
                FullAddress = fullAddress,
                TotalAreaHa = Math.Round(totalArea, 2),
                TotalPlots = farm.LandPlots.Count,
                MainCrop = mainCrop,
                SoilTypes = !string.IsNullOrEmpty(soilTypes) ? soilTypes : "N/A",
                Status = status,
                CreatedDate = farm.CreatedDate,
                CreatedDateFormatted = farm.CreatedDate.ToString("MMMM dd, yyyy")
            };

            return Json(new { success = true, farm = viewModel });
        }

        // POST: /Admin/CreateFarm - Creates a new farm
        [HttpPost]
        public async Task<IActionResult> CreateFarm([FromBody] CreateFarmViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                // Verify farmer exists
                var farmer = await _context.Farmers.FindAsync(model.FarmerId);
                if (farmer == null)
                {
                    return Json(new { success = false, message = "Selected farmer not found" });
                }

                // Create new farm
                var farm = new Farm
                {
                    FarmerId = model.FarmerId,
                    FarmName = model.FarmName,
                    Village = model.Village,
                    Taluka = model.Taluka,
                    District = model.District,
                    State = model.State,
                    Pincode = model.Pincode,
                    CreatedDate = DateTime.Now
                };

                _context.Farms.Add(farm);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Farm created successfully", farmId = farm.FarmId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error creating farm: " + ex.Message });
            }
        }

        // GET: /Admin/EditFarm - Gets farm data for editing
        [HttpGet]
        public async Task<IActionResult> EditFarm(int id)
        {
            var farm = await _context.Farms
                .Include(f => f.Farmer)
                .FirstOrDefaultAsync(f => f.FarmId == id);

            if (farm == null)
            {
                return Json(new { success = false, message = "Farm not found" });
            }

            var model = new EditFarmViewModel
            {
                FarmId = farm.FarmId,
                FarmerId = farm.FarmerId,
                FarmName = farm.FarmName,
                Village = farm.Village,
                Taluka = farm.Taluka,
                District = farm.District ?? "",
                State = farm.State ?? "",
                Pincode = farm.Pincode
            };

            return Json(new { success = true, farm = model });
        }

        // POST: /Admin/EditFarm - Updates farm information
        [HttpPost]
        public async Task<IActionResult> EditFarm([FromBody] EditFarmViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = "Validation failed", errors });
            }

            try
            {
                var farm = await _context.Farms.FindAsync(model.FarmId);
                if (farm == null)
                {
                    return Json(new { success = false, message = "Farm not found" });
                }

                // Verify farmer exists
                var farmer = await _context.Farmers.FindAsync(model.FarmerId);
                if (farmer == null)
                {
                    return Json(new { success = false, message = "Selected farmer not found" });
                }

                // Update farm properties
                farm.FarmerId = model.FarmerId;
                farm.FarmName = model.FarmName;
                farm.Village = model.Village;
                farm.Taluka = model.Taluka;
                farm.District = model.District;
                farm.State = model.State;
                farm.Pincode = model.Pincode;

                _context.Farms.Update(farm);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Farm updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating farm: " + ex.Message });
            }
        }

        // POST: /Admin/DeleteFarm - Deletes a farm
        [HttpPost]
        public async Task<IActionResult> DeleteFarm(int id)
        {
            try
            {
                var farm = await _context.Farms
                    .Include(f => f.LandPlots)
                    .FirstOrDefaultAsync(f => f.FarmId == id);

                if (farm == null)
                {
                    return Json(new { success = false, message = "Farm not found" });
                }

                // Check if farm has associated land plots
                if (farm.LandPlots.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = "Cannot delete farm with associated land plots. Please remove all land plots first." 
                    });
                }

                _context.Farms.Remove(farm);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Farm deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error deleting farm: " + ex.Message });
            }
        }

        #endregion

        #region Assignment Management

        // GET: /Admin/AssignmentManagement
        public async Task<IActionResult> AssignmentManagement(string? searchTerm, string? statusFilter, string? officerFilter)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            string? sessionName = HttpContext.Session.GetString("UserName");
            string name = !string.IsNullOrEmpty(sessionName) ? sessionName : "System Admin";
            string initials = HttpContext.Session.GetString("UserInitials") ?? "SA";

            ViewData["Title"] = "Assignment Management";
            ViewData["Subtitle"] = "Monitor and assign field tasks to officers.";
            ViewData["UserRole"] = "Admin";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#dc2626";

            // Build the ViewModel with real data
            var viewModel = await BuildAssignmentManagementViewModelAsync(searchTerm, statusFilter, officerFilter);

            return View(viewModel);
        }

        /// <summary>
        /// Builds the Assignment Management ViewModel with data from database
        /// </summary>
        private async Task<AssignmentManagementViewModel> BuildAssignmentManagementViewModelAsync(string? searchTerm, string? statusFilter, string? officerFilter)
        {
            var model = new AssignmentManagementViewModel
            {
                SearchTerm = searchTerm,
                StatusFilter = statusFilter,
                OfficerFilter = officerFilter
            };

            // Get all assignments with related data - filtered to only show farmers who have actively raised a Pest Case
            var assignmentsQuery = _context.Assignments
                .Include(a => a.Farmer)
                    .ThenInclude(f => f.User)
                .Include(a => a.Farm)
                .Include(a => a.Officer)
                .Where(a => _context.PestCases.Any(pc => pc.CropCycle.LandPlot.Farm.FarmerId == a.FarmerId))
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                assignmentsQuery = assignmentsQuery.Where(a =>
                    a.Farmer.FullName.ToLower().Contains(searchTerm) ||
                    a.Farm.FarmName.ToLower().Contains(searchTerm) ||
                    a.Officer.Username.ToLower().Contains(searchTerm) ||
                    a.Task.ToLower().Contains(searchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                assignmentsQuery = assignmentsQuery.Where(a => a.Status == statusFilter);
            }

            if (!string.IsNullOrWhiteSpace(officerFilter) && int.TryParse(officerFilter, out int officerId))
            {
                assignmentsQuery = assignmentsQuery.Where(a => a.OfficerId == officerId);
            }

            var assignments = await assignmentsQuery.OrderByDescending(a => a.AssignedDate).ToListAsync();

            // Calculate statistics
            model.TotalAssignments = assignments.Count;

            // Map assignments to DTOs
            foreach (var assignment in assignments)
            {
                // Check if overdue
                bool isOverdue = assignment.DueDate < DateTime.Now && assignment.Status != "Completed";

                // Determine status (override with Overdue if applicable)
                string actualStatus = isOverdue && assignment.Status != "Completed" ? "Overdue" : assignment.Status;

                // Get officer and farmer initials
                string officerInitials = GetInitials(assignment.Officer.Username);
                string farmerInitials = GetInitials(assignment.Farmer.FullName);

                // Determine badge classes
                string priorityBadgeClass = assignment.Priority.ToLower() switch
                {
                    "high" => "badge-danger",
                    "medium" => "badge-warning",
                    "low" => "badge-info",
                    _ => "badge-secondary"
                };

                string statusBadgeClass = actualStatus.ToLower() switch
                {
                    "completed" => "badge-success",
                    "in progress" => "badge-primary",
                    "pending" => "badge-warning",
                    "overdue" => "badge-danger",
                    _ => "badge-secondary"
                };

                var assignmentDto = new AssignmentDto
                {
                    AssignmentId = assignment.AssignmentId,
                    FarmerName = assignment.Farmer.FullName,
                    FarmerInitials = farmerInitials,
                    OfficerName = assignment.Officer.Username,
                    OfficerInitials = officerInitials,
                    FarmName = assignment.Farm.FarmName,
                    Task = assignment.Task,
                    AssignedDate = assignment.AssignedDate,
                    AssignedDateFormatted = assignment.AssignedDate.ToString("yyyy-MM-dd"),
                    DueDate = assignment.DueDate,
                    DueDateFormatted = assignment.DueDate.ToString("yyyy-MM-dd"),
                    Priority = assignment.Priority,
                    PriorityBadgeClass = priorityBadgeClass,
                    Status = actualStatus,
                    StatusBadgeClass = statusBadgeClass,
                    IsOverdue = isOverdue
                };

                model.Assignments.Add(assignmentDto);

                // Update statistics
                if (actualStatus == "Pending")
                    model.PendingAssignments++;
                else if (actualStatus == "Completed")
                    model.CompletedAssignments++;
                else if (actualStatus == "Overdue")
                    model.OverdueAssignments++;
            }

            return model;
        }

        // GET: /Admin/GetAssignmentOfficers - Returns all Field Officers for dropdown
        [HttpGet]
        public async Task<IActionResult> GetAssignmentOfficers()
        {
            // Get Field Officer role ID (RoleId = 5 for Field Officer)
            var fieldOfficerRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Field Officer");

            if (fieldOfficerRole == null)
            {
                return Json(new List<OfficerOption>());
            }

            var officers = await _context.Users
                .Where(u => u.RoleId == fieldOfficerRole.RoleId && u.IsActive && !u.IsDeleted)
                .OrderBy(u => u.Username)
                .Select(u => new OfficerOption
                {
                    OfficerId = u.UserId,
                    OfficerName = u.Username
                })
                .ToListAsync();

            return Json(officers);
        }

        // GET: /Admin/GetAssignmentFarmers - Returns all farmers for dropdown
        [HttpGet]
        public async Task<IActionResult> GetAssignmentFarmers()
        {
            var farmers = await _context.Farmers
                .Include(f => f.User)
                .Where(f => !f.User.IsDeleted)
                .OrderBy(f => f.FullName)
                .Select(f => new FarmerOption
                {
                    FarmerId = f.FarmerId,
                    FarmerName = f.FullName
                })
                .ToListAsync();

            return Json(farmers);
        }

        // GET: /Admin/GetAssignmentFarms - Returns farms for selected farmer
        [HttpGet]
        public async Task<IActionResult> GetAssignmentFarms(int? farmerId)
        {
            var query = _context.Farms.AsQueryable();

            if (farmerId.HasValue && farmerId.Value > 0)
            {
                query = query.Where(f => f.FarmerId == farmerId.Value);
            }

            var farms = await query
                .OrderBy(f => f.FarmName)
                .Select(f => new FarmOption
                {
                    FarmId = f.FarmId,
                    FarmName = f.FarmName,
                    FarmerId = f.FarmerId
                })
                .ToListAsync();

            return Json(farms);
        }

        // GET: /Admin/GetAssignmentStatuses - Returns all valid status values
        [HttpGet]
        public async Task<IActionResult> GetAssignmentStatuses()
        {
            var statuses = new List<string> { "Pending", "In Progress", "Completed", "Overdue" };
            return Json(statuses);
        }

        // GET: /Admin/ViewAssignment - Returns detailed assignment information
        [HttpGet]
        public async Task<IActionResult> ViewAssignment(int id)
        {
            var assignment = await _context.Assignments
                .Include(a => a.Farmer)
                    .ThenInclude(f => f.User)
                .Include(a => a.Farm)
                .Include(a => a.Officer)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null)
            {
                return NotFound(new { success = false, message = "Assignment not found" });
            }

            // Build farm location
            var locationParts = new List<string>();
            if (!string.IsNullOrEmpty(assignment.Farm.Village)) locationParts.Add(assignment.Farm.Village);
            if (!string.IsNullOrEmpty(assignment.Farm.District)) locationParts.Add(assignment.Farm.District);
            if (!string.IsNullOrEmpty(assignment.Farm.State)) locationParts.Add(assignment.Farm.State);
            string farmLocation = string.Join(", ", locationParts);

            var viewModel = new ViewAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                FarmerName = assignment.Farmer.FullName,
                FarmerPhone = assignment.Farmer.MobileNumber,
                OfficerName = assignment.Officer.Username,
                OfficerEmail = assignment.Officer.Email,
                FarmName = assignment.Farm.FarmName,
                FarmLocation = farmLocation,
                Task = assignment.Task,
                AssignedDate = assignment.AssignedDate,
                AssignedDateFormatted = assignment.AssignedDate.ToString("MMMM dd, yyyy"),
                DueDate = assignment.DueDate,
                DueDateFormatted = assignment.DueDate.ToString("MMMM dd, yyyy"),
                Priority = assignment.Priority,
                Status = assignment.Status,
                Notes = assignment.Notes,
                CompletedDate = assignment.CompletedDate,
                CompletedDateFormatted = assignment.CompletedDate?.ToString("MMMM dd, yyyy"),
                CreatedDate = assignment.CreatedDate,
                CreatedDateFormatted = assignment.CreatedDate.ToString("MMMM dd, yyyy")
            };

            return Json(new { success = true, assignment = viewModel });
        }

        // POST: /Admin/CreateAssignment - Creates a new assignment
        [HttpPost]
        public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = "Validation failed", errors });
            }

            // Validate Due Date is after Assigned Date
            if (model.DueDate <= model.AssignedDate)
            {
                return Json(new { success = false, message = "Due Date must be after Assigned Date" });
            }

            // Verify related entities exist
            var farmerExists = await _context.Farmers.AnyAsync(f => f.FarmerId == model.FarmerId);
            if (!farmerExists)
            {
                return Json(new { success = false, message = "Selected farmer does not exist" });
            }

            var farmExists = await _context.Farms.AnyAsync(f => f.FarmId == model.FarmId && f.FarmerId == model.FarmerId);
            if (!farmExists)
            {
                return Json(new { success = false, message = "Selected farm does not exist or does not belong to the farmer" });
            }

            var officerExists = await _context.Users.AnyAsync(u => u.UserId == model.OfficerId && u.IsActive && !u.IsDeleted);
            if (!officerExists)
            {
                return Json(new { success = false, message = "Selected officer does not exist or is inactive" });
            }

            var assignment = new Assignment
            {
                FarmerId = model.FarmerId,
                FarmId = model.FarmId,
                OfficerId = model.OfficerId,
                Task = model.Task,
                AssignedDate = model.AssignedDate,
                DueDate = model.DueDate,
                Priority = model.Priority,
                Status = "Pending",
                Notes = model.Notes,
                CreatedDate = DateTime.Now
            };

            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Assignment created successfully", assignmentId = assignment.AssignmentId });
        }

        // GET: /Admin/EditAssignment - Returns assignment data for editing
        [HttpGet]
        public async Task<IActionResult> EditAssignment(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);

            if (assignment == null)
            {
                return NotFound(new { success = false, message = "Assignment not found" });
            }

            var editModel = new EditAssignmentViewModel
            {
                AssignmentId = assignment.AssignmentId,
                FarmerId = assignment.FarmerId,
                FarmId = assignment.FarmId,
                OfficerId = assignment.OfficerId,
                Task = assignment.Task,
                AssignedDate = assignment.AssignedDate,
                DueDate = assignment.DueDate,
                Priority = assignment.Priority,
                Status = assignment.Status,
                Notes = assignment.Notes
            };

            return Json(new { success = true, assignment = editModel });
        }

        // POST: /Admin/EditAssignment - Updates an existing assignment
        [HttpPost]
        public async Task<IActionResult> EditAssignment([FromBody] EditAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return Json(new { success = false, message = "Validation failed", errors });
            }

            // Validate Due Date is after Assigned Date
            if (model.DueDate <= model.AssignedDate)
            {
                return Json(new { success = false, message = "Due Date must be after Assigned Date" });
            }

            var assignment = await _context.Assignments.FindAsync(model.AssignmentId);
            if (assignment == null)
            {
                return Json(new { success = false, message = "Assignment not found" });
            }

            // Verify related entities exist
            var farmerExists = await _context.Farmers.AnyAsync(f => f.FarmerId == model.FarmerId);
            if (!farmerExists)
            {
                return Json(new { success = false, message = "Selected farmer does not exist" });
            }

            var farmExists = await _context.Farms.AnyAsync(f => f.FarmId == model.FarmId && f.FarmerId == model.FarmerId);
            if (!farmExists)
            {
                return Json(new { success = false, message = "Selected farm does not exist or does not belong to the farmer" });
            }

            var officerExists = await _context.Users.AnyAsync(u => u.UserId == model.OfficerId && u.IsActive && !u.IsDeleted);
            if (!officerExists)
            {
                return Json(new { success = false, message = "Selected officer does not exist or is inactive" });
            }

            // Update assignment
            assignment.FarmerId = model.FarmerId;
            assignment.FarmId = model.FarmId;
            assignment.OfficerId = model.OfficerId;
            assignment.Task = model.Task;
            assignment.AssignedDate = model.AssignedDate;
            assignment.DueDate = model.DueDate;
            assignment.Priority = model.Priority;
            assignment.Status = model.Status;
            assignment.Notes = model.Notes;

            // Update completed date if status changed to Completed
            if (model.Status == "Completed" && assignment.CompletedDate == null)
            {
                assignment.CompletedDate = DateTime.Now;
            }
            else if (model.Status != "Completed")
            {
                assignment.CompletedDate = null;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Assignment updated successfully" });
        }

        // POST: /Admin/DeleteAssignment - Deletes an assignment
        [HttpPost]
        public async Task<IActionResult> DeleteAssignment([FromBody] int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);

            if (assignment == null)
            {
                return Json(new { success = false, message = "Assignment not found" });
            }

            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Assignment deleted successfully" });
        }


        
        #endregion

        #region Reports Management

        // GET: /Admin/Reports
        public async Task<IActionResult> Reports(DateTime? fromDate, DateTime? toDate, string? reportTypeFilter)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            string? sessionName = HttpContext.Session.GetString("UserName");
            string name = !string.IsNullOrEmpty(sessionName) ? sessionName : "System Admin";
            string initials = HttpContext.Session.GetString("UserInitials") ?? "SA";

            ViewData["Title"] = "Reports";
            ViewData["Subtitle"] = "Generate and export system reports.";
            ViewData["UserRole"] = "Admin";
            ViewData["UserName"] = name;
            ViewData["UserInitials"] = initials;
            ViewData["RoleColor"] = "#dc2626";

            // Build the ViewModel with real data
            var viewModel = await BuildReportsViewModelAsync(fromDate, toDate, reportTypeFilter);
            viewModel.FromDate = fromDate;
            viewModel.ToDate = toDate;
            viewModel.ReportTypeFilter = reportTypeFilter;

            return View(viewModel);
        }

        private async Task<ReportsViewModel> BuildReportsViewModelAsync(DateTime? fromDate, DateTime? toDate, string? reportTypeFilter)
        {
            var model = new ReportsViewModel();
            var reportsQuery = _context.Reports.AsQueryable();
            
            if (fromDate.HasValue) reportsQuery = reportsQuery.Where(r => r.GeneratedDate >= fromDate.Value);
            if (toDate.HasValue) reportsQuery = reportsQuery.Where(r => r.GeneratedDate < toDate.Value.AddDays(1));
            if (!string.IsNullOrEmpty(reportTypeFilter)) reportsQuery = reportsQuery.Where(r => r.ReportType == reportTypeFilter);

            var reports = await reportsQuery.OrderByDescending(r => r.GeneratedDate).Take(10).ToListAsync();
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            model.TotalReports = await _context.Reports.CountAsync();
            model.GeneratedThisMonth = await _context.Reports.CountAsync(r => r.GeneratedDate >= firstDayOfMonth);
            model.ExportedReports = await _context.Reports.CountAsync(r => r.IsExported == true);
            model.PendingReports = await _context.Reports.CountAsync(r => r.Status == "Pending");

            model.Reports = reports.Select(r => new ReportDto
            {
                ReportId = r.ReportId,
                ReportName = r.ReportName,
                ReportType = r.ReportType,
                ReportTypeBadgeClass = GetReportTypeBadgeClass(r.ReportType),
                GeneratedDate = r.GeneratedDate,
                GeneratedDateFormatted = r.GeneratedDate.ToString("yyyy-MM-dd"),
                GeneratedBy = r.GeneratedBy,
                Status = r.Status,
                Description = r.Description,
                RelatedModule = r.RelatedModule,
                IsExported = r.IsExported
            }).ToList();

            return model;
        }

        private string GetReportTypeBadgeClass(string reportType)
        {
            return reportType switch
            {
                "Crop Report" => "badge-success",
                "Farm Report" => "badge-info",
                "Revenue Report" => "badge-warning",
                "Yield Report" => "badge-primary",
                "Assignment Report" => "badge-secondary",
                _ => "badge-secondary"
            };
        }


        // POST: /Admin/GenerateRealtimeReport
        [HttpPost]
        public async Task<IActionResult> GenerateRealtimeReport(string reportType)
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized access" });
            }

            try
            {
                // Create a new report entry in the Report table
                var newReport = new Report
                {
                    ReportName = $"{reportType} - {DateTime.Now:MMM dd, yyyy}",
                    ReportType = reportType,
                    GeneratedDate = DateTime.Now,
                    GeneratedBy = HttpContext.Session.GetString("UserName") ?? "Admin",
                    Status = "Generated",
                    Description = $"Real-time {reportType.ToLower()} generated from live database",
                    IsExported = false,
                    CreatedDate = DateTime.Now
                };

                // Set RelatedModule based on report type
                newReport.RelatedModule = reportType switch
                {
                    "Crop Report" => "Crop",
                    "Farm Report" => "Farm",
                    "Revenue Report" => "Revenue",
                    "Yield Report" => "Crop",
                    "Assignment Report" => "Assignment",
                    _ => null
                };

                _context.Reports.Add(newReport);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"{reportType} generated successfully!",
                    reportId = newReport.ReportId,
                    reportName = newReport.ReportName,
                    generatedDate = newReport.GeneratedDate.ToString("yyyy-MM-dd HH:mm")
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error generating report: {ex.Message}"
                });
            }
        }


        // GET: /Admin/ViewReport/{id}
        [HttpGet]
        public async Task<IActionResult> ViewReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return Json(new { success = false, message = "Report not found" });
            string reportContent = await GenerateReportContentAsync(report);
            var reportData = new
            {
                reportId = report.ReportId, reportName = report.ReportName, reportType = report.ReportType,
                generatedDate = report.GeneratedDate.ToString("yyyy-MM-dd HH:mm"), generatedBy = report.GeneratedBy,
                status = report.Status, relatedModule = report.RelatedModule, description = report.Description,
                content = reportContent, isExported = report.IsExported, exportedDate = report.ExportedDate?.ToString("yyyy-MM-dd HH:mm")
            };
            return Json(new { success = true, report = reportData });
        }

        // GET: /Admin/DownloadReport/{id}
        [HttpGet]
        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();
            string reportContent = await GenerateFullReportContentAsync(report);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(reportContent);
            report.IsExported = true;
            report.ExportedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            string fileName = $"{report.ReportName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.txt";
            return File(bytes, "text/plain", fileName);
        }

        // GET: /Admin/ExportReport
        [HttpGet]
        public async Task<IActionResult> ExportReport(DateTime? fromDate, DateTime? toDate, string? reportTypeFilter)
        {
            // Query reports with the same filters as the Reports page
            var reportsQuery = _context.Reports.AsQueryable();

            if (fromDate.HasValue)
                reportsQuery = reportsQuery.Where(r => r.GeneratedDate >= fromDate.Value);

            if (toDate.HasValue)
                reportsQuery = reportsQuery.Where(r => r.GeneratedDate < toDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(reportTypeFilter))
                reportsQuery = reportsQuery.Where(r => r.ReportType == reportTypeFilter);

            var reports = await reportsQuery.OrderByDescending(r => r.GeneratedDate).ToListAsync();

            if (!reports.Any())
            {
                // Return an empty CSV with headers
                var emptyContent = "No reports found matching the selected filters.";
                var emptyBytes = System.Text.Encoding.UTF8.GetBytes(emptyContent);
                return File(emptyBytes, "text/plain", $"Reports_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }

            // Generate CSV content
            var csv = new System.Text.StringBuilder();

            // CSV Header
            csv.AppendLine("Report ID,Report Name,Report Type,Generated Date,Generated By,Status,Related Module,Description,Is Exported,Exported Date");

            // CSV Rows
            foreach (var report in reports)
            {
                csv.AppendLine($"{report.ReportId}," +
                              $"\"{EscapeCsvField(report.ReportName)}\"," +
                              $"\"{EscapeCsvField(report.ReportType)}\"," +
                              $"\"{report.GeneratedDate:yyyy-MM-dd HH:mm}\"," +
                              $"\"{EscapeCsvField(report.GeneratedBy)}\"," +
                              $"\"{EscapeCsvField(report.Status)}\"," +
                              $"\"{EscapeCsvField(report.RelatedModule ?? "")}\"," +
                              $"\"{EscapeCsvField(report.Description ?? "")}\"," +
                              $"{(report.IsExported ? "Yes" : "No")}," +
                              $"\"{(report.ExportedDate.HasValue ? report.ExportedDate.Value.ToString("yyyy-MM-dd HH:mm") : "")}\"");
            }

            // Update IsExported and ExportedDate for all exported reports
            foreach (var report in reports)
            {
                report.IsExported = true;
                report.ExportedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            // Return CSV file
            var csvBytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            string fileName = $"Reports_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(csvBytes, "text/csv", fileName);
        }

        /// <summary>
        /// Helper method to escape CSV fields that contain commas, quotes, or newlines
        /// </summary>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field))
                return string.Empty;

            // Escape double quotes by doubling them
            field = field.Replace("\"", "\"\"");

            // Remove newlines and carriage returns
            field = field.Replace("\r", " ").Replace("\n", " ");

            return field;
        }

        private async Task<string> GenerateReportContentAsync(Report report)
        {
            string content = $"<h5>{report.ReportName}</h5><p class='text-muted'>{report.Description}</p><hr>";
            switch (report.ReportType)
            {
                case "Crop Report": content += await GenerateCropReportContentAsync(); break;
                case "Farm Report": content += await GenerateFarmReportContentAsync(); break;
                case "Revenue Report": content += await GenerateRevenueReportContentAsync(); break;
                case "Yield Report": content += await GenerateYieldReportContentAsync(); break;
                case "Assignment Report": content += await GenerateAssignmentReportContentAsync(); break;
                default: content += "<p>Report content not available.</p>"; break;
            }
            return content;
        }

        private async Task<string> GenerateFullReportContentAsync(Report report)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("======================================");
            sb.AppendLine($"REPORT: {report.ReportName}");
            sb.AppendLine("======================================");
            sb.AppendLine($"Type: {report.ReportType}");
            sb.AppendLine($"Generated: {report.GeneratedDate:yyyy-MM-dd HH:mm}");
            sb.AppendLine($"Generated By: {report.GeneratedBy}");
            sb.AppendLine($"Description: {report.Description}");
            sb.AppendLine("======================================\n");

            switch (report.ReportType)
            {
                case "Crop Report": sb.AppendLine(await GenerateCropReportTextAsync()); break;
                case "Farm Report": sb.AppendLine(await GenerateFarmReportTextAsync()); break;
                case "Revenue Report": sb.AppendLine(await GenerateRevenueReportTextAsync());break;
                case "Yield Report": sb.AppendLine(await GenerateYieldReportTextAsync()); break;
                case "Assignment Report": sb.AppendLine(await GenerateAssignmentReportTextAsync()); break;
            }

            sb.AppendLine("\n======================================");
            sb.AppendLine($"Report generated on: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("SmartFarm Platform - Admin Dashboard");
            sb.AppendLine("======================================");
            return sb.ToString();
        }

        private async Task<string> GenerateCropReportContentAsync()
        {
            var cropStats = await _context.CropCycles.Include(cc => cc.Crop).GroupBy(cc => cc.Crop.CropName)
                .Select(g => new { CropName = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).Take(5).ToListAsync();
            var content = "<h6>Top 5 Crops by Cycle Count</h6><ul class='list-unstyled'>";
            foreach (var crop in cropStats) content += $"<li><strong>{crop.CropName}:</strong> {crop.Count} cycles</li>";
            content += "</ul>";
            return content;
        }

        private async Task<string> GenerateCropReportTextAsync()
        {
            var cropStats = await _context.CropCycles.Include(cc => cc.Crop).GroupBy(cc => cc.Crop.CropName)
                .Select(g => new { CropName = g.Key, Count = g.Count() }).OrderByDescending(x => x.Count).Take(5).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("TOP 5 CROPS BY CYCLE COUNT:");
            foreach (var crop in cropStats) sb.AppendLine($"  - {crop.CropName}: {crop.Count} cycles");
            return sb.ToString();
        }

        private async Task<string> GenerateFarmReportContentAsync()
        {
            var farmCount = await _context.Farms.CountAsync();
            var stateCount = await _context.Farms.Select(f => f.State).Distinct().CountAsync();
            // Note: Farm entity doesn't have TotalArea property
            var content = "<h6>Farm Statistics</h6><p><strong>Total Farms:</strong> " + farmCount + "</p>";
            content += $"<p><strong>States Covered:</strong> {stateCount}</p>";
            return content;
        }

        private async Task<string> GenerateFarmReportTextAsync()
        {
            var farmCount = await _context.Farms.CountAsync();
            var stateCount = await _context.Farms.Select(f => f.State).Distinct().CountAsync();
            // Note: Farm entity doesn't have TotalArea property
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("FARM STATISTICS:");
            sb.AppendLine($"  Total Farms: {farmCount}");
            sb.AppendLine($"  States Covered: {stateCount}");
            return sb.ToString();
        }

        private async Task<string> GenerateRevenueReportContentAsync()
        {
            var totalHarvests = await _context.Harvests.CountAsync();
            var totalQuantity = await _context.Harvests.SumAsync(h => (decimal?)h.ActualQuantity) ?? 0;
            return $"<h6>Revenue Metrics</h6><p><strong>Total Harvests:</strong> {totalHarvests}</p><p><strong>Total Quantity:</strong> {totalQuantity:F2} kg</p>";
        }

        private async Task<string> GenerateRevenueReportTextAsync()
        {
            var totalHarvests = await _context.Harvests.CountAsync();
            var totalQuantity = await _context.Harvests.SumAsync(h => (decimal?)h.ActualQuantity) ?? 0;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("REVENUE METRICS:");
            sb.AppendLine($"  Total Harvests: {totalHarvests}");
            sb.AppendLine($"  Total Quantity: {totalQuantity:F2} kg");
            return sb.ToString();
        }

        private async Task<string> GenerateYieldReportContentAsync()
        {
            var yieldData = await _context.Harvests.Include(h => h.CropCycle).ThenInclude(cc => cc.Crop)
                .GroupBy(h => h.CropCycle.Crop.CropName).Select(g => new { CropName = g.Key, TotalQty = g.Sum(h => h.ActualQuantity), Count = g.Count() })
                .OrderByDescending(x => x.TotalQty).Take(5).ToListAsync();
            var content = "<h6>Top 5 Crops by Yield</h6><ul class='list-unstyled'>";
            foreach (var crop in yieldData)
            {
                var avg = crop.Count > 0 ? crop.TotalQty / crop.Count : 0;
                content += $"<li><strong>{crop.CropName}:</strong> {crop.TotalQty:F2} kg (Avg: {avg:F2} kg)</li>";
            }
            content += "</ul>";
            return content;
        }

        private async Task<string> GenerateYieldReportTextAsync()
        {
            var yieldData = await _context.Harvests.Include(h => h.CropCycle).ThenInclude(cc => cc.Crop)
                .GroupBy(h => h.CropCycle.Crop.CropName).Select(g => new { CropName = g.Key, TotalQty = g.Sum(h => h.ActualQuantity), Count = g.Count() })
                .OrderByDescending(x => x.TotalQty).Take(5).ToListAsync();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("TOP 5 CROPS BY YIELD:");
            foreach (var crop in yieldData)
            {
                var avg = crop.Count > 0 ? crop.TotalQty / crop.Count : 0;
                sb.AppendLine($"  - {crop.CropName}: {crop.TotalQty:F2} kg total (Avg: {avg:F2} kg)");
            }
            return sb.ToString();
        }

        private async Task<string> GenerateAssignmentReportContentAsync()
        {
            var stats = await _context.Assignments.GroupBy(a => a.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            var total = stats.Sum(s => s.Count);
            var completed = stats.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
            var rate = total > 0 ? (completed * 100.0 / total) : 0;
            var content = $"<h6>Assignment Statistics</h6><p><strong>Total:</strong> {total}</p><p><strong>Completed:</strong> {completed} ({rate:F1}%)</p><ul class='list-unstyled'>";
            foreach (var stat in stats) content += $"<li>{stat.Status}: {stat.Count}</li>";
            content += "</ul>";
            return content;
        }

        private async Task<string> GenerateAssignmentReportTextAsync()
        {
            var stats = await _context.Assignments.GroupBy(a => a.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync();
            var total = stats.Sum(s => s.Count);
            var completed = stats.FirstOrDefault(s => s.Status == "Completed")?.Count ?? 0;
            var rate = total > 0 ? (completed * 100.0 / total) : 0;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ASSIGNMENT STATISTICS:");
            sb.AppendLine($"  Total Assignments: {total}");
            sb.AppendLine($"  Completed: {completed} ({rate:F1}%)");
            sb.AppendLine("  Status Breakdown:");
            foreach (var stat in stats) sb.AppendLine($"    - {stat.Status}: {stat.Count}");
            return sb.ToString();
        }

        #endregion

        #region My Profile

        /// <summary>
        /// GET: Admin/MyProfile
        /// Display the My Profile page with admin profile information
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            // Check if user is logged in and is Admin
            string? role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Get UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                // Fallback: try to get from username
                string? username = HttpContext.Session.GetString("UserUsername");
                if (string.IsNullOrEmpty(username))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var userByUsername = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (userByUsername == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                userId = userByUsername.UserId;
                HttpContext.Session.SetInt32("UserId", userId.Value);
            }

            // Load user and profile data
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var adminProfile = await _context.AdminProfiles
                .FirstOrDefaultAsync(ap => ap.UserId == userId.Value);

            // Set ViewData for layout
            ViewData["Title"] = "My Profile";
            ViewData["Subtitle"] = "View and manage your account details.";
            ViewData["UserRole"] = user.Role?.RoleName ?? "Admin";
            ViewData["UserName"] = user.FullName ?? "System Admin";
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials") ?? "SA";
            ViewData["RoleColor"] = "#dc2626";

            // Build ViewModel
            var model = new MyProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role?.RoleName ?? "Administrator",
                IsActive = user.IsActive,
                FirstName = adminProfile?.FirstName ?? "",
                LastName = adminProfile?.LastName ?? "",
                Email = user.Email,
                PhoneNumber = user.Phone ?? "",
                EmployeeId = adminProfile?.EmployeeId,
                Department = adminProfile?.Department,
                Address = adminProfile?.Address,
                City = adminProfile?.City,
                State = adminProfile?.State,
                PinCode = adminProfile?.PinCode
            };

            return View(model);
        }

        /// <summary>
        /// POST: Admin/UpdateMyProfile
        /// Update admin profile information with validation
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMyProfile(MyProfileViewModel model)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return Json(new { success = false, message = "Unauthorized. Please log in." });
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Load user
                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found." });
                }

                // Update user basic information
                user.Email = model.Email.Trim();
                user.Phone = model.PhoneNumber.Trim();
                user.FullName = $"{model.FirstName.Trim()} {model.LastName.Trim()}";

                // Load or create AdminProfile
                var adminProfile = await _context.AdminProfiles
                    .FirstOrDefaultAsync(ap => ap.UserId == userId.Value);

                if (adminProfile == null)
                {
                    // Create new profile
                    adminProfile = new AdminProfile
                    {
                        UserId = userId.Value,
                        FirstName = model.FirstName.Trim(),
                        LastName = model.LastName.Trim(),
                        EmployeeId = model.EmployeeId?.Trim(),
                        Department = model.Department?.Trim(),
                        Address = model.Address?.Trim(),
                        City = model.City?.Trim(),
                        State = model.State?.Trim(),
                        PinCode = model.PinCode?.Trim(),
                        UpdatedAt = DateTime.Now
                    };
                    _context.AdminProfiles.Add(adminProfile);
                }
                else
                {
                    // Update existing profile
                    adminProfile.FirstName = model.FirstName.Trim();
                    adminProfile.LastName = model.LastName.Trim();
                    adminProfile.EmployeeId = model.EmployeeId?.Trim();
                    adminProfile.Department = model.Department?.Trim();
                    adminProfile.Address = model.Address?.Trim();
                    adminProfile.City = model.City?.Trim();
                    adminProfile.State = model.State?.Trim();
                    adminProfile.PinCode = model.PinCode?.Trim();
                    adminProfile.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                // Update session
                HttpContext.Session.SetString("UserName", user.FullName);

                // Update initials in session
                var initials = $"{model.FirstName[0]}{model.LastName[0]}".ToUpper();
                HttpContext.Session.SetString("UserInitials", initials);

                return Json(new { 
                    success = true, 
                    message = "Profile updated successfully!",
                    fullName = user.FullName,
                    initials = initials
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error updating profile: {ex.Message}" });
            }
        }

        #endregion
    }
}
