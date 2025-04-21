using System;

namespace SmartCowork.Services.Booking.Messages
{
    // Messages reçus par Booking
    public class PaymentProcessedMessage
    {
        public Guid InvoiceId { get; set; }
        public Guid BookingId { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime ProcessedAt { get; set; }
    }

    public class SpaceStatusChangedMessage
    {
        public Guid SpaceId { get; set; }
        public string NewStatus { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Reason { get; set; }
    }
    public class InvoiceCreatedMessage
    {
        public Guid InvoiceId { get; set; }
        public Guid UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}