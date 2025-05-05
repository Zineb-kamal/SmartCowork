// Models/DTOs
namespace SmartCowork.Services.AI.DTOs
{
    public class WorkspaceRecommendationRequest
    {
        public Guid UserId { get; set; }
        public int? DesiredCapacity { get; set; }
        public DateTime? PreferredDate { get; set; }
        public TimeSpan? PreferredDuration { get; set; }
        public string? Purpose { get; set; }
        public decimal? MaxBudget { get; set; }
    }

    public class WorkspaceRecommendation
    {
        public Guid SpaceId { get; set; }
        public string SpaceName { get; set; }
        public string Description { get; set; }
        public decimal RecommendationScore { get; set; } // 0-1 score
        public string RecommendationReason { get; set; }
        public decimal EstimatedPrice { get; set; }
    }

    public class PriceRecommendationRequest
    {
        public Guid SpaceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PricingRecommendation
    {
        public decimal RecommendedHourlyRate { get; set; }
        public decimal RecommendedDailyRate { get; set; }
        public decimal CurrentOccupancyRate { get; set; } // 0-1
        public decimal SeasonalAdjustment { get; set; } // multiplier
        public string Rationale { get; set; }
    }
}