

namespace SmartCowork.Services.Billing.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public TransactionStatus Status { get; set; }
        public string ReferenceNumber { get; set; }
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        Cash,
        PayPal
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Refunded
    }
}