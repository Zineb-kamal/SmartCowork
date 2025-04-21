using MongoDB.Driver;
using SmartCowork.Services.Notification.Models;
using SmartCowork.Services.Notification.Services.Interfaces;

namespace SmartCowork.Services.Notification.Services.Implementation
{
    public class TemplateService : ITemplateService
    {
        private readonly IMongoCollection<NotificationTemplate> _templates;
        private readonly ILogger<TemplateService> _logger;

        public TemplateService(
            IMongoDatabase database,
            ILogger<TemplateService> logger)
        {
            _templates = database.GetCollection<NotificationTemplate>("templates");
            _logger = logger;
        }

        public async Task<NotificationTemplate> GetTemplateAsync(string code)
        {
            return await _templates.Find(t => t.Code == code && t.IsActive)
                .FirstOrDefaultAsync();
        }

        public async Task<NotificationTemplate> CreateTemplateAsync(NotificationTemplate template)
        {
            await _templates.InsertOneAsync(template);
            return template;
        }

        public async Task<NotificationTemplate> UpdateTemplateAsync(string code, NotificationTemplate template)
        {
            await _templates.ReplaceOneAsync(t => t.Code == code, template);
            return template;
        }

        public async Task<bool> DeleteTemplateAsync(string code)
        {
            var result = await _templates.DeleteOneAsync(t => t.Code == code);
            return result.DeletedCount > 0;
        }

        public string ProcessTemplate(string template, Dictionary<string, string> data)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            string processed = template;
            foreach (var kvp in data)
            {
                processed = processed.Replace($"{{{kvp.Key}}}", kvp.Value);
            }

            return processed;
        }
    }
}

