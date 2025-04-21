using MongoDB.Driver;

namespace SmartCowork.Services.Notification.Infrastructure.MongoDB
{

    // Infrastructure/MongoDB/MongoDbSettings.cs
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string AuthSource { get; set; }
        public string ReplicaSet { get; set; }
        public bool RetryWrites { get; set; }
        public int MaxConnectionPoolSize { get; set; }

        public MongoClientSettings ToMongoClientSettings()
        {
            var mongoUrlBuilder = new MongoUrlBuilder(ConnectionString)
            {
                // Configurer les paramètres d'authentification si nécessaire
                AuthenticationSource = AuthSource,
                MaxConnectionPoolSize = MaxConnectionPoolSize,
                RetryWrites = RetryWrites,
                ReplicaSetName = ReplicaSet
            };

            var settings = MongoClientSettings.FromUrl(mongoUrlBuilder.ToMongoUrl());

            // Configuration additionnelle si nécessaire
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            return settings;
        }
    }
}
