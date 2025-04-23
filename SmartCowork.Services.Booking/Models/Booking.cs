// Models/Booking.cs
namespace SmartCowork.Services.Booking.Models
{
    public class Booking
    {
        public Guid Id { get; set; }
        public Guid SpaceId { get; set; }
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string? Purpose { get; set; } 
        public int? AttendeesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? CancellationReason { get; internal set; }
        public string? PaymentStatus { get; internal set; }
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled,
        Completed
    }
}