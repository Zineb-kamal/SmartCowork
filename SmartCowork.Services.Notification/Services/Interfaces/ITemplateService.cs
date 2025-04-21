


using SmartCowork.Services.Notification.Models;

namespace SmartCowork.Services.Notification.Services.Interfaces
{
    public interface ITemplateService
    {
        Task<NotificationTemplate> GetTemplateAsync(string code);
        Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template);
        Task<NotificationTemplate> UpdateTemplateAsync(string code, NotificationTemplate template);
        Task<bool> DeleteTemplateAsync(string code);
        string ProcessTemplate(string template, Dictionary<string, string> data);
    }
}

