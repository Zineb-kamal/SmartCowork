using SmartCowork.Services.Notification.Models;


namespace SmartCowork.Services.Notification.Repository
{
    public interface INotificationRepository
    {
        Task<Models.Notification> GetByIdAsync(Guid id);
        Task<IEnumerable<Models.Notification>> GetByUserIdAsync(Guid userId);
        Task<Models.Notification> CreateAsync(Models.Notification notification);
        Task<Models.Notification> UpdateAsync(Models.Notification notification);
        Task<bool> DeleteAsync(Guid id);
        Task<NotificationPreference> GetUserPreferencesAsync(Guid userId);
        Task UpdatePreferencesAsync(NotificationPreference preferences);
    }
}

