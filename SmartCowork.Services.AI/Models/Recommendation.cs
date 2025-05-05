namespace SmartCowork.Services.AI.Models
{
    public class Recommendation
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public RecommendationType Type { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRead { get; set; }
        public double Confidence { get; set; }
        public DateTime ReadAt { get; internal set; }
    }

    public enum RecommendationType
    {
        Space,
        TimeSlot,
        Pricing,
        OccupancyPrediction
    }
}