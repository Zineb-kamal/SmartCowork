using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;


namespace SmartCowork.Common.Messaging.RabbitMQ
{
    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly ILogger<RabbitMQProducer> _logger;
        private IConnection _connection;
        private IModel _channel;
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;
        private readonly int _port;
        private readonly bool _enabled;

        public RabbitMQProducer(ILogger<RabbitMQProducer> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Lire la configuration
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");
            _enabled = rabbitMQConfig.GetValue<bool>("Enabled", false);
            _hostname = rabbitMQConfig["HostName"] ?? "localhost";
            _username = rabbitMQConfig["UserName"] ?? "guest";
            _password = rabbitMQConfig["Password"] ?? "guest";
            _port = rabbitMQConfig.GetValue<int>("Port", 5672);

            if (_enabled)
            {
                try
                {
                    CreateConnection();
                    _logger.LogInformation("RabbitMQ producer initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize RabbitMQ producer");
                }
            }
            else
            {
                _logger.LogWarning("RabbitMQ is disabled - producer will not send messages");
            }
        }

        private void CreateConnection()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password,
                    Port = _port,
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    AutomaticRecoveryEnabled = true
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating RabbitMQ connection to {_hostname}:{_port}");
                throw;
            }
        }

        public void PublishMessage<T>(string exchangeName, string routingKey, T message)
        {
            if (!_enabled)
            {
                _logger.LogInformation($"RabbitMQ disabled - message to {exchangeName}/{routingKey} not sent");
                return;
            }

            if (_channel == null)
            {
                _logger.LogWarning("Cannot publish message - RabbitMQ channel is not initialized");
                return;
            }

            try
            {
                // Déclarer l'échange s'il n'existe pas
                _channel.ExchangeDeclare(exchangeName, ExchangeType.Topic, durable: true);

                // Sérialiser le message
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                // Préparer les propriétés du message
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true; // Message persistant même si RabbitMQ redémarre
                properties.ContentType = "application/json";
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                // Publier le message
                _channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);

                _logger.LogInformation($"Published message to {exchangeName}/{routingKey}: {json}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing message to {exchangeName}/{routingKey}");
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing RabbitMQ connections");
            }
        }
    }
}