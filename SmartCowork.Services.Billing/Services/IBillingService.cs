// Services/IBillingService.cs
using SmartCowork.Services.Billing.DTOs;
using SmartCowork.Services.Billing.Messages;
using SmartCowork.Services.Billing.Models;
using SmartCowork.Services.Billing.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartCowork.Services.Billing.Services
{
    public interface IBillingService
    {
        // Méthodes CRUD standard
        Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync();
        Task<IEnumerable<InvoiceDto>> GetUserInvoicesAsync(Guid userId);
        Task<InvoiceDto> GetInvoiceByIdAsync(Guid id);
        Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto createInvoiceDto);
        Task<InvoiceDto> UpdateInvoiceAsync(UpdateInvoiceDto updateInvoiceDto);
        Task<bool> DeleteInvoiceAsync(Guid id);

        // Méthodes de paiement
        Task<TransactionDto> ProcessPaymentAsync(CreateTransactionDto createTransactionDto);
        Task<IEnumerable<TransactionDto>> GetInvoiceTransactionsAsync(Guid invoiceId);
        Task<TransactionDto> ProcessRefundAsync(Guid transactionId, decimal amount, string reason);

        // Méthodes de génération de rapports
        Task<RevenueReportDto> GenerateRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<InvoiceDto>> GetOverdueInvoicesAsync();

        // Nouvelles méthodes pour traiter les messages RabbitMQ
        Task ProcessBookingCreatedAsync(BookingCreatedMessage message);
        Task ProcessBookingUpdatedAsync(BookingUpdatedMessage message);
        Task ProcessBookingCancelledAsync(BookingMessages message);
        Task ProcessBookingCompletedAsync(BookingCompletedMessage message);
    }
}