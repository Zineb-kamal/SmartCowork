

namespace SmartCowork.Services.Billing.Models.DTOs
{
    public class CreateInvoiceDto
    {
        public Guid UserId { get; set; }
        public DateTime? DueDate { get; set; } // Optionnel, pourrait être calculé automatiquement
        public List<CreateInvoiceItemDto> Items { get; set; }
    }

    public class CreateInvoiceItemDto
    {
        public Guid BookingId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }




}
