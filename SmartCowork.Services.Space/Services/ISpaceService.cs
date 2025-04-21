using SmartCowork.Services.Space.DTOs;
using SmartCowork.Services.Space.Messages;
using SmartCowork.Services.Space.Models;

namespace SmartCowork.Services.Space.Services
{
    public interface ISpaceService
    {
        Task<IEnumerable<Models.Space>> GetAllSpacesAsync();
        Task<Models.Space> GetSpaceByIdAsync(Guid id);
        Task<Models.Space> CreateSpaceAsync(Models.Space space);
        Task UpdateSpaceAsync(Models.Space space);
        Task DeleteSpaceAsync(Guid id);
        Task<IEnumerable<Models.Space>> GetAvailableSpacesAsync();
        Task<IEnumerable<Models.Space>> GetSpacesByTypeAsync(SpaceType type);
        Task ProcessBookingCreatedAsync(BookingCreatedMessage message);
        Task ProcessBookingCancelledAsync(BookingCancelledMessage message);
        Task<SpaceCreateDto> CreateSpaceAsync(SpaceCreateDto createSpaceDto);
    }
}
