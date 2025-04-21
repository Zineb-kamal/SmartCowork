
using SmartCowork.Services.Notification.Exceptions;
using SmartCowork.Services.Notification.Models;
using SmartCowork.Services.Notification.Models.DTOs;
using SmartCowork.Services.Notification.Models.Enums;
using SmartCowork.Services.Notification.Repository;
using SmartCowork.Services.Notification.Services.Interfaces;

namespace SmartCowork.Services.Notification.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ITemplateService _templateService;
        private readonly IEmailService _emailService;
        private readonly ISMSService _smsService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            ITemplateService templateService,
            IEmailService emailService,
            ISMSService smsService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _templateService = templateService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<Models.Notification> CreateNotificationAsync(CreateNotificationDto dto)
        {
            var template = await _templateService.GetTemplateAsync(dto.TemplateCode);
            if (template == null)
                throw new TemplateNotFoundException(dto.TemplateCode);

            var notification = new Models.Notification
            {
                Id = Guid.NewGuid(),
                UserId = dto.UserId,
                Type = dto.Type,
                Priority = dto.Priority,
                Status = NotificationStatus.Pending,
                Data = dto.Data,
                CreatedAt = DateTime.UtcNow
            };

            notification.Title = _templateService.ProcessTemplate(template.Subject, dto.Data);
            notification.Message = _templateService.ProcessTemplate(
                GetTemplateBodyByType(template, dto.Type),
                dto.Data
            );

            await _notificationRepository.CreateAsync(notification);

            try
            {
                await SendNotificationByType(notification);
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send notification {notification.Id}");
                notification.Status = NotificationStatus.Failed;
            }

            await _notificationRepository.UpdateAsync(notification);
            return notification;
        }

        public async Task<Models.Notification> GetNotificationAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                throw new NotificationNotFoundException(id);

            return notification;
        }

        public async Task<IEnumerable<Models.Notification>> GetUserNotificationsAsync(Guid userId)
        {
            try
            {
                var notifications = await _notificationRepository.GetByUserIdAsync(userId);
                return notifications.OrderByDescending(n => n.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notifications for user {userId}");
                throw new NotificationServiceException("Failed to retrieve user notifications", ex);
            }
        }

        public async Task<bool> MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                    return false;

                notification.Status = NotificationStatus.Read;
                notification.ReadAt = DateTime.UtcNow;

                await _notificationRepository.UpdateAsync(notification);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notificationId} as read");
                throw new NotificationServiceException("Failed to mark notification as read", ex);
            }
        }

        public async Task<bool> DeleteNotificationAsync(Guid notificationId)
        {
            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                    return false;

                return await _notificationRepository.DeleteAsync(notificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {notificationId}");
                throw new NotificationServiceException("Failed to delete notification", ex);
            }
        }

        public async Task UpdatePreferencesAsync(Guid userId, UpdatePreferencesDto preferences)
        {
            try
            {
                var existingPreferences = await _notificationRepository.GetUserPreferencesAsync(userId);
                if (existingPreferences == null)
                {
                    existingPreferences = new NotificationPreference
                    {
                        UserId = userId
                    };
                }

                existingPreferences.EmailEnabled = preferences.EmailEnabled;
                existingPreferences.SMSEnabled = preferences.SMSEnabled;
                existingPreferences.PushEnabled = preferences.PushEnabled;
                existingPreferences.InAppEnabled = preferences.InAppEnabled;
                existingPreferences.PreferredChannel = preferences.PreferredChannel;

                await _notificationRepository.UpdatePreferencesAsync(existingPreferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating preferences for user {userId}");
                throw new NotificationServiceException("Failed to update notification preferences", ex);
            }
        }

        private async Task SendNotificationByType(Models.Notification notification)
        {
            switch (notification.Type)
            {
                case NotificationType.Email:
                    if (!notification.Data.ContainsKey("email"))
                        throw new NotificationDataMissingException("email");

                    await _emailService.SendAsync(
                        notification.Data["email"],
                        notification.Title,
                        notification.Message
                    );
                    break;

                case NotificationType.SMS:
                    if (!notification.Data.ContainsKey("phone"))
                        throw new NotificationDataMissingException("phone");

                    await _smsService.SendAsync(
                        notification.Data["phone"],
                        notification.Message
                    );
                    break;

                case NotificationType.Push:
                    if (!notification.Data.ContainsKey("deviceToken"))
                        throw new NotificationDataMissingException("deviceToken");

                    // Implémentation du Push à ajouter
                    break;

                case NotificationType.InApp:
                    // Les notifications in-app sont déjà stockées dans la base de données
                    break;

                default:
                    throw new ArgumentException($"Unsupported notification type: {notification.Type}");
            }
        }

        private string GetTemplateBodyByType(NotificationTemplate template, NotificationType type)
        {
            return type switch
            {
                NotificationType.Email => template.EmailBody,
                NotificationType.SMS => template.SMSBody,
                NotificationType.Push => template.PushBody,
                NotificationType.InApp => template.EmailBody,
                _ => throw new ArgumentException($"Unsupported notification type: {type}")
            };
        }
    }
}

