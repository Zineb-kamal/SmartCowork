// Services/BookingRabbitMQConsumer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Booking.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCowork.Services.Booking.Services
{
    public class BookingRabbitMQConsumer : RabbitMQConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingRabbitMQConsumer(
            ILogger<BookingRabbitMQConsumer> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
            : base(logger, configuration)
        {
            _serviceProvider = serviceProvider;
        }

        protected override void InitializeRabbitMQ()
        {
            base.InitializeRabbitMQ();

            // Déclarer les échanges
            DeclareExchange("booking_events");
            DeclareExchange("billing_events");
            DeclareExchange("space_events");

            // Déclarer les files d'attente pour les messages entrants
            DeclareQueue("booking_payment_processed");
            DeclareQueue("booking_space_status_changed");

            // Lier les files d'attente aux échanges
            BindQueue("booking_payment_processed", "billing_events", "payment.processed");
            BindQueue("booking_space_status_changed", "space_events", "space.status_changed");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Enabled) return Task.CompletedTask;

            // Configurer les consommateurs
            StartConsumer<PaymentProcessedMessage>("booking_payment_processed", ProcessPaymentMessage);
            StartConsumer<SpaceStatusChangedMessage>("booking_space_status_changed", ProcessSpaceStatusChange);

            return Task.CompletedTask;
        }

        private async Task ProcessPaymentMessage(PaymentProcessedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.ProcessPaymentMessageAsync(message);
        }

        private async Task ProcessSpaceStatusChange(SpaceStatusChangedMessage message)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.ProcessSpaceStatusChangeAsync(message);
        }
    }
}