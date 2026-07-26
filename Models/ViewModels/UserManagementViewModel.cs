using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models.ViewModels
{
    public class UserManagementViewModel
    {
        // Statistics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingApproval { get; set; }
        public int BlockedUsers { get; set; }

        // User List
        public List<UserDto> Users { get; set; } = new List<UserDto>();

        // Search and Filters
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string ProfileInitials { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string RoleBadgeClass { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusBadgeClass { get; set; } = string.Empty;
        public DateTime JoinDate { get; set; }
        public string JoinDateFormatted { get; set; } = string.Empty;
        public DateTime? LastLogin { get; set; }
        public string LastLoginFormatted { get; set; } = string.Empty;
        public string AssignedFieldOfficer { get; set; } = "N/A";
        public string AssignedAgronomist { get; set; } = "N/A";
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public int RoleId { get; set; }
    }

    public class EditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}
