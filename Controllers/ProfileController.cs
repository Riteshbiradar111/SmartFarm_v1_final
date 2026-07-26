using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using SmartFarmMVC.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class ProfileController : Controller
    {
        private readonly SmartFarmDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(SmartFarmDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Helper to validate Farmer Session
        private Smart_Farm_and_Crop_Yeild_Management_System.Models.Farmer? GetActiveFarmer()
        {
            var role = HttpContext.Session.GetString("UserRole");
            var username = HttpContext.Session.GetString("UserUsername");

            if (role != "Farmer" || string.IsNullOrEmpty(username))
            {
                return null;
            }

            return _context.Farmers
                .Include(f => f.User)
                .FirstOrDefault(f => f.User.Username == username);
        }

        // GET: /Profile
        // Display farmer profile with option to edit
        public IActionResult Index()
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            // Map farmer data to view model
            var model = new FarmerProfileViewModel
            {
                FullName = farmer.FullName,
                MobileNumber = farmer.MobileNumber,
                Address = farmer.Address,
                Village = farmer.Village,
                Taluka = farmer.Taluka,
                District = farmer.District,
                State = farmer.State,
                PinCode = farmer.PinCode,
                Gender = farmer.Gender,
                DateOfBirth = farmer.DateOfBirth,
                EmergencyContact = farmer.EmergencyContact
            };

            ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
            ViewBag.Email = farmer.User.Email;
            ViewBag.Username = farmer.User.Username;

            ViewData["UserName"] = HttpContext.Session.GetString("UserName");
            ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
            ViewData["UserRole"] = "Farmer";
            ViewData["Title"] = "My Profile";

            return View(model);
        }

        // POST: /Profile
        // Update farmer profile information
        [HttpPost]
        public IActionResult Index(FarmerProfileViewModel model)
        {
            var farmer = GetActiveFarmer();
            if (farmer == null) return RedirectToAction("Login", "Auth");

            // Remove password fields from validation if not changing password
            if (string.IsNullOrEmpty(model.CurrentPassword) && 
                string.IsNullOrEmpty(model.NewPassword) && 
                string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                ModelState.Remove("CurrentPassword");
                ModelState.Remove("NewPassword");
                ModelState.Remove("ConfirmNewPassword");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                ViewBag.Email = farmer.User.Email;
                ViewBag.Username = farmer.User.Username;

                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                ViewData["Title"] = "My Profile";

                return View(model);
            }

            try
            {
                // Update farmer details
                farmer.FullName = model.FullName;
                farmer.MobileNumber = model.MobileNumber;
                farmer.Address = model.Address;
                farmer.Village = model.Village;
                farmer.Taluka = model.Taluka;
                farmer.District = model.District;
                farmer.State = model.State;
                farmer.PinCode = model.PinCode;
                farmer.Gender = model.Gender;
                farmer.DateOfBirth = model.DateOfBirth;
                farmer.EmergencyContact = model.EmergencyContact;

                // Handle profile picture upload
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    // Validate file type
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProfilePicture", "Only image files (jpg, jpeg, png, gif) are allowed.");

                        ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                        ViewBag.Email = farmer.User.Email;
                        ViewBag.Username = farmer.User.Username;

                        ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                        ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                        ViewData["UserRole"] = "Farmer";

                        return View(model);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(farmer.ProfilePicturePath))
                    {
                        var oldFilePath = Path.Combine(_environment.WebRootPath, farmer.ProfilePicturePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Create uploads directory if it doesn't exist
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Generate unique filename
                    var uniqueFileName = $"{Guid.NewGuid()}_{model.ProfilePicture.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        model.ProfilePicture.CopyTo(fileStream);
                    }

                    // Update farmer record with new path
                    farmer.ProfilePicturePath = $"/uploads/profiles/{uniqueFileName}";
                }

                // Handle password change if requested
                if (!string.IsNullOrEmpty(model.CurrentPassword) && 
                    !string.IsNullOrEmpty(model.NewPassword))
                {
                    // Verify current password
                    if (farmer.User.PasswordHash != model.CurrentPassword)
                    {
                        ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");

                        ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                        ViewBag.Email = farmer.User.Email;
                        ViewBag.Username = farmer.User.Username;

                        ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                        ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                        ViewData["UserRole"] = "Farmer";

                        return View(model);
                    }

                    // Update password
                    farmer.User.PasswordHash = model.NewPassword;
                }

                _context.SaveChanges();

                // Update session with new full name
                HttpContext.Session.SetString("UserName", farmer.FullName);
                HttpContext.Session.SetString("UserInitials", GetInitials(farmer.FullName));

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Error updating profile: " + ex.Message;

                ViewBag.ProfilePicturePath = farmer.ProfilePicturePath;
                ViewBag.Email = farmer.User.Email;
                ViewBag.Username = farmer.User.Username;

                ViewData["UserName"] = HttpContext.Session.GetString("UserName");
                ViewData["UserInitials"] = HttpContext.Session.GetString("UserInitials");
                ViewData["UserRole"] = "Farmer";
                ViewData["Title"] = "My Profile";

                return View(model);
            }
        }

        // Helper: Get first two initials from a full name
        private string GetInitials(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
                return "U";

            string[] parts = fullName.Split(new char[] { ' ', '_' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
                return (parts[0][0].ToString() + parts[1][0].ToString()).ToUpper();

            if (parts.Length == 1 && parts[0].Length >= 2)
                return parts[0].Substring(0, 2).ToUpper();

            return "U";
        }
    }
}
