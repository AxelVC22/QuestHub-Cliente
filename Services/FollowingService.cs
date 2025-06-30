using QuestHubClient.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IFollowingService
    {
        Task<string> FollowUserAsync(string toFollow, string follower);
    }
    public class FollowingService : IFollowingService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/users";
        public FollowingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> FollowUserAsync(string toFollow,string follower)
        {
            try
            {
                var registerRequest = new FollowingRequestDto
                {
                    Id = follower
                };

                var jsonContent = JsonSerializer.Serialize(registerRequest);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/{toFollow}/follow", httpContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var followResponse = JsonSerializer.Deserialize<FollowingsResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return followResponse.Message;
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return errorResponse.Message;
                }

            }
            catch (HttpRequestException ex)
            {
                return $"Error de conexión: {ex.Message}";
            }
            catch (JsonException ex)
            {
                return $"Error al procesar la respuesta: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error inesperado: {ex.Message}";
            }


        }
    }
}
