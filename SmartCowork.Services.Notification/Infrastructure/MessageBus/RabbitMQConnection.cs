//using RabbitMQ.Client;
//using Microsoft.Extensions.Logging;
//using System;
//using Microsoft.EntityFrameworkCore.Metadata;
//using RabbitMQ.Client.Events;

//namespace SmartCowork.Services.Notification.Infrastructure.MessageBus
//{
//    // Interface pour la connexion RabbitMQ
//    public interface IRabbitMQConnection : IDisposable
//    {
//        IModel CreateChannel();
//        bool IsConnected { get; }
//        void TryConnect();
//    }

//    // Implémentation de la connexion RabbitMQ
//    public class RabbitMQConnection : IRabbitMQConnection
//    {
//        private readonly IConnectionFactory _connectionFactory;
//        private IConnection _connection;
//        private readonly ILogger<RabbitMQConnection> _logger;
//        private bool _disposed;

//        public RabbitMQConnection(IConnectionFactory connectionFactory, ILogger<RabbitMQConnection> logger)
//        {
//            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
//            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        }

//        public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

//        public IModel CreateChannel()
//        {
//            if (!IsConnected)
//            {
//                TryConnect();
//            }

//            if (!IsConnected)
//            {
//                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
//            }

//            try
//            {
//                return _connection.CreateModel();
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error creating RabbitMQ channel");
//                throw;
//            }
//        }

//        public void TryConnect()
//        {
//            try
//            {
//                if (IsConnected) return;

//                _connection = _connectionFactory.CreateConnection();

//                if (!IsConnected)
//                {
//                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
//                    throw new InvalidOperationException("FATAL ERROR: RabbitMQ connections could not be created and opened");
//                }

//                _connection.ConnectionShutdown += OnConnectionShutdown;
//                _connection.CallbackException += OnCallbackException;
//                _connection.ConnectionBlocked += OnConnectionBlocked;

//                _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}'",
//                    _connectionFactory.HostName);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogCritical(ex, "FATAL ERROR: RabbitMQ connections could not be created and opened");
//                throw;
//            }
//        }

//        private void OnConnectionBlocked(object sender, global::RabbitMQ.Client.Events.ConnectionBlockedEventArgs e)
//        {
//            if (_disposed) return;

//            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
//            TryConnect();
//        }

//        private void OnCallbackException(object sender, global::RabbitMQ.Client.Events.CallbackExceptionEventArgs e)
//        {
//            if (_disposed) return;

//            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
//            TryConnect();
//        }

//        private void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
//        {
//            if (_disposed) return;

//            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
//            TryConnect();
//        }

//        public void Dispose()
//        {
//            if (_disposed) return;

//            try
//            {
//                _connection?.Dispose();
//            }
//            catch (IOException ex)
//            {
//                _logger.LogCritical(ex.ToString());
//            }

//            _disposed = true;
//        }
//    }
//}