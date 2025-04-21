


namespace SmartCowork.Services.Notification.Services.Interfaces
{
    public interface ISMSService
    {
        Task<bool> SendAsync(string phoneNumber, string message);
    }
}

