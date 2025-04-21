using MongoDB.Driver;
using SmartCowork.Services.Notification.Infrastructure.MongoDB;
using SmartCowork.Services.Notification.Models;

namespace SmartCowork.Services.Notification.Repository
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Models.Notification> _notifications;
        private readonly IMongoCollection<NotificationPreference> _preferences;
        private readonly MongoDbContext _context;
        private readonly ILogger<NotificationRepository> _logger;

        public NotificationRepository(
            IMongoDatabase database,
            MongoDbContext context,
            ILogger<NotificationRepository> logger)
        {
            _notifications = database.GetCollection<Models.Notification>("notifications");
            _preferences = database.GetCollection<NotificationPreference>("notification_preferences");
            _context = context;
            _logger = logger;

            CreateIndexes();
        }

        private void CreateIndexes()
        {
            // Index pour les notifications
            var notificationIndexBuilder = Builders<Models.Notification>.IndexKeys;
            var notificationIndexes = new[]
            {
                new CreateIndexModel<Models.Notification>(
                    notificationIndexBuilder.Ascending(x => x.UserId)),
                new CreateIndexModel<Models.Notification>(
                    notificationIndexBuilder.Descending(x => x.CreatedAt))
            };
            _notifications.Indexes.CreateMany(notificationIndexes);

            // Index pour les préférences
            var preferenceIndexBuilder = Builders<NotificationPreference>.IndexKeys;
            var preferenceIndex = new CreateIndexModel<NotificationPreference>(
                preferenceIndexBuilder.Ascending(x => x.UserId),
                new CreateIndexOptions { Unique = true });
            _preferences.Indexes.CreateOne(preferenceIndex);
        }

        public async Task<Models.Notification> GetByIdAsync(Guid id)
        {
            try
            {
                return await _notifications
                    .Find(n => n.Id == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notification {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Models.Notification>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                return await _notifications
                    .Find(n => n.UserId == userId)
                    .SortByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving notifications for user {userId}");
                throw;
            }
        }

        public async Task<Models.Notification> CreateAsync(Models.Notification notification)
        {
            try
            {
                notification.CreatedAt = DateTime.UtcNow;
                await _notifications.InsertOneAsync(notification);
                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating notification");
                throw;
            }
        }

        public async Task<Models.Notification> UpdateAsync(Models.Notification notification)
        {
            try
            {
                var filter = Builders<Models.Notification>.Filter.Eq(n => n.Id, notification.Id);
                var result = await _notifications.ReplaceOneAsync(filter, notification);

                if (result.ModifiedCount == 0)
                    throw new Exception($"Notification {notification.Id} not found");

                return notification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating notification {notification.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            try
            {
                var result = await _notifications.DeleteOneAsync(n => n.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting notification {id}");
                throw;
            }
        }

        public async Task<NotificationPreference> GetUserPreferencesAsync(Guid userId)
        {
            try
            {
                return await _preferences
                    .Find(p => p.UserId == userId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving preferences for user {userId}");
                throw;
            }
        }

        public async Task UpdatePreferencesAsync(NotificationPreference preferences)
        {
            try
            {
                var filter = Builders<NotificationPreference>.Filter
                    .Eq(p => p.UserId, preferences.UserId);
                var options = new ReplaceOptions { IsUpsert = true };

                await _preferences.ReplaceOneAsync(filter, preferences, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating preferences for user {preferences.UserId}");
                throw;
            }
        }
    }
}