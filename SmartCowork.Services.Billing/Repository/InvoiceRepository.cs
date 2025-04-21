using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.Billing.Data;
using SmartCowork.Services.Billing.Models;



namespace SmartCowork.Services.Billing.Repository
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByUserIdAsync(Guid userId)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .Where(i => i.UserId == userId)
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            invoice.Id = Guid.NewGuid();
            invoice.CreatedDate = DateTime.UtcNow;

            // Calculer le montant total à partir des éléments
            invoice.TotalAmount = invoice.Items.Sum(item => item.TotalPrice);

            // Ajouter l'ID à chaque élément
            foreach (var item in invoice.Items)
            {
                item.Id = Guid.NewGuid();
                item.InvoiceId = invoice.Id;
            }

            await _context.Invoices.AddAsync(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task UpdateInvoiceAsync(Invoice invoice)
        {
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteInvoiceAsync(Guid id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate < today)
                .ToListAsync();
        }

        public async Task<bool> AddTransactionAsync(Transaction transaction)
        {
            transaction.Id = Guid.NewGuid();
            transaction.Date = DateTime.UtcNow;

            var invoice = await _context.Invoices.FindAsync(transaction.InvoiceId);
            if (invoice == null)
                return false;

            await _context.Transactions.AddAsync(transaction);

            // Si le paiement est complet, mettre à jour le statut de la facture
            if (transaction.Status == TransactionStatus.Completed)
            {
                var invoiceTransactions = await _context.Transactions
                    .Where(t => t.InvoiceId == transaction.InvoiceId && t.Status == TransactionStatus.Completed)
                    .ToListAsync();

                decimal totalPaid = invoiceTransactions.Sum(t => t.Amount) + transaction.Amount;

                if (totalPaid >= invoice.TotalAmount)
                {
                    invoice.Status = InvoiceStatus.Paid;
                    _context.Invoices.Update(invoice);
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<IEnumerable<Invoice>> FindInvoicesByBookingIdAsync(Guid bookingId)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .Where(i => i.Items.Any(item => item.BookingId == bookingId))
                .ToListAsync();
        }

        public async Task<Invoice> FindInvoiceByTransactionIdAsync(Guid transactionId)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(i => i.Transactions.Any(t => t.Id == transactionId));
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.Transactions)
                .Where(i => i.CreatedDate >= startDate && i.CreatedDate <= endDate)
                .ToListAsync();
        }

    }
}