namespace SmartCowork.Services.Notification.Exceptions
{
    public class TemplateNotFoundException : Exception
    {
        public TemplateNotFoundException(string code)
            : base($"Template with code {code} not found")
        {
        }
    }
}
