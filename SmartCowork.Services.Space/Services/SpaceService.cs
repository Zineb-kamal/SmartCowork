using AutoMapper;
using SmartCowork.Common.Messaging.RabbitMQ;
using SmartCowork.Services.Space.DTOs;
using SmartCowork.Services.Space.Messages;
using SmartCowork.Services.Space.Models;
using SmartCowork.Services.Space.Repository;

namespace SmartCowork.Services.Space.Services
{
    public class SpaceService : ISpaceService
    {
        private readonly ISpaceRepository _spaceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<SpaceService> _logger;
        private readonly IRabbitMQProducer _rabbitMQProducer;

        public SpaceService(ISpaceRepository spaceRepository, IMapper mapper,
            ILogger<SpaceService> logger,
            IRabbitMQProducer rabbitMQProducer)
        {
            _spaceRepository = spaceRepository;
            _mapper = mapper;
            _logger = logger;
            _rabbitMQProducer = rabbitMQProducer;
        }

        public async Task<IEnumerable<Models.Space>> GetAllSpacesAsync()
        {
            return await _spaceRepository.GetAllAsync();
        }

        public async Task<Models.Space> GetSpaceByIdAsync(Guid id)
        {
            return await _spaceRepository.GetByIdAsync(id);
        }

        public async Task<Models.Space> CreateSpaceAsync(Models.Space space)
        {
            return await _spaceRepository.CreateAsync(space);
        }

        public async Task UpdateSpaceAsync(Models.Space space)
        {
            await _spaceRepository.UpdateAsync(space);
        }

        public async Task DeleteSpaceAsync(Guid id)
        {
            await _spaceRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<Models.Space>> GetAvailableSpacesAsync()
        {
            return await _spaceRepository.GetAvailableSpacesAsync();
        }

        public async Task<IEnumerable<Models.Space>> GetSpacesByTypeAsync(SpaceType type)
        {
            return await _spaceRepository.GetByTypeAsync(type);
        }
        // Méthode modifiée pour publier un message lors de la création d'un espace
        public async Task<SpaceCreateDto> CreateSpaceAsync(SpaceCreateDto createSpaceDto)
        {
            // Logique existante pour créer un espace
            var space = _mapper.Map<Models.Space>(createSpaceDto);
            var createdSpace = await _spaceRepository.CreateAsync(space);

            // Publier un message
            try
            {
                _rabbitMQProducer.PublishMessage(
                    "space_events",
                    "space.created",
                    new SpaceCreatedMessage
                    {
                        SpaceId = createdSpace.Id,
                        Name = createdSpace.Name,
                        Description = createdSpace.Description,
                        Capacity = createdSpace.Capacity,
                        HourlyRate = createdSpace.PricePerHour,
                        //Location = createdSpace.Location,
                        Status = createdSpace.Status.ToString(),
                        Type = createdSpace.Type.ToString(),
                        //Amenities = createdSpace.Amenities.ToArray(),
                        CreatedAt = DateTime.UtcNow
                    });

                _logger.LogInformation($"Event publié: espace {createdSpace.Id} créé");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Impossible de publier l'événement de création d'espace: {ex.Message}");
                // Continue malgré l'erreur
            }

            return _mapper.Map<SpaceCreateDto>(createdSpace);
        }

        // Méthode modifiée pour publier un message lors de la mise à jour du statut
        public async Task<SpaceUpdateDto> UpdateSpaceStatusAsync(Guid id, string newStatus, string reason = null, DateTime? endDate = null)
        {
            var space = await _spaceRepository.GetByIdAsync(id);
            if (space == null)
                throw new KeyNotFoundException($"Espace avec ID {id} non trouvé");

            var previousStatus = space.Status.ToString();

            // Mettre à jour le statut
            if (Enum.TryParse<SpaceStatus>(newStatus, true, out var statusEnum))
            {
                space.Status = statusEnum;
                await _spaceRepository.UpdateAsync(space);

                // Publier un message
                try
                {
                    _rabbitMQProducer.PublishMessage(
                        "space_events",
                        "space.status_changed",
                        new SpaceStatusChangedMessage
                        {
                            SpaceId = space.Id,
                            PreviousStatus = previousStatus,
                            NewStatus = newStatus,
                            Reason = reason,
                            StartDate = DateTime.UtcNow,
                            EndDate = endDate,
                            ChangedAt = DateTime.UtcNow
                        });

                    _logger.LogInformation($"Event publié: statut de l'espace {id} changé de {previousStatus} à {newStatus}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Impossible de publier l'événement de changement de statut: {ex.Message}");
                    // Continue malgré l'erreur
                }
            }
            else
            {
                throw new ArgumentException($"Statut invalide: {newStatus}");
            }

            return _mapper.Map<SpaceUpdateDto>(space);
        }

        // Nouvelles méthodes pour traiter les messages RabbitMQ
        public async Task ProcessBookingCreatedAsync(BookingCreatedMessage message)
        {
            _logger.LogInformation($"Traitement de la réservation {message.BookingId} pour l'espace {message.SpaceId}");

            try
            {
                var space = await _spaceRepository.GetByIdAsync(message.SpaceId);
                if (space == null)
                {
                    _logger.LogWarning($"Espace {message.SpaceId} non trouvé pour la réservation {message.BookingId}");
                    return;
                }

                // Ici, vous pourriez mettre à jour les disponibilités de l'espace
                // Par exemple, ajouter cette réservation à une liste de créneaux occupés

                _logger.LogInformation($"Réservation {message.BookingId} traitée pour l'espace {message.SpaceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement de la réservation {message.BookingId}: {ex.Message}");
                throw;
            }
        }

        public async Task ProcessBookingCancelledAsync(BookingCancelledMessage message)
        {
            _logger.LogInformation($"Traitement de l'annulation de réservation {message.BookingId} pour l'espace {message.SpaceId}");

            try
            {
                var space = await _spaceRepository.GetByIdAsync(message.SpaceId);
                if (space == null)
                {
                    _logger.LogWarning($"Espace {message.SpaceId} non trouvé pour l'annulation de réservation {message.BookingId}");
                    return;
                }

                // Ici, vous pourriez mettre à jour les disponibilités de l'espace
                // Par exemple, libérer le créneau occupé par cette réservation

                _logger.LogInformation($"Annulation de réservation {message.BookingId} traitée pour l'espace {message.SpaceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du traitement de l'annulation de réservation {message.BookingId}: {ex.Message}");
                throw;
            }
        }
    }
}
