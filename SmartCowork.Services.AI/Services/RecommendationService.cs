// Services/RecommendationService.cs - version modifiée pour SQL Server
using SmartCowork.Services.AI.DTOs;
using SmartCowork.Services.AI.Models;
using SmartCowork.Services.AI.Repository;
using System.Text.Json;

namespace SmartCowork.Services.AI.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IRecommendationRepository _repository;
        private readonly ILogger<RecommendationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiGatewayUrl;

        public RecommendationService(
            IRecommendationRepository repository,
            ILogger<RecommendationService> logger,
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _repository = repository;
            _logger = logger;
            _httpClient = httpClient;
            _apiGatewayUrl = configuration["ApiGateway:Url"] ?? "http://localhost:5072";
        }

        public async Task<IEnumerable<RecommendationDto>> GetRecommendationsForUserAsync(
            Guid userId, RecommendationType? type = null)
        {
            try
            {
                // Vérifier s'il existe déjà des recommandations pour cet utilisateur
                var existingRecommendations = await _repository.GetRecommendationsForUserAsync(userId, type);

                // Si pas de recommandations existantes ou moins de 3, en générer de nouvelles
                if (!existingRecommendations.Any() || existingRecommendations.Count() < 3)
                {
                    // Générer des recommandations
                    if (type == null || type == RecommendationType.Space)
                    {
                        var spaceRec = await GenerateSpaceRecommendationAsync(userId);
                    }

                    if (type == null || type == RecommendationType.TimeSlot)
                    {
                        // Pour cet exemple, nous générons pour le premier espace trouvé
                        var spaces = await GetSpacesAsync();
                        if (spaces.Any())
                        {
                            await GenerateTimeSlotRecommendationAsync(userId, spaces.First().Id);
                        }
                    }

                    if (type == null || type == RecommendationType.Pricing)
                    {
                        await GeneratePricingRecommendationAsync(userId);
                    }

                    // Récupérer les recommandations maintenant qu'elles ont été générées
                    existingRecommendations = await _repository.GetRecommendationsForUserAsync(userId, type);
                }

                // Mapper les recommandations vers DTOs
                return existingRecommendations.Select(r => new RecommendationDto
                {
                    Id = r.Id,
                    Type = r.Type.ToString(),
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    Confidence = r.Confidence
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating recommendations for user {userId}");
                return Enumerable.Empty<RecommendationDto>();
            }
        }

        public async Task<RecommendationDto> GenerateSpaceRecommendationAsync(Guid userId)
        {
            try
            {
                // Récupérer les préférences de l'utilisateur
                var preferences = await _repository.GetUserPreferenceAsync(userId);

                // Récupérer les espaces depuis le service Space
                var spaces = await GetSpacesAsync();

                if (!spaces.Any())
                    return null;

                // Récupérer l'historique des activités de l'utilisateur
                var userActivities = await _repository.GetUserActivitiesAsync(userId);

                // Analyser les espaces fréquemment utilisés
                var favoriteSpaces = userActivities
                    .Where(a => !a.WasCancelled)
                    .GroupBy(a => a.SpaceId)
                    .Select(g => new { SpaceId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .Select(x => x.SpaceId)
                    .ToList();

                // Filtrer les espaces selon les préférences
                var filteredSpaces = spaces;
                if (preferences != null)
                {
                    if (!string.IsNullOrEmpty(preferences.PreferredSpaceType))
                    {
                        filteredSpaces = filteredSpaces.Where(s =>
                            s.Type.ToString() == preferences.PreferredSpaceType).ToList();
                    }

                    if (preferences.PreferredCapacity.HasValue)
                    {
                        filteredSpaces = filteredSpaces.Where(s =>
                            s.Capacity >= preferences.PreferredCapacity.Value).ToList();
                    }
                }

                // S'il n'y a pas d'espaces correspondants, utiliser tous les espaces
                if (!filteredSpaces.Any())
                    filteredSpaces = spaces;

                // Calculer un score pour chaque espace
                var scoredSpaces = filteredSpaces.Select(space => {
                    double score = 0.5; // Score de base

                    // Bonus pour les espaces préférés
                    if (favoriteSpaces.Contains(space.Id))
                    {
                        score += 0.3;
                    }

                    // Bonus pour le type d'espace préféré
                    if (preferences != null && space.Type.ToString() == preferences.PreferredSpaceType)
                    {
                        score += 0.2;
                    }

                    return new { Space = space, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .ToList();

                // Sélectionner le meilleur espace
                var recommendedSpace = scoredSpaces.First().Space;
                var confidenceScore = scoredSpaces.First().Score;

                // Créer et sauvegarder la recommandation
                var recommendation = new Recommendation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = RecommendationType.Space,
                    Content = JsonSerializer.Serialize(new
                    {
                        SpaceId = recommendedSpace.Id,
                        SpaceName = recommendedSpace.Name,
                        SpaceType = recommendedSpace.Type,
                        Capacity = recommendedSpace.Capacity,
                        PricePerHour = recommendedSpace.PricePerHour,
                        Reason = favoriteSpaces.Contains(recommendedSpace.Id) ?
                            "Based on your booking history" :
                            "Based on your preferences"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRead = false,
                    Confidence = confidenceScore
                };

                var savedRecommendation = await _repository.CreateRecommendationAsync(recommendation);

                // Mapper vers DTO
                return new RecommendationDto
                {
                    Id = savedRecommendation.Id,
                    Type = savedRecommendation.Type.ToString(),
                    Content = savedRecommendation.Content,
                    CreatedAt = savedRecommendation.CreatedAt,
                    Confidence = savedRecommendation.Confidence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating space recommendation for user {userId}");
                return null;
            }
        }

        public async Task<RecommendationDto> GenerateTimeSlotRecommendationAsync(Guid userId, Guid spaceId)
        {
            try
            {
                // Récupérer les préférences de l'utilisateur
                var preferences = await _repository.GetUserPreferenceAsync(userId);

                // Récupérer l'historique des activités
                var userActivities = await _repository.GetUserActivitiesAsync(userId);

                // Analyser les horaires préférés
                var preferredDayOfWeek = DayOfWeek.Monday; // Par défaut
                var preferredHour = 10; // Par défaut

                if (userActivities.Any())
                {
                    // Calculer le jour de la semaine le plus fréquent
                    preferredDayOfWeek = userActivities
                        .GroupBy(a => a.StartTime.DayOfWeek)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault();

                    // Calculer l'heure de début la plus fréquente
                    preferredHour = userActivities
                        .GroupBy(a => a.StartTime.Hour)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault();
                }

                // Utiliser les préférences explicites si disponibles
                if (preferences != null)
                {
                    if (preferences.PreferredDayOfWeek.HasValue)
                    {
                        preferredDayOfWeek = preferences.PreferredDayOfWeek.Value;
                    }

                    if (preferences.PreferredStartTime.HasValue)
                    {
                        preferredHour = preferences.PreferredStartTime.Value.Hours;
                    }
                }

                // Calculer la prochaine date avec le jour de la semaine préféré
                var today = DateTime.UtcNow.Date;
                var daysToAdd = ((int)preferredDayOfWeek - (int)today.DayOfWeek + 7) % 7;
                if (daysToAdd == 0 && DateTime.UtcNow.Hour >= preferredHour)
                {
                    daysToAdd = 7; // Passer à la semaine suivante si l'heure est déjà passée aujourd'hui
                }

                var recommendedDate = today.AddDays(daysToAdd);
                var startDateTime = recommendedDate.AddHours(preferredHour);
                var endDateTime = startDateTime.AddHours(2); // Durée par défaut = 2 heures

                // Créer et sauvegarder la recommandation
                var recommendation = new Recommendation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = RecommendationType.TimeSlot,
                    Content = JsonSerializer.Serialize(new
                    {
                        SpaceId = spaceId,
                        StartTime = startDateTime,
                        EndTime = endDateTime,
                        DayOfWeek = startDateTime.DayOfWeek.ToString(),
                        StartHour = startDateTime.Hour,
                        Reason = userActivities.Any() ?
                            "Based on your booking patterns" :
                            "Suggested time slot for optimal availability"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRead = false,
                    Confidence = userActivities.Any() ? 0.8 : 0.6
                };

                var savedRecommendation = await _repository.CreateRecommendationAsync(recommendation);

                // Mapper vers DTO
                return new RecommendationDto
                {
                    Id = savedRecommendation.Id,
                    Type = savedRecommendation.Type.ToString(),
                    Content = savedRecommendation.Content,
                    CreatedAt = savedRecommendation.CreatedAt,
                    Confidence = savedRecommendation.Confidence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating time slot recommendation for user {userId} and space {spaceId}");
                return null;
            }
        }

        public async Task<RecommendationDto> GeneratePricingRecommendationAsync(Guid userId)
        {
            try
            {
                // Récupérer l'historique des activités
                var userActivities = await _repository.GetUserActivitiesAsync(userId);

                // Analyser le modèle d'utilisation pour déterminer le meilleur plan tarifaire
                string recommendedPlan = "Hourly"; // Plan par défaut
                int discountPercentage = 0;
                double confidence = 0.5;

                if (userActivities.Count() > 10)
                {
                    // Calculer la durée totale de réservation
                    var totalHours = userActivities.Sum(a =>
                        (a.EndTime - a.StartTime).TotalHours);

                    // Calculer le nombre moyen de réservations par semaine
                    var firstActivity = userActivities.Min(a => a.StartTime);
                    var weeks = Math.Max(1, (DateTime.UtcNow - firstActivity).TotalDays / 7);
                    var reservationsPerWeek = userActivities.Count() / weeks;

                    if (reservationsPerWeek >= 3)
                    {
                        recommendedPlan = "Weekly";
                        discountPercentage = 15;
                        confidence = 0.85;
                    }
                    else if (totalHours > 40)
                    {
                        recommendedPlan = "Monthly";
                        discountPercentage = 20;
                        confidence = 0.8;
                    }
                    else if (totalHours > 10)
                    {
                        recommendedPlan = "Daily";
                        discountPercentage = 10;
                        confidence = 0.75;
                    }
                }

                // Créer et sauvegarder la recommandation
                var recommendation = new Recommendation
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = RecommendationType.Pricing,
                    Content = JsonSerializer.Serialize(new
                    {
                        PlanType = recommendedPlan,
                        Discount = discountPercentage,
                        Reason = userActivities.Count() > 10 ?
                            $"Based on your usage of {userActivities.Count()} bookings" :
                            "Suggested pricing plan for new users"
                    }),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30), // Recommandation tarifaire valide plus longtemps
                    IsRead = false,
                    Confidence = confidence
                };

                var savedRecommendation = await _repository.CreateRecommendationAsync(recommendation);

                // Mapper vers DTO
                return new RecommendationDto
                {
                    Id = savedRecommendation.Id,
                    Type = savedRecommendation.Type.ToString(),
                    Content = savedRecommendation.Content,
                    CreatedAt = savedRecommendation.CreatedAt,
                    Confidence = savedRecommendation.Confidence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating pricing recommendation for user {userId}");
                return null;
            }
        }

        public async Task<UserPreference> GetUserPreferencesAsync(Guid userId)
        {
            return await _repository.GetUserPreferenceAsync(userId);
        }

        public async Task UpdateUserPreferencesAsync(Guid userId, UserPreferenceUpdateDto preferencesDto)
        {
            var existingPreferences = await _repository.GetUserPreferenceAsync(userId);

            if (existingPreferences == null)
            {
                existingPreferences = new UserPreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId
                };
            }

            // Mettre à jour les préférences
            if (preferencesDto.PreferredSpaceType != null)
                existingPreferences.PreferredSpaceType = preferencesDto.PreferredSpaceType;

            if (preferencesDto.PreferredCapacity.HasValue)
                existingPreferences.PreferredCapacity = preferencesDto.PreferredCapacity;

            if (preferencesDto.PreferredDayOfWeek.HasValue)
                existingPreferences.PreferredDayOfWeek = preferencesDto.PreferredDayOfWeek;

            if (preferencesDto.PreferredStartTime.HasValue)
                existingPreferences.PreferredStartTime = preferencesDto.PreferredStartTime;

            if (preferencesDto.PreferredDuration.HasValue)
                existingPreferences.PreferredDuration = preferencesDto.PreferredDuration;

            if (preferencesDto.FeaturePreferences != null)
                existingPreferences.FeaturePreferences = preferencesDto.FeaturePreferences;

            await _repository.UpdateUserPreferenceAsync(existingPreferences);

            // Invalider les recommandations existantes pour forcer la génération de nouvelles
            // basées sur les préférences mises à jour
            // (Cette fonctionnalité pourrait être implémentée dans le repository)
        }

        public async Task TrackUserActivityAsync(UserActivity activity)
        {
            await _repository.TrackUserActivityAsync(activity);

            // Analyser l'activité pour générer de nouvelles recommandations
            // (optionnel, pourrait être fait périodiquement par un job)
        }

        public async Task<IEnumerable<RecommendationDto>> GetTrendingSpacesAsync()
        {
            try
            {
                // Récupérer tous les espaces
                var spaces = await GetSpacesAsync();

                if (!spaces.Any())
                    return Enumerable.Empty<RecommendationDto>();

                // Récupérer toutes les activités pour calculer les tendances
                var allActivities = new List<UserActivity>();
                foreach (var space in spaces)
                {
                    var activities = await _repository.GetActivitiesBySpaceAsync(space.Id);
                    allActivities.AddRange(activities);
                }

                // Calculer les espaces les plus utilisés ces 30 derniers jours
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                var spaceUsage = allActivities
                    .Where(a => !a.WasCancelled && a.StartTime >= thirtyDaysAgo)
                    .GroupBy(a => a.SpaceId)
                    .Select(g => new { SpaceId = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(3)
                    .ToList();

                // S'il n'y a pas assez d'activités, utiliser des espaces au hasard
                if (spaceUsage.Count < 3)
                {
                    var randomSpaces = spaces
                        .Where(s => !spaceUsage.Any(u => u.SpaceId == s.Id))
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(3 - spaceUsage.Count)
                        .Select(s => new { SpaceId = s.Id, Count = 0 });

                    spaceUsage.AddRange(randomSpaces);
                }

                // Créer des recommandations "trending"
                var recommendations = new List<RecommendationDto>();
                foreach (var usage in spaceUsage)
                {
                    var space = spaces.FirstOrDefault(s => s.Id == usage.SpaceId);
                    if (space == null) continue;

                    // Créer une recommandation générique (non liée à un utilisateur spécifique)
                    var recommendation = new Recommendation
                    {
                        Id = Guid.NewGuid(),
                        UserId = Guid.Empty, // Pas d'utilisateur spécifique
                        Type = RecommendationType.Space,
                        Content = JsonSerializer.Serialize(new
                        {
                            SpaceId = space.Id,
                            SpaceName = space.Name,
                            SpaceType = space.Type,
                            BookingCount = usage.Count,
                            Reason = usage.Count > 0 ? "Popular among users" : "New and exciting"
                        }),
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddDays(1), // Actualisation quotidienne
                        IsRead = false,
                        Confidence = usage.Count > 0 ? 0.9 : 0.6
                    };

                    recommendations.Add(new RecommendationDto
                    {
                        Id = recommendation.Id,
                        Type = "Trending",
                        Content = recommendation.Content,
                        CreatedAt = recommendation.CreatedAt,
                        Confidence = recommendation.Confidence
                    });
                }

                return recommendations;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending spaces");
                return Enumerable.Empty<RecommendationDto>();
            }
        }

        // Méthode utilitaire pour récupérer les espaces
        private async Task<List<SpaceInfo>> GetSpacesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiGatewayUrl}/api/space");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get spaces: {response.StatusCode}");
                    return new List<SpaceInfo>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var spaces = JsonSerializer.Deserialize<List<SpaceInfo>>(content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return spaces ?? new List<SpaceInfo>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting spaces from API");
                return new List<SpaceInfo>();
            }
        }
        public async Task<bool> MarkRecommendationAsReadAsync(Guid recommendationId)
        {
            try
            {
                return await _repository.MarkRecommendationAsReadAsync(recommendationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking recommendation {recommendationId} as read");
                return false;
            }
        }

        public async Task<bool> DeleteRecommendationAsync(Guid recommendationId)
        {
            try
            {
                return await _repository.DeleteRecommendationAsync(recommendationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting recommendation {recommendationId}");
                return false;
            }
        }
    }

    // Classe utilitaire pour représenter un espace
    public class SpaceInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public decimal PricePerHour { get; set; }
    }
}