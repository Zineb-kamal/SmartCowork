// Services/BillingRabbitMQConsumer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Billing.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCowork.Services.Billing.Services
{
    public class BillingRabbitMQConsumer : RabbitMQConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public BillingRabbitMQConsumer(
            ILogger<BillingRabbitMQConsumer> logger,
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
            DeclareExchange("billing_events");
            DeclareExchange("booking_events");

            // Déclarer les files d'attente pour les messages entrants
            DeclareQueue("billing_booking_created");
            DeclareQueue("billing_booking_updated");
            DeclareQueue("billing_booking_cancelled");
            DeclareQueue("billing_booking_completed");

            // Lier les files d'attente aux échanges avec routing keys
            BindQueue("billing_booking_created", "booking_events", "booking.created");
            BindQueue("billing_booking_updated", "booking_events", "booking.updated");
            BindQueue("billing_booking_cancelled", "booking_events", "booking.cancelled");
            BindQueue("billing_booking_completed", "booking_events", "booking.completed");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Enabled) return Task.CompletedTask;

            // Configurer les consommateurs pour chaque file d'attente
            StartConsumer<BookingCreatedMessage>("billing_booking_created", ProcessBookingCreated);
            StartConsumer<BookingUpdatedMessage>("billing_booking_updated", ProcessBookingUpdated);
            StartConsumer<BookingMessages>("billing_booking_cancelled", ProcessBookingCancelled);
            StartConsumer<BookingCompletedMessage>("billing_booking_completed", ProcessBookingCompleted);

            return Task.CompletedTask;
        }

        private async Task ProcessBookingCreated(BookingCreatedMessage message)
        {
            Logger.LogInformation($"Message reçu: réservation créée {message.BookingId} pour la facturation");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                await billingService.ProcessBookingCreatedAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement du message de réservation créée: {ex.Message}");
                throw;
            }
        }

        private async Task ProcessBookingUpdated(BookingUpdatedMessage message)
        {
            Logger.LogInformation($"Message reçu: réservation mise à jour {message.BookingId} pour la facturation");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                await billingService.ProcessBookingUpdatedAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement du message de réservation mise à jour: {ex.Message}");
                throw;
            }
        }

        private async Task ProcessBookingCancelled(BookingMessages message)
        {
            Logger.LogInformation($"Message reçu: réservation annulée {message.BookingId} pour la facturation");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                await billingService.ProcessBookingCancelledAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement du message de réservation annulée: {ex.Message}");
                throw;
            }
        }

        private async Task ProcessBookingCompleted(BookingCompletedMessage message)
        {
            Logger.LogInformation($"Message reçu: réservation terminée {message.BookingId} pour la facturation");

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var billingService = scope.ServiceProvider.GetRequiredService<IBillingService>();
                await billingService.ProcessBookingCompletedAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement du message de réservation terminée: {ex.Message}");
                throw;
            }
        }
    }
}