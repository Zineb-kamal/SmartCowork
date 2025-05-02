namespace SmartCowork.Services.Space.Models
{
    // Models/Space.cs
    public class Space
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        //public string Location { get; set; }
        public SpaceType Type { get; set; }
        public SpaceStatus Status { get; set; }
        public string ImageUrl { get; set; }
        //public List<string> Amenities { get; set; }
        // Pricing
        public decimal PricePerHour { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal PricePerYear { get; set; }
    }

    public enum SpaceType
    {
        Desk,
        MeetingRoom,
        PrivateOffice,
        ConferenceRoom
    }

    public enum SpaceStatus
    {
        Available,
        Occupied,
        Maintenance
    }
}
