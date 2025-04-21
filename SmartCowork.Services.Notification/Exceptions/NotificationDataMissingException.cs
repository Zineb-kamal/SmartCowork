namespace SmartCowork.Services.Notification.Exceptions
{
    public class NotificationDataMissingException : Exception
    {
        public NotificationDataMissingException(string fieldName)
            : base($"Required notification data field '{fieldName}' is missing")
        {
        }
    }
}
