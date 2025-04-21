namespace SmartCowork.Services.Booking.Repository
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Models.Booking>> GetAllAsync();
        Task<Models.Booking> GetByIdAsync(Guid id);
        Task<IEnumerable<Models.Booking>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Models.Booking>> GetBySpaceIdAsync(Guid spaceId);
        Task<Models.Booking> CreateAsync(Models.Booking booking);
        Task UpdateAsync(Models.Booking booking);
        Task DeleteAsync(Guid id);
    }
}
