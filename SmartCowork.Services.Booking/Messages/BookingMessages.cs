namespace SmartCowork.Services.Booking.Messages
{
    // Messages publiés par Booking
    public class BookingCreatedMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public string SpaceName { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Purpose { get; set; }
        public int AttendeesCount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookingUpdatedMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Purpose { get; set; }
        public int AttendeesCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class BookingMessages
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime CancellationTime { get; set; }
    }
    public class BookingCancelledMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime CancellationTime { get; set; }
        public string CancellationReason { get; set; }
    }

    public class BookingCompletedMessage
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}


