// Repository/IDataRepository.cs
using SmartCowork.Services.AI.Data;
using SmartCowork.Services.AI.Models;

// Repository/IRecommendationRepository.cs
using SmartCowork.Services.AI.Models;

namespace SmartCowork.Services.AI.Repository
{
    public interface IRecommendationRepository
    {
        // Recommandations
        Task<Recommendation> GetRecommendationByIdAsync(Guid id);
        Task<IEnumerable<Recommendation>> GetRecommendationsForUserAsync(Guid userId, RecommendationType? type = null);
        Task<Recommendation> CreateRecommendationAsync(Recommendation recommendation);
        Task<bool> MarkRecommendationAsReadAsync(Guid id);
        Task<bool> DeleteRecommendationAsync(Guid id);

        // Préférences utilisateur
        Task<UserPreference> GetUserPreferenceAsync(Guid userId);
        Task UpdateUserPreferenceAsync(UserPreference preference);

        // Activités utilisateur
        Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(Guid userId);
        Task TrackUserActivityAsync(UserActivity activity);
        Task<IEnumerable<UserActivity>> GetActivitiesBySpaceAsync(Guid spaceId);
    }
}