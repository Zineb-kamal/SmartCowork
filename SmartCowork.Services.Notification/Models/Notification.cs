

using SmartCowork.Services.Notification.Models.Enums;

namespace SmartCowork.Services.Notification.Models
{
    public class Notification
    {
     
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public NotificationStatus Status { get; set; }
        public NotificationPriority Priority { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime? SentAt { get; set; }
    }



}