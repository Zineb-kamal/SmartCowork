// RabbitMQ/RabbitMQConsumerBase.cs
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SmartCowork.Common.Messaging.RabbitMQ
{
    public abstract class RabbitMQConsumerBase : BackgroundService
    {
        protected readonly ILogger<RabbitMQConsumerBase> Logger;
        protected IConnection Connection;
        protected IModel Channel;
        protected readonly string Hostname;
        protected readonly string Username;
        protected readonly string Password;
        protected readonly int Port;
        protected readonly bool Enabled;

        protected RabbitMQConsumerBase(ILogger<RabbitMQConsumerBase> logger, IConfiguration configuration)
        {
            Logger = logger;

            // Lire la configuration
            var rabbitMQConfig = configuration.GetSection("RabbitMQ");
            Enabled = rabbitMQConfig.GetValue<bool>("Enabled", false);
            Hostname = rabbitMQConfig["HostName"] ?? "localhost";
            Username = rabbitMQConfig["UserName"] ?? "guest";
            Password = rabbitMQConfig["Password"] ?? "guest";
            Port = rabbitMQConfig.GetValue<int>("Port", 5672);

            if (Enabled)
            {
                try
                {
                    InitializeRabbitMQ();
                    Logger.LogInformation("RabbitMQ consumer base initialized successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to initialize RabbitMQ consumer base");
                }
            }
            else
            {
                Logger.LogWarning("RabbitMQ is disabled - consumer will not receive messages");
            }
        }

        protected virtual void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = Hostname,
                    UserName = Username,
                    Password = Password,
                    Port = Port,
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    AutomaticRecoveryEnabled = true
                };

                Connection = factory.CreateConnection();
                Channel = Connection.CreateModel();

                // Les déclarations spécifiques d'échanges et de files d'attente seront dans les classes dérivées
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error initializing RabbitMQ connection to {Hostname}:{Port}");
                throw;
            }
        }

        protected void DeclareExchange(string exchangeName, string exchangeType = ExchangeType.Topic)
        {
            Channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType, durable: true);
            Logger.LogInformation($"Declared exchange '{exchangeName}' of type '{exchangeType}'");
        }

        protected void DeclareQueue(string queueName)
        {
            Channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            Logger.LogInformation($"Declared queue '{queueName}'");
        }

        protected void BindQueue(string queueName, string exchangeName, string routingKey)
        {
            Channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
            Logger.LogInformation($"Bound queue '{queueName}' to exchange '{exchangeName}' with routing key '{routingKey}'");
        }

        protected void StartConsumer<T>(string queueName, Func<T, Task> messageHandler)
        {
            var consumer = new EventingBasicConsumer(Channel);

            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    Logger.LogInformation($"Received message from queue '{queueName}': {message}");
                    var typedMessage = JsonConvert.DeserializeObject<T>(message);

                    await messageHandler(typedMessage);

                    // Message traité avec succès
                    Channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error processing message from queue '{queueName}': {message}");
                    // Rejeter le message et le remettre dans la file d'attente
                    Channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
            };

            Channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
            Logger.LogInformation($"Started consuming from queue '{queueName}'");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Enabled)
            {
                Logger.LogWarning("RabbitMQ consumer disabled - not starting message consumption");
                return Task.CompletedTask;
            }

            // La configuration des consommateurs sera dans les classes dérivées
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            Channel?.Close();
            Connection?.Close();
            base.Dispose();
        }
    }
}