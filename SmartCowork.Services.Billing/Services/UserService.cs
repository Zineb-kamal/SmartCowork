using SmartCowork.Services.Billing.Models.DTOs;
using System.Net.Http.Headers;

namespace SmartCowork.Services.Billing.Services
{
    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserService> _logger;
        private readonly string _apiGatewayUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(HttpClient httpClient, IConfiguration configuration, ILogger<UserService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiGatewayUrl = configuration["ApiGateway:Url"] ?? "http://localhost:5072";
            _httpContextAccessor = httpContextAccessor;
            _httpContextAccessor= httpContextAccessor;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid userId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?
                .Request.Headers["Authorization"].ToString()
                .Replace("Bearer ", "");

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("No JWT token found in request");
                    return null;
                }

                // Attach token to HttpClient
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync($"{_apiGatewayUrl}/api/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
                    return userDto;
                }

                _logger.LogWarning($"Échec de la récupération de l'utilisateur {userId}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de la récupération de l'utilisateur {userId}");
                return null;
            }
        }

    }
}
