// Services/IRecommendationService.cs
using SmartCowork.Services.AI.DTOs;
using SmartCowork.Services.AI.Models;

namespace SmartCowork.Services.AI.Services
{
    public interface IRecommendationService
    {
        // Méthodes pour obtenir et générer des recommandations
        Task<IEnumerable<RecommendationDto>> GetRecommendationsForUserAsync(Guid userId, RecommendationType? type = null);
        Task<RecommendationDto> GenerateSpaceRecommendationAsync(Guid userId);
        Task<RecommendationDto> GenerateTimeSlotRecommendationAsync(Guid userId, Guid spaceId);
        Task<RecommendationDto> GeneratePricingRecommendationAsync(Guid userId);

        // Méthodes pour gérer les préférences utilisateur
        Task<UserPreference> GetUserPreferencesAsync(Guid userId);
        Task UpdateUserPreferencesAsync(Guid userId, UserPreferenceUpdateDto preferences);

        // Méthode pour suivre l'activité des utilisateurs
        Task TrackUserActivityAsync(UserActivity activity);

        // Méthode pour obtenir les espaces tendance
        Task<IEnumerable<RecommendationDto>> GetTrendingSpacesAsync();

        // Méthode pour marquer une recommandation comme lue
        Task<bool> MarkRecommendationAsReadAsync(Guid recommendationId);

        // Méthode pour supprimer une recommandation
        Task<bool> DeleteRecommendationAsync(Guid recommendationId);
    }
}