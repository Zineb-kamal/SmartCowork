// Messages/BookingMessages.cs
using System;

namespace SmartCowork.Services.Space.Messages
{
    // Messages que Space va recevoir
    public class BookingCreatedMessage
    {
        public Guid BookingId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public Guid UserId { get; set; }
    }

    public class BookingCancelledMessage
    {
        public Guid BookingId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime CancellationTime { get; set; }
    }
}
