using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.Space.Data;
using SmartCowork.Services.Space.Models;

namespace SmartCowork.Services.Space.Repository
{
    public class SpaceRepository : ISpaceRepository
    {
        private readonly ApplicationDbContext _context;

        public SpaceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.Space>> GetAllAsync()
        {
            return await _context.Spaces.ToListAsync();
        }

        public async Task<Models.Space> GetByIdAsync(Guid id)
        {
            return await _context.Spaces.FindAsync(id);
        }

        public async Task<Models.Space> CreateAsync(Models.Space space)
        {
            await _context.Spaces.AddAsync(space);
            await _context.SaveChangesAsync();
            return space;
        }

        public async Task UpdateAsync(Models.Space space)
        {
            _context.Spaces.Update(space);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var space = await _context.Spaces.FindAsync(id);
            if (space != null)
            {
                _context.Spaces.Remove(space);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Models.Space>> GetAvailableSpacesAsync()
        {
            return await _context.Spaces
                .Where(s => s.Status == SpaceStatus.Available)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Space>> GetByTypeAsync(SpaceType type)
        {
            return await _context.Spaces
                .Where(s => s.Type == type)
                .ToListAsync();
        }
    }
}
