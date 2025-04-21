

using SmartCowork.Services.Notification.Models.Enums;

namespace SmartCowork.Services.Notification.Models.DTOs
{
    public class CreateNotificationDto
    {
        public Guid UserId { get; set; }
        public string TemplateCode { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }

}