
namespace SmartCowork.Services.Billing.Models.DTOs
{
    public class CreateTransactionDto
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
    }

}