using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using Smart_Farm_and_Crop_Yeild_Management_System.Models;
using Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Controllers
{
    public class RegisterController : Controller
    {
        private readonly SmartFarmDbContext _context;

        // Inject database context
        public RegisterController(SmartFarmDbContext context)
        {
            _context = context;
        }

        // ---------------------------------------------------------------
        // GET: /Register/ChooseType
        // Show the "Choose Registration Type" page with 2 cards
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult ChooseType()
        {
            ViewData["Title"] = "Choose Registration Type";
            return View();
        }

        // ---------------------------------------------------------------
        // GET: /Register/Farmer
        // Show the Farmer Registration form
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Farmer()
        {
            ViewData["Title"] = "Farmer Registration";
            return View(new FarmerRegistrationViewModel());
        }

        // ---------------------------------------------------------------
        // POST: /Register/Farmer
        // Database-driven Farmer registration with validations
        // ---------------------------------------------------------------
        [HttpPost]
        public IActionResult Farmer(FarmerRegistrationViewModel model)
        {
            // Check all required fields are filled in first to show the generic message
            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.MobileNumber) ||
                string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword) ||
                string.IsNullOrWhiteSpace(model.Village) ||
                string.IsNullOrWhiteSpace(model.District) ||
                string.IsNullOrWhiteSpace(model.State) ||
                string.IsNullOrWhiteSpace(model.Pincode))
            {
                ViewData["Title"] = "Farmer Registration";
                ViewData["ErrorMessage"] = "Please fill in all required fields.";
                return View(model);
            }

            // Server-side validation check
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Farmer Registration";
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault();
                ViewData["ErrorMessage"] = firstError != null ? firstError.ErrorMessage : "Please fill in all required fields correctly.";
                return View(model);
            }

            try
            {
                // Check whether username already exists
                var isUsernameDup = _context.Users.Any(u => u.Username == model.Username.Trim());
                if (isUsernameDup)
                {
                    ViewData["Title"] = "Farmer Registration";
                    ViewData["ErrorMessage"] = "Username already exists.";
                    return View(model);
                }

                // Check duplicate Email in Users table
                var isEmailDup = _context.Users.Any(u => u.Email == model.Email.Trim());
                if (isEmailDup)
                {
                    ViewData["Title"] = "Farmer Registration";
                    ViewData["ErrorMessage"] = "Email already registered.";
                    return View(model);
                }

                // Check duplicate Mobile Number in Farmers table
                var isMobileDup = _context.Farmers.Any(f => f.MobileNumber == model.MobileNumber.Trim());
                if (isMobileDup)
                {
                    ViewData["Title"] = "Farmer Registration";
                    ViewData["ErrorMessage"] = "Mobile number already registered.";
                    return View(model);
                }

                // Check password matches confirm password (backup validation)
                if (model.Password != model.ConfirmPassword)
                {
                    ViewData["Title"] = "Farmer Registration";
                    ViewData["ErrorMessage"] = "Passwords do not match.";
                    return View(model);
                }

                // Save user information
                var newUser = new User
                {
                    Username = model.Username.Trim(),
                    PasswordHash = model.Password, // ALIGNED WITH DB: PasswordHash
                    Email = model.Email.Trim(), // Saved in Users table
                    Phone = model.MobileNumber.Trim(), // Saved in Users table
                    RoleId = 2, // ALIGNED WITH DB: Role ID 2 is Farmer
                    IsActive = true,
                    IsDeleted = false,
                    IsBlocked = false,
                    CreatedAt = DateTime.Now // ALIGNED WITH DB: CreatedAt
                };

                _context.Users.Add(newUser);
                _context.SaveChanges(); // Saves to database and populates newUser.UserId

                // Save farmer details
                var newFarmer = new Smart_Farm_and_Crop_Yeild_Management_System.Models.Farmer
                {
                    UserId = newUser.UserId,
                    FullName = model.FullName.Trim(),
                    MobileNumber = model.MobileNumber.Trim(),
                    Address = model.Address?.Trim() ?? "",
                    Village = model.Village.Trim(),
                    District = model.District.Trim(),
                    State = model.State.Trim(),
                    PinCode = model.Pincode.Trim()
                };

                _context.Farmers.Add(newFarmer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration completed successfully.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ViewData["Title"] = "Farmer Registration";
                var deepest = ex;
                while (deepest.InnerException != null)
                {
                    deepest = deepest.InnerException;
                }
                ViewData["ErrorMessage"] = "An error occurred during registration. Details: " + deepest.Message;
                return View(model);
            }
        }

        // ---------------------------------------------------------------
        // GET: /Register/Buyer
        // Show the Buyer Registration form
        // ---------------------------------------------------------------
        [HttpGet]
        public IActionResult Buyer()
        {
            ViewData["Title"] = "Buyer Registration";
            return View(new BuyerRegistrationViewModel());
        }

        // ---------------------------------------------------------------
        // POST: /Register/Buyer
        // Database-driven Buyer registration with validations
        // ---------------------------------------------------------------
        [HttpPost]
        public IActionResult Buyer(BuyerRegistrationViewModel model)
        {
            // Check all required fields are filled in first to show the generic message
            if (string.IsNullOrWhiteSpace(model.FullName) ||
                string.IsNullOrWhiteSpace(model.Email) ||
                string.IsNullOrWhiteSpace(model.MobileNumber) ||
                string.IsNullOrWhiteSpace(model.Username) ||
                string.IsNullOrWhiteSpace(model.Password) ||
                string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ViewData["Title"] = "Buyer Registration";
                ViewData["ErrorMessage"] = "Please fill in all required fields.";
                return View(model);
            }

            // Server-side validation check
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Buyer Registration";
                var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault();
                ViewData["ErrorMessage"] = firstError != null ? firstError.ErrorMessage : "Please fill in all required fields correctly.";
                return View(model);
            }

            try
            {
                // Check whether username already exists
                var isUsernameDup = _context.Users.Any(u => u.Username == model.Username.Trim());
                if (isUsernameDup)
                {
                    ViewData["Title"] = "Buyer Registration";
                    ViewData["ErrorMessage"] = "Username already exists.";
                    return View(model);
                }

                // Check duplicate Email in Users table
                var isEmailDup = _context.Users.Any(u => u.Email == model.Email.Trim());
                if (isEmailDup)
                {
                    ViewData["Title"] = "Buyer Registration";
                    ViewData["ErrorMessage"] = "Email already registered.";
                    return View(model);
                }

                // Check duplicate Mobile Number in Buyers table
                var isMobileDup = _context.Buyers.Any(b => b.MobileNumber == model.MobileNumber.Trim());
                if (isMobileDup)
                {
                    ViewData["Title"] = "Buyer Registration";
                    ViewData["ErrorMessage"] = "Mobile number already registered.";
                    return View(model);
                }

                // Check password matches confirm password (backup validation)
                if (model.Password != model.ConfirmPassword)
                {
                    ViewData["Title"] = "Buyer Registration";
                    ViewData["ErrorMessage"] = "Passwords do not match.";
                    return View(model);
                }

                // Save user information
                var newUser = new User
                {
                    Username = model.Username.Trim(),
                    PasswordHash = model.Password, // ALIGNED WITH DB: PasswordHash
                    Email = model.Email.Trim(), // Saved in Users table
                    Phone = model.MobileNumber.Trim(), // Saved in Users table
                    RoleId = 3, // ALIGNED WITH DB: Role ID 3 is Buyer
                    IsActive = true,
                    CreatedAt = DateTime.Now // ALIGNED WITH DB: CreatedAt
                };

                _context.Users.Add(newUser);
                _context.SaveChanges(); // Saves to database and populates newUser.UserId

                // Save buyer details mapping to SSMS columns
                var newBuyer = new Buyer
                {
                    UserId = newUser.UserId,
                    FullName = model.FullName.Trim(),
                    CompanyName = model.CompanyName.Trim(),
                    MobileNumber = model.MobileNumber.Trim(),
                    BusinessAddress = model.Address?.Trim() ?? "",
                    City = model.City?.Trim() ?? "",
                    District = model.District?.Trim() ?? "",
                    State = model.State?.Trim() ?? "",
                    PinCode = model.Pincode?.Trim() ?? ""
                };

                _context.Buyers.Add(newBuyer);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Registration completed successfully.";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                ViewData["Title"] = "Buyer Registration";
                var deepest = ex;
                while (deepest.InnerException != null)
                {
                    deepest = deepest.InnerException;
                }
                ViewData["ErrorMessage"] = "An error occurred during registration. Details: " + deepest.Message;
                return View(model);
            }
        }
    }
}
