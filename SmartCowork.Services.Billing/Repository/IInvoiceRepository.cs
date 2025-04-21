using SmartCowork.Services.Billing.Models;

namespace SmartCowork.Services.Billing.Repository
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<IEnumerable<Invoice>> GetInvoicesByUserIdAsync(Guid userId);
        Task<Invoice> GetInvoiceByIdAsync(Guid id);
        Task<Invoice> CreateInvoiceAsync(Invoice invoice);
        Task UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(Guid id);

        // Méthodes spécifiques
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
        Task<bool> AddTransactionAsync(Transaction transaction);

        // Méthodes supplémentaires
        Task<IEnumerable<Invoice>> FindInvoicesByBookingIdAsync(Guid bookingId);
        Task<Invoice> FindInvoiceByTransactionIdAsync(Guid transactionId);
        Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate);

    }
}
