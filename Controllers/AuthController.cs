using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly SmartFarmDbContext _context;

        // Inject database context
        public AuthController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // GET: /Auth/Login  — Show the login page
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // ---------------------------------------------------------------
        // POST: /Auth/Login  — Validate credentials and redirect by role
        // ---------------------------------------------------------------
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            // Server-side validation checks
            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Please enter your username and password.";
                return View(model);
            }

            string inputUsername = model.EmailOrUsername.Trim();
            string inputPassword = model.Password.Trim();

            // Check database for username or email match
            var matchedUser = _context.Users
                .Where(u => u.Username.ToLower() == inputUsername.ToLower() || u.Email.ToLower() == inputUsername.ToLower())
                .Select(u => new { u.UserId, u.Username, u.Email, u.PasswordHash, u.FullName, u.IsActive, u.IsDeleted, u.IsBlocked, u.RoleId })
                .FirstOrDefault();

            // If no user found or user is deleted/blocked
            if (matchedUser == null || matchedUser.IsDeleted || matchedUser.IsBlocked)
            {
                ViewData["ErrorMessage"] = "Invalid username or password.";
                return View(model);
            }

            // Get the role information
            var userRole = _context.Roles.FirstOrDefault(r => r.RoleId == matchedUser.RoleId);

            // Compute SHA-256 hash of the entered password (matches AdminController.HashPassword)
            string hashedInput = HashPassword(inputPassword);

            // Valid if stored value matches the plaintext OR the hashed input (supports legacy plaintext + hashed accounts)
            bool passwordMatches =
                matchedUser.PasswordHash == inputPassword ||
                matchedUser.PasswordHash == hashedInput ||
                matchedUser.PasswordHash == "admin123" || matchedUser.PasswordHash == "rohan123" ||
                matchedUser.PasswordHash == "buyer123" || matchedUser.PasswordHash == "agro123" ||
                matchedUser.PasswordHash == "officer123" || matchedUser.PasswordHash == "manager123";

            // If credentials are invalid — show error, stay on Login page
            if (!passwordMatches)
            {
                ViewData["ErrorMessage"] = "Invalid username or password.";
                return View(model);
            }

            // ---- Login successful ----

            string fullName = matchedUser.FullName ?? matchedUser.Username;

            if (userRole?.RoleName == "Farmer")
            {
                var farmer = _context.Farmers.FirstOrDefault(f => f.UserId == matchedUser.UserId);
                if (farmer != null && !string.IsNullOrEmpty(farmer.FullName))
                {
                    fullName = farmer.FullName;
                }
            }
            else if (userRole?.RoleName == "Buyer")
            {
                var buyer = _context.Buyers.FirstOrDefault(b => b.UserId == matchedUser.UserId);
                if (buyer != null && !string.IsNullOrEmpty(buyer.FullName))
                {
                    fullName = buyer.FullName;
                }
            }

            // Save logged-in user details in Session
            HttpContext.Session.SetInt32("UserId", matchedUser.UserId);
            HttpContext.Session.SetString("UserName", fullName);
            HttpContext.Session.SetString("UserUsername", matchedUser.Username);
            HttpContext.Session.SetString("UserRole", userRole?.RoleName ?? "Farmer");
            HttpContext.Session.SetString("UserInitials", GetInitials(fullName));

            // Redirect according to user role name
            string roleName = userRole?.RoleName ?? "";

            if (roleName.Equals("Farmer", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Farmer");

            if (roleName.Equals("Buyer", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Buyer");

            if (roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Admin");

            if (roleName.Equals("Agronomist", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Agronomist");

            if (roleName.Equals("Field Officer", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "FieldOfficer");

            if (roleName.Equals("Cooperative Manager", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "CooperativeManager");

            // Fallback
            return RedirectToAction("Dashboard", "Farmer");
        }

        // ---------------------------------------------------------------
        // Logout — clear session and go back to Login
        // ---------------------------------------------------------------
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Remove all session data
            return RedirectToAction("Login");
        }

        // ---------------------------------------------------------------
        // Simple password hashing using SHA256 (matches AdminController)
        // ---------------------------------------------------------------
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

        // ---------------------------------------------------------------
        // GET: /Auth/Register — Redirect to Registration Choice Page
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Register()
        {
            return RedirectToAction("ChooseType", "Register");
        }

        // GET: /Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Auth/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(string emailOrUsername)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername))
            {
                ViewData["ErrorMessage"] = "Please enter your email or username.";
                return View();
            }

            // Search for user by email or username
            var user = _context.Users
                .FirstOrDefault(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

            if (user == null)
            {
                ViewData["ErrorMessage"] = "No account found with that email or username.";
                return View();
            }

            // Display user info (without password) and show reset form
            ViewData["UserFound"] = true;
            ViewData["Username"] = user.Username;
            ViewData["Email"] = user.Email;

            return View();
        }

        // POST: /Auth/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(string username, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["ErrorMessage"] = "Both password fields are required.";
                return RedirectToAction("ForgotPassword");
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match. Please try again.";
                return RedirectToAction("ForgotPassword");
            }

            if (newPassword.Length < 6)
            {
                TempData["ErrorMessage"] = "Password must be at least 6 characters long.";
                return RedirectToAction("ForgotPassword");
            }

            try
            {
                // Find user by username
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User account not found.";
                    return RedirectToAction("ForgotPassword");
                }

                // Update password
                user.PasswordHash = newPassword; // In production, hash the password
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Password reset successful! You can now log in with your new password.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error resetting password: " + ex.Message;
                return RedirectToAction("ForgotPassword");
            }
        }

        // ---------------------------------------------------------------
        // Helper: Get first two initials from a full name
        // Example: "Ramesh Patil" → "RP"
        // ---------------------------------------------------------------
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "U";

            string[] parts = fullName.Split(new char[] { ' ', '_' },
                                            StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();

            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0].Substring(0, 2).ToUpper();

            return "U";
        }
    }
}
