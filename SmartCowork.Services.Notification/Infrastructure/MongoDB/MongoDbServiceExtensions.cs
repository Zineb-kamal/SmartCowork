using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SmartCowork.Services.Notification.Models;

namespace SmartCowork.Services.Notification.Infrastructure.MongoDB
{
    // Infrastructure/MongoDB/MongoDbServiceExtensions.cs
    public static class MongoDbServiceExtensions
    {
        public static IServiceCollection AddMongoDb(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment env)
        {
            var mongoSettings = configuration
                .GetSection("MongoDB")
                .Get<MongoDbSettings>();

            // Validation de la configuration
            if (string.IsNullOrEmpty(mongoSettings?.ConnectionString))
                throw new InvalidOperationException("MongoDB connection string is not configured");

            if (string.IsNullOrEmpty(mongoSettings?.DatabaseName))
                throw new InvalidOperationException("MongoDB database name is not configured");

            // Configuration spécifique à l'environnement
            if (env.IsDevelopment())
            {
                services.AddSingleton<IMongoClient>(sp =>
                    new MongoClient(mongoSettings.ConnectionString));
            }
            else
            {
                services.AddSingleton<IMongoClient>(sp =>
                    new MongoClient(mongoSettings.ToMongoClientSettings()));
            }

            services.Configure<MongoDbSettings>(
                configuration.GetSection("MongoDB"));

            services.AddSingleton<MongoDbContext>();

            return services;
        }
    }
}
