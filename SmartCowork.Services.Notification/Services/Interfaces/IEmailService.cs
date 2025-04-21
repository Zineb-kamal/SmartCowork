


namespace SmartCowork.Services.Notification.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendAsync(string to, string subject, string body);
    }
}

