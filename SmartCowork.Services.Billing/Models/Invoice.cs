

namespace SmartCowork.Services.Billing.Models
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum InvoiceStatus
    {
        Pending,
        Paid,
        Cancelled,
        Overdue
    }




}




