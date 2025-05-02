

namespace SmartCowork.Services.Billing.Models.DTOs
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } // Ajouté pour l'affichage
        public DateTime CreatedDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } // Représentation sous forme de chaîne
        public List<InvoiceItemDto> Items { get; set; }
        public List<TransactionDto> Transactions { get; set; }
        public string UserFirstName { get; set; }
        public string UserLastName { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
    }
    public class InvoiceItemDto
    {
        public Guid Id { get; set; }
        public Guid BookingId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
    public class TransactionDto
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string ReferenceNumber { get; set; }
    }
}