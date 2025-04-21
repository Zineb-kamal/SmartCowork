using SmartCowork.Services.Booking.Messages;

namespace SmartCowork.Services.Booking.Services
{
    public interface IBookingService
    {
        Task<IEnumerable<Models.Booking>> GetAllBookingsAsync();
        Task<Models.Booking> GetBookingByIdAsync(Guid id);
        Task<IEnumerable<Models.Booking>> GetBookingsByUserAsync(Guid userId);
        Task<IEnumerable<Models.Booking>> GetBookingsBySpaceAsync(Guid spaceId);
        Task<Models.Booking> CreateBookingAsync(Models.Booking booking);
        Task UpdateBookingAsync(Models.Booking booking);
        Task DeleteBookingAsync(Guid id);
        Task<bool> CheckSpaceAvailabilityAsync(Guid spaceId, DateTime startTime, DateTime endTime);

        Task ProcessPaymentMessageAsync(PaymentProcessedMessage message);
        Task ProcessSpaceStatusChangeAsync(SpaceStatusChangedMessage message);
        Task ProcessInvoiceCreatedAsync(InvoiceCreatedMessage message);
    }
}