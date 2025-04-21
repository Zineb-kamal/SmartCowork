

namespace SmartCowork.Services.Billing.Models.DTOs
{
    public class UpdateInvoiceDto
    {
        public Guid Id { get; set; }
        public DateTime? DueDate { get; set; }
        public InvoiceStatus? Status { get; set; }
    }

}