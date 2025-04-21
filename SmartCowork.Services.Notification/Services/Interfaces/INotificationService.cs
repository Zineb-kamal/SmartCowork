using SmartCowork.Services.Notification.Models.DTOs;


namespace SmartCowork.Services.Notification.Services.Interfaces
{
    public interface INotificationService
    {
        Task<Models.Notification> CreateNotificationAsync(CreateNotificationDto dto);
        Task<Models.Notification> GetNotificationAsync(Guid id);
        Task<IEnumerable<Models.Notification>> GetUserNotificationsAsync(Guid userId);
        Task<bool> MarkAsReadAsync(Guid notificationId);
        Task<bool> DeleteNotificationAsync(Guid notificationId);
        Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesDto preferences);
    }
}

