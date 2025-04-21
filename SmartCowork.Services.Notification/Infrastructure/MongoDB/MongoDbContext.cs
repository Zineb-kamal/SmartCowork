using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SmartCowork.Services.Notification.Models;

namespace SmartCowork.Services.Notification.Infrastructure.MongoDB
{
    // Infrastructure/MongoDB/MongoDbContext.cs
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly ILogger<MongoDbContext> _logger;

        public MongoDbContext(
            IOptions<MongoDbSettings> settings,
            ILogger<MongoDbContext> logger)
        {
            _logger = logger;

            try
            {
                var clientSettings = settings.Value.ToMongoClientSettings();
                var client = new MongoClient(clientSettings);
                _database = client.GetDatabase(settings.Value.DatabaseName);

                _logger.LogInformation(
                    "MongoDB connected to {Database} in {Environment}",
                    settings.Value.DatabaseName,
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                );
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to connect to MongoDB");
                throw;
            }
        }

        // Collections...
    }
}
