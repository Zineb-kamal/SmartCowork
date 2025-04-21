// Services/SpaceRabbitMQConsumer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Space.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCowork.Services.Space.Services
{
    public class SpaceRabbitMQConsumer : RabbitMQConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public SpaceRabbitMQConsumer(
            ILogger<SpaceRabbitMQConsumer> logger,
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
            DeclareExchange("space_events");
            DeclareExchange("booking_events");

            // Déclarer les files d'attente pour les messages entrants
            DeclareQueue("space_booking_created");
            DeclareQueue("space_booking_cancelled");

            // Lier les files d'attente aux échanges
            BindQueue("space_booking_created", "booking_events", "booking.created");
            BindQueue("space_booking_cancelled", "booking_events", "booking.cancelled");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Enabled) return Task.CompletedTask;

            // Configurer les consommateurs
            StartConsumer<BookingCreatedMessage>("space_booking_created", ProcessBookingCreated);
            StartConsumer<BookingCancelledMessage>("space_booking_cancelled", ProcessBookingCancelled);

            return Task.CompletedTask;
        }

        private async Task ProcessBookingCreated(BookingCreatedMessage message)
        {
            Logger.LogInformation($"Traitement du message de réservation créée pour l'espace {message.SpaceId}");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var spaceService = scope.ServiceProvider.GetRequiredService<ISpaceService>();
                await spaceService.ProcessBookingCreatedAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement de la réservation créée: {ex.Message}");
                throw;
            }
        }

        private async Task ProcessBookingCancelled(BookingCancelledMessage message)
        {
            Logger.LogInformation($"Traitement du message d'annulation de réservation pour l'espace {message.SpaceId}");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var spaceService = scope.ServiceProvider.GetRequiredService<ISpaceService>();
                await spaceService.ProcessBookingCancelledAsync(message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement de l'annulation de réservation: {ex.Message}");
                throw;
            }
        }
    }
}