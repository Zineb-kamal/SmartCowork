// Repository/RecommendationRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartCowork.Services.AI.Data;
using SmartCowork.Services.AI.Models;

namespace SmartCowork.Services.AI.Repository
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RecommendationRepository> _logger;

        public RecommendationRepository(
            ApplicationDbContext context,
            ILogger<RecommendationRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Recommandations
        public async Task<Recommendation> GetRecommendationByIdAsync(Guid id)
        {
            return await _context.Recommendations.FindAsync(id);
        }

        public async Task<IEnumerable<Recommendation>> GetRecommendationsForUserAsync(Guid userId, RecommendationType? type = null)
        {
            var query = _context.Recommendations
                .Where(r => r.UserId == userId && r.ExpiresAt > DateTime.UtcNow);

            if (type.HasValue)
                query = query.Where(r => r.Type == type.Value);

            return await query.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<Recommendation> CreateRecommendationAsync(Recommendation recommendation)
        {
            recommendation.CreatedAt = DateTime.UtcNow;
            if (recommendation.ExpiresAt == default)
                recommendation.ExpiresAt = DateTime.UtcNow.AddDays(7); // Recommandation valide 7 jours par défaut

            await _context.Recommendations.AddAsync(recommendation);
            await _context.SaveChangesAsync();
            return recommendation;
        }

        public async Task<bool> MarkRecommendationAsReadAsync(Guid id)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);
            if (recommendation == null) return false;

            recommendation.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRecommendationAsync(Guid id)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);
            if (recommendation == null) return false;

            _context.Recommendations.Remove(recommendation);
            await _context.SaveChangesAsync();
            return true;
        }

        // Préférences utilisateur
        public async Task<UserPreference> GetUserPreferenceAsync(Guid userId)
        {
            return await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateUserPreferenceAsync(UserPreference preference)
        {
            var existing = await _context.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == preference.UserId);

            if (existing == null)
            {
                await _context.UserPreferences.AddAsync(preference);
            }
            else
            {
                // Mettre à jour les propriétés
                _context.Entry(existing).CurrentValues.SetValues(preference);
            }

            await _context.SaveChangesAsync();
        }

        // Activités utilisateur
        public async Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(Guid userId)
        {
            return await _context.UserActivities
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task TrackUserActivityAsync(UserActivity activity)
        {
            activity.CreatedAt = DateTime.UtcNow;
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserActivity>> GetActivitiesBySpaceAsync(Guid spaceId)
        {
            return await _context.UserActivities
                .Where(a => a.SpaceId == spaceId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}