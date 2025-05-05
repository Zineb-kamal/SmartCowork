// DTOs/RecommendationDto.cs
namespace SmartCowork.Services.AI.DTOs
{
    public class RecommendationDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public double Confidence { get; set; }
    }

    public class UserPreferenceUpdateDto
    {
        public string PreferredSpaceType { get; set; }
        public int? PreferredCapacity { get; set; }
        public DayOfWeek? PreferredDayOfWeek { get; set; }
        public TimeSpan? PreferredStartTime { get; set; }
        public TimeSpan? PreferredDuration { get; set; }
        public Dictionary<string, double> FeaturePreferences { get; set; }
    }
}