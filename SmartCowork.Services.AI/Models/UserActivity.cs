// Models/UserActivity.cs
namespace SmartCowork.Services.AI.Models
{
    public class UserActivity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public Guid SpaceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool WasCancelled { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}