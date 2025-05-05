// Controllers/RecommendationController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCowork.Services.AI.DTOs;
using SmartCowork.Services.AI.Models;
using SmartCowork.Services.AI.Services;
using System.Security.Claims;

namespace SmartCowork.Services.AI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<RecommendationController> _logger;

        public RecommendationController(
            IRecommendationService recommendationService,
            ILogger<RecommendationController> logger)
        {
            _recommendationService = recommendationService;
            _logger = logger;
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetUserRecommendations(
            [FromQuery] string type = null)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                RecommendationType? recommendationType = null;
                if (!string.IsNullOrEmpty(type) && Enum.TryParse<RecommendationType>(type, true, out var parsedType))
                {
                    recommendationType = parsedType;
                }

                var recommendations = await _recommendationService.GetRecommendationsForUserAsync(userId, recommendationType);
                return Ok(recommendations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user recommendations");
                return StatusCode(500, "An error occurred while retrieving recommendations");
            }
        }

        [HttpGet("trending")]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> GetTrendingSpaces()
        {
            try
            {
                var trending = await _recommendationService.GetTrendingSpacesAsync();
                return Ok(trending);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending spaces");
                return StatusCode(500, "An error occurred while retrieving trending spaces");
            }
        }

        [HttpGet("space/{userId}")]
        [Authorize]
        public async Task<ActionResult<RecommendationDto>> GetSpaceRecommendation(Guid userId)
        {
            try
            {
                // Vérifier que l'utilisateur courant a les droits pour cette requête
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                // Vérifier que l'utilisateur demande ses propres recommandations ou est admin
                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only get recommendations for yourself");
                }

                var recommendation = await _recommendationService.GenerateSpaceRecommendationAsync(userId);
                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating space recommendation for user {userId}");
                return StatusCode(500, "An error occurred while generating recommendations");
            }
        }

        [HttpGet("timeslot/{userId}/{spaceId}")]
        [Authorize]
        public async Task<ActionResult<RecommendationDto>> GetTimeSlotRecommendation(Guid userId, Guid spaceId)
        {
            try
            {
                // Vérification similaire à la méthode précédente
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only get recommendations for yourself");
                }

                var recommendation = await _recommendationService.GenerateTimeSlotRecommendationAsync(userId, spaceId);
                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating time slot recommendation for user {userId} and space {spaceId}");
                return StatusCode(500, "An error occurred while generating recommendations");
            }
        }

        [HttpGet("pricing/{userId}")]
        [Authorize]
        public async Task<ActionResult<RecommendationDto>> GetPricingRecommendation(Guid userId)
        {
            try
            {
                // Vérification similaire
                var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserIdClaim) || !Guid.TryParse(currentUserIdClaim, out var currentUserId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                if (userId != currentUserId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only get recommendations for yourself");
                }

                var recommendation = await _recommendationService.GeneratePricingRecommendationAsync(userId);
                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating pricing recommendation for user {userId}");
                return StatusCode(500, "An error occurred while generating recommendations");
            }
        }

        [HttpGet("preferences")]
        [Authorize]
        public async Task<ActionResult<UserPreference>> GetUserPreferences()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                var preferences = await _recommendationService.GetUserPreferencesAsync(userId);
                if (preferences == null)
                {
                    return NotFound("No preferences found for this user");
                }

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user preferences");
                return StatusCode(500, "An error occurred while retrieving user preferences");
            }
        }

        [HttpPut("preferences")]
        [Authorize]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] UserPreferenceUpdateDto preferencesDto)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                await _recommendationService.UpdateUserPreferencesAsync(userId, preferencesDto);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user preferences");
                return StatusCode(500, "An error occurred while updating user preferences");
            }
        }

        [HttpPost("track")]
        [Authorize]
        public async Task<IActionResult> TrackActivity([FromBody] UserActivity activity)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized("User ID not found in claims");
                }

                // S'assurer que l'ID utilisateur dans l'activité correspond à l'utilisateur authentifié
                if (activity.UserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid("You can only track your own activities");
                }

                await _recommendationService.TrackUserActivityAsync(activity);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking user activity");
                return StatusCode(500, "An error occurred while tracking user activity");
            }
        }

        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            try
            {
                var success = await _recommendationService.MarkRecommendationAsReadAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking recommendation {id} as read");
                return StatusCode(500, "An error occurred while marking recommendation as read");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteRecommendation(Guid id)
        {
            try
            {
                var success = await _recommendationService.DeleteRecommendationAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting recommendation {id}");
                return StatusCode(500, "An error occurred while deleting recommendation");
            }
        }
    }
}