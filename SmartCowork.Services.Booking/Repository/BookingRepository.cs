using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.Booking.Data;

namespace SmartCowork.Services.Booking.Repository
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Models.Booking>> GetAllAsync()
        {
            return await _context.Bookings.ToListAsync();
        }

        public async Task<Models.Booking> GetByIdAsync(Guid id)
        {
            return await _context.Bookings.FindAsync(id);
        }

        public async Task<IEnumerable<Models.Booking>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Models.Booking>> GetBySpaceIdAsync(Guid spaceId)
        {
            return await _context.Bookings
                .Where(b => b.SpaceId == spaceId)
                .ToListAsync();
        }

        public async Task<Models.Booking> CreateAsync(Models.Booking booking)
        {
            booking.CreatedAt = DateTime.UtcNow;
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task UpdateAsync(Models.Booking booking)
        {
            booking.UpdatedAt = DateTime.UtcNow;
            // L'entité est déjà suivie, pas besoin de _context.Bookings.Update()
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
        }
    }
}