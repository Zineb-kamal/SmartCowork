
namespace SmartCowork.Services.Notification.Models
{
    public class NotificationTemplate
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string EmailBody { get; set; }
        public string SMSBody { get; set; }
        public string PushBody { get; set; }
        public bool IsActive { get; set; }
        public List<string> RequiredParameters { get; set; }
    }
}




