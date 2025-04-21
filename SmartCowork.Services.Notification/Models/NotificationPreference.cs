
namespace SmartCowork.Services.Notification.Models
{
    public class NotificationPreference
    {
       
        public Guid UserId { get; set; }
        public bool EmailEnabled { get; set; }
        public bool SMSEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public string PreferredChannel { get; set; }
        public Dictionary<string, bool> TypePreferences { get; set; }
    }
}




