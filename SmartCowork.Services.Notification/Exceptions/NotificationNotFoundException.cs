namespace SmartCowork.Services.Notification.Exceptions
{
    public class NotificationNotFoundException : Exception
    {
        public NotificationNotFoundException(Guid id)
         : base($"Notification with ID {id} not found")
        {
        }
    }
}
