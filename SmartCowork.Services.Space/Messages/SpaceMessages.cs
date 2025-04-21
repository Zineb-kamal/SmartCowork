// Messages/SpaceMessages.cs
using System;

namespace SmartCowork.Services.Space.Messages
{
    // Messages que Space va publier
    public class SpaceCreatedMessage
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string[] Amenities { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SpaceUpdatedMessage
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public string Location { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class SpaceStatusChangedMessage
    {
        public Guid SpaceId { get; set; }
        public string PreviousStatus { get; set; }
        public string NewStatus { get; set; }
        public string Reason { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } // Optionnel, pour maintenance temporaire
        public DateTime ChangedAt { get; set; }
    }

    public class SpaceDeletedMessage
    {
        public Guid SpaceId { get; set; }
        public string Name { get; set; }
        public DateTime DeletedAt { get; set; }
    }
}

