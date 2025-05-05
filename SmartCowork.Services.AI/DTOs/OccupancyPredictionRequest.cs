// Additional DTOs
namespace SmartCowork.Services.AI.DTOs
{
    public class OccupancyPredictionRequest
    {
        public Guid SpaceId { get; set; }
        public DateTime Date { get; set; }
    }

    public class OccupancyPrediction
    {
        public Guid SpaceId { get; set; }
        public DateTime Date { get; set; }
        public float PredictedOccupancyRate { get; set; }
        public float Confidence { get; set; }
    }
}

