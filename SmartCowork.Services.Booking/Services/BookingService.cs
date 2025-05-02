using AutoMapper;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Booking.DTOs;
using SmartCowork.Services.Booking.Messages;
using SmartCowork.Services.Booking.Models;
using SmartCowork.Services.Booking.Repository;

namespace SmartCowork.Services.Booking.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly ILogger<BookingService> _logger;
        private readonly IMapper _mapper;
        private readonly IRabbitMQProducer _rabbitMQProducer;
        private readonly ISpaceService _spaceService;

        public BookingService(IBookingRepository bookingRepository, IMapper mapper, ILogger<BookingService> logger,IRabbitMQProducer rabbitMQProducer, ISpaceService spaceService)
        {
            _bookingRepository = bookingRepository;
            _mapper = mapper;
            _logger = logger;
            _rabbitMQProducer= rabbitMQProducer;
            _spaceService = spaceService;
        }

        public async Task<IEnumerable<Models.Booking>> GetAllBookingsAsync()
        {
            return await _bookingRepository.GetAllAsync();
        }

        public async Task<Models.Booking> GetBookingByIdAsync(Guid id)
        {
            return await _bookingRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Models.Booking>> GetBookingsByUserAsync(Guid userId)
        {
            return await _bookingRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Models.Booking>> GetBookingsBySpaceAsync(Guid spaceId)
        {
            return await _bookingRepository.GetBySpaceIdAsync(spaceId);
        }

        public async Task<Models.Booking> CreateBookingAsync(Models.Booking booking)
        {
            if (!await CheckSpaceAvailabilityAsync(booking.SpaceId, booking.StartTime, booking.EndTime))
            {
                throw new InvalidOperationException("Space is not available for the selected time period");
            }

            // D'abord, créer la réservation en BDD pour obtenir l'ID
            var createdBooking = await _bookingRepository.CreateAsync(booking);

            // Récupérer les détails de l'espace
            var spaceDetails = await _spaceService.GetSpaceDetailsAsync(createdBooking.SpaceId);

            // Ensuite, publier le message avec l'ID généré
            try
            {
                _rabbitMQProducer.PublishMessage(
                    "booking_events",
                    "booking.created",
                    new BookingCreatedMessage
                    {
                        BookingId = createdBooking.Id,
                        UserId = createdBooking.UserId,
                        SpaceId = createdBooking.SpaceId,
                        StartDateTime = createdBooking.StartTime,
                        EndDateTime = createdBooking.EndTime,
                        SpaceName = spaceDetails?.Name ?? "Espace non spécifié",
                        PricePerHour= spaceDetails?.PricePerHour?? 0M,
                        PricePerDay = spaceDetails?.PricePerDay ?? 0M,
                        PricePerWeek = spaceDetails?.PricePerWeek ?? 0M,
                        PricePerMonth= spaceDetails?.PricePerMonth ?? 0M,
                        PricePerYear = spaceDetails?.PricePerYear ?? 0M,
                        Purpose = createdBooking.Purpose ?? "Non spécifié",
                        AttendeesCount = createdBooking.AttendeesCount ?? 0,
                        Status = createdBooking.Status.ToString(),
                        CreatedAt = createdBooking.CreatedAt
                    }) ;
                _logger.LogInformation($"Message de création de réservation publié pour la réservation {createdBooking.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la publication: {ex.Message}");
            }
            return createdBooking;
        }

        public async Task UpdateBookingAsync(Models.Booking booking)
        {
            await _bookingRepository.UpdateAsync(booking);
        }
        public async Task DeleteBookingAsync(Guid id)
        {
            // Récupérer d'abord la réservation pour avoir toutes les informations nécessaires
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                _logger.LogWarning($"Tentative de suppression d'une réservation inexistante: {id}");
                return;
            }

            // Changer le statut de la réservation à "Cancelled" au lieu de la supprimer physiquement
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Annulé par l'utilisateur";
            await _bookingRepository.UpdateAsync(booking);

            // Publier l'événement d'annulation
            try
            {
                _rabbitMQProducer.PublishMessage(
                    "booking_events",
                    "booking.cancelled",
                    new BookingCancelledMessage
                    {
                        BookingId = booking.Id,
                        UserId = booking.UserId,
                        SpaceId = booking.SpaceId,
                        StartDateTime = booking.StartTime,
                        EndDateTime = booking.EndTime,
                        CancellationTime = DateTime.UtcNow,
                        CancellationReason = "Annulé par l'utilisateur"
                    });

                _logger.LogInformation($"Message d'annulation de réservation publié pour la réservation {booking.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la publication du message d'annulation: {ex.Message}");
            }
        }

        public async Task<bool> CheckSpaceAvailabilityAsync(Guid spaceId, DateTime startTime, DateTime endTime)
        {
            var bookings = await _bookingRepository.GetBySpaceIdAsync(spaceId);
            return !bookings.Any(b =>
                (startTime >= b.StartTime && startTime < b.EndTime) ||
                (endTime > b.StartTime && endTime <= b.EndTime) ||
                (startTime <= b.StartTime && endTime >= b.EndTime));
        }
       

        public async Task ProcessPaymentMessageAsync(PaymentProcessedMessage message)
        {
            _logger.LogInformation($"Traitement du message de paiement pour la réservation {message.BookingId}");

            try
            {
                var booking = await _bookingRepository.GetByIdAsync(message.BookingId);
                if (booking == null)
                {
                    _logger.LogWarning($"Réservation {message.BookingId} non trouvée pour le traitement du paiement");
                    return;
                }

                if (message.Status == "Completed")
                {
                    // Mettre à jour le statut de paiement
                    booking.PaymentStatus = "Paid";
                    if (booking.Status == BookingStatus.Pending)
                    {
                        booking.Status = BookingStatus.Confirmed;
                    }

                    await _bookingRepository.UpdateAsync(booking);
                    _logger.LogInformation($"Statut de paiement de la réservation {message.BookingId} mis à jour en 'Paid'");
                }
                else if (message.Status == "Failed")
                {
                    // Gérer l'échec du paiement
                    booking.PaymentStatus = "Failed";
                    await _bookingRepository.UpdateAsync(booking);
                    _logger.LogWarning($"Paiement échoué pour la réservation {message.BookingId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement du message de paiement: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessSpaceStatusChangeAsync(SpaceStatusChangedMessage message)
        {
            _logger.LogInformation($"Traitement du changement de statut pour l'espace {message.SpaceId}");

            try
            {
                // Si l'espace devient indisponible, gérer les réservations affectées
                if (message.NewStatus == "Unavailable" || message.NewStatus == "Maintenance")
                {
                    var affectedBookings = await _bookingRepository.GetBySpaceIdAsync(message.SpaceId);
                    var activeBookings = affectedBookings.Where(b =>
                        b.Status != BookingStatus.Cancelled &&
                        b.Status != BookingStatus.Completed &&
                        b.EndTime> DateTime.UtcNow);

                    foreach (var booking in activeBookings)
                    {
                        // Si la période de maintenance chevauche la période de réservation
                        if (booking.StartTime < message.EndDate && booking.EndTime > message.StartDate)
                        {
                            // Annuler la réservation
                            booking.Status = BookingStatus.Cancelled;
                            booking.CancellationReason = $"Espace indisponible: {message.Reason}";
                            await _bookingRepository.UpdateAsync(booking);

                            // Publier un événement d'annulation
                            _rabbitMQProducer.PublishMessage(
                                "booking_events",
                                "booking.cancelled",
                                new BookingCancelledMessage
                                {
                                    BookingId = booking.Id,
                                    UserId = booking.UserId,
                                    SpaceId = booking.SpaceId,
                                    StartDateTime = booking.StartTime,
                                    EndDateTime = booking.EndTime,
                                    CancellationTime = DateTime.UtcNow,
                                    CancellationReason = $"Espace indisponible: {message.Reason}"
                                });

                            _logger.LogInformation($"Réservation {booking.Id} annulée en raison du changement de statut de l'espace");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement du changement de statut d'espace: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessInvoiceCreatedAsync(InvoiceCreatedMessage message)
        {
            _logger.LogInformation($"Traitement de la facture créée {message.InvoiceId} pour l'utilisateur {message.UserId}");

            try
            {
                // Vous pourriez mettre à jour les réservations liées à cette facture
                // avec une référence vers la facture, ou tout autre traitement nécessaire

                _logger.LogInformation($"Facture {message.InvoiceId} traitée");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement de la facture créée: {ex.Message}");
                throw;
            }
        }
    }
    
    
}