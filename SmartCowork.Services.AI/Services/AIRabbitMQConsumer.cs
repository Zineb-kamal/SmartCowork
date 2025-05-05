// Services/AIRabbitMQConsumer.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmartCowork.Common.Messaging.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCowork.Services.AI.Services
{
    public class AIRabbitMQConsumer : RabbitMQConsumerBase
    {
        private readonly IServiceProvider _serviceProvider;

        public AIRabbitMQConsumer(
            ILogger<AIRabbitMQConsumer> logger,
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
            DeclareExchange("space_events");
            DeclareExchange("user_events");

            // Déclarer les files d'attente pour les messages entrants
            DeclareQueue("ai_booking_created");
            DeclareQueue("ai_booking_completed");
            DeclareQueue("ai_space_created");
            DeclareQueue("ai_user_preference_updated");

            // Lier les files d'attente aux échanges
            BindQueue("ai_booking_created", "booking_events", "booking.created");
            BindQueue("ai_booking_completed", "booking_events", "booking.completed");
            BindQueue("ai_space_created", "space_events", "space.created");
            BindQueue("ai_user_preference_updated", "user_events", "user.preference_updated");
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!Enabled) return Task.CompletedTask;

            // Configurer les consommateurs
            StartConsumer<BookingCreatedMessage>("ai_booking_created", ProcessBookingCreated);
            StartConsumer<BookingCompletedMessage>("ai_booking_completed", ProcessBookingCompleted);
            StartConsumer<SpaceCreatedMessage>("ai_space_created", ProcessSpaceCreated);
            StartConsumer<UserPreferenceUpdatedMessage>("ai_user_preference_updated", ProcessUserPreferenceUpdated);

            return Task.CompletedTask;
        }

        private async Task ProcessBookingCreated(BookingCreatedMessage message)
        {
            Logger.LogInformation($"Traitement du message de réservation créée {message.BookingId} pour l'IA");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var recommendationService = scope.ServiceProvider.GetRequiredService<IRecommendationService>();

                // Enregistrer l'activité utilisateur
                await recommendationService.TrackUserActivityAsync(new Models.UserActivity
                {
                    Id = Guid.NewGuid(),
                    UserId = message.UserId,
                    BookingId = message.BookingId,
                    SpaceId = message.SpaceId,
                    StartTime = message.StartDateTime,
                    EndTime = message.EndDateTime,
                    WasCancelled = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Erreur lors du traitement de la réservation créée: {ex.Message}");
            }
        }

        private async Task ProcessBookingCompleted(BookingCompletedMessage message)
        {
            Logger.LogInformation($"Traitement du message de réservation terminée {message.BookingId} pour l'IA");
            // Logique similaire...
        }

        private async Task ProcessSpaceCreated(SpaceCreatedMessage message)
        {
            Logger.LogInformation($"Traitement du message d'espace créé {message.SpaceId} pour l'IA");
            // Logique de traitement...
        }

        private async Task ProcessUserPreferenceUpdated(UserPreferenceUpdatedMessage message)
        {
            Logger.LogInformation($"Traitement du message de préférence utilisateur mise à jour pour l'utilisateur {message.UserId}");
            // Logique de traitement...
        }
    }

    // Classes pour les messages
    public class BookingCreatedMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string SpaceName { get; set; }
        public string Purpose { get; set; }
    }

    public class BookingCompletedMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
    }

    public class SpaceCreatedMessage
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
    }

    public class UserPreferenceUpdatedMessage
    {
        public Guid UserId { get; set; }
        public Dictionary<string, object> Preferences { get; set; }
    }
}