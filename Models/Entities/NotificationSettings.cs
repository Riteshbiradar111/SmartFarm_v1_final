using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    [Table("NotificationSettings")]
    public class NotificationSettings
    {
        [Key]
        public int NotificationSettingId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public bool EmailNotifications { get; set; } = true;

        [Required]
        public bool SmsNotifications { get; set; } = false;

        [Required]
        public bool PushNotifications { get; set; } = true;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
