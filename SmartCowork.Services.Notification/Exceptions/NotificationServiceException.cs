namespace SmartCowork.Services.Notification.Exceptions
{
    public class NotificationServiceException : Exception
    {
        public NotificationServiceException(string message) : base(message)
        {
        }

        public NotificationServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
