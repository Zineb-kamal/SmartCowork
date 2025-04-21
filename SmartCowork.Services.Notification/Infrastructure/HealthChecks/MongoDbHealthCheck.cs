

using SmartCowork.Services.Notification.Infrastructure.MongoDB;

namespace SmartCowork.Services.Notification.Infrastructure.HealthChecks
{
    public static class MongoDbHealthCheck
    {
        public static IHealthChecksBuilder AddMongoDbHealthCheck(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            var mongoSettings = configuration
                .GetSection("MongoDB")
                .Get<MongoDbSettings>();

            return builder.AddMongoDb(
                mongoSettings.ConnectionString,
                mongoSettings.DatabaseName,
                name: "mongodb",
                timeout: TimeSpan.FromSeconds(3));
        }
    }
}