using SmartCowork.Services.Space.Models;

namespace SmartCowork.Services.Space.Repository
{
    public interface ISpaceRepository
    {
        Task<IEnumerable<Models.Space>> GetAllAsync();
        Task<Models.Space> GetByIdAsync(Guid id);
        Task<Models.Space> CreateAsync(Models.Space space);
        Task UpdateAsync(Models.Space space);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Models.Space>> GetAvailableSpacesAsync();
        Task<IEnumerable<Models.Space>> GetByTypeAsync(SpaceType type);
    }

}
