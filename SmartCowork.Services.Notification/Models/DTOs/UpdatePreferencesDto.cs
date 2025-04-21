

namespace SmartCowork.Services.Notification.Models.DTOs
{
    public class UpdatePreferencesDto
    {
        public bool EmailEnabled { get; set; }
        public bool SMSEnabled { get; set; }
        public bool PushEnabled { get; set; }
        public bool InAppEnabled { get; set; }
        public string PreferredChannel { get; set; }
    }

}