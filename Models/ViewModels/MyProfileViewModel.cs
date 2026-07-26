using System.ComponentModel.DataAnnotations;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class MyProfileViewModel
    {
        // User Information
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        // Profile Information
        [Required(ErrorMessage = "First Name is required")]
        [StringLength(50, ErrorMessage = "First Name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last Name is required")]
        [StringLength(50, ErrorMessage = "Last Name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone Number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone Number must be 10 digits")]
        [StringLength(20, ErrorMessage = "Phone Number cannot exceed 20 characters")]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Employee ID cannot exceed 20 characters")]
        public string? EmployeeId { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        public string? City { get; set; }

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        public string? State { get; set; }

        [RegularExpression(@"^\d{6}$", ErrorMessage = "PIN Code must be 6 digits")]
        [StringLength(10, ErrorMessage = "PIN Code cannot exceed 10 characters")]
        public string? PinCode { get; set; }

        // Computed Properties
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string ProfileInitials
        {
            get
            {
                var initials = string.Empty;
                if (!string.IsNullOrWhiteSpace(FirstName))
                    initials += FirstName[0];
                if (!string.IsNullOrWhiteSpace(LastName))
                    initials += LastName[0];
                return initials.ToUpper();
            }
        }

        public string StatusText => IsActive ? "Active" : "Inactive";
    }
}
