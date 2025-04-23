// Services/SpaceService.cs
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace SmartCowork.Services.Booking.Services
{
    public class SpaceService : ISpaceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SpaceService> _logger;
        private readonly string _apiGatewayUrl;

        public SpaceService(HttpClient httpClient, ILogger<SpaceService> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiGatewayUrl = configuration["ApiGateway:Url"] ?? "http://localhost:5072";
        }

        public async Task<SpaceDetailsDto> GetSpaceDetailsAsync(Guid spaceId)
        {
            try
            {
                _logger.LogInformation($"Récupération des détails de l'espace {spaceId}");
                var response = await _httpClient.GetAsync($"{_apiGatewayUrl}/api/space/{spaceId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Détails de l'espace reçus: {content}");
                    return JsonConvert.DeserializeObject<SpaceDetailsDto>(content);
                }

                _logger.LogWarning($"Erreur lors de la récupération de l'espace {spaceId}: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception lors de la récupération de l'espace {spaceId}");
                return null;
            }
        }
    }
}