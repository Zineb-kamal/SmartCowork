// Messages/BillingMessages.cs

namespace SmartCowork.Services.Billing.Messages
{
    // Messages publiés par le service Billing
    public class InvoiceCreatedMessage
    {
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }
        public List<InvoiceItemInfo> Items { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class InvoiceItemInfo
    {
        public Guid BookingId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    public class InvoicePaidMessage
    {
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaidAt { get; set; }
    }

    public class PaymentProcessedMessage
    {
        public Guid PaymentId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; } // "Completed", "Failed", "Refunded"
        public string ReferenceNumber { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class InvoiceCancelledMessage
    {
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public string Reason { get; set; }
        public DateTime CancelledAt { get; set; }
    }
}

