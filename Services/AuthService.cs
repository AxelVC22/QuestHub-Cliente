using System.Net.Http;
using System.Text;
using System.Text.Json;
using QuestHubClient.Dtos;
using System.Threading.Tasks;
using QuestHubClient.Models;

namespace QuestHubClient.Services
{
    public interface IAuthService
    {
        Task<(string token, User userModel, string message)> LoginAsync(LoginUser user);

        Task<(string token, User userModel, string message)> RegisterAsync(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:3033/api/auth";

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<(string token, User userModel, string message)> LoginAsync(LoginUser user)
        {
            try
            {
                var loginRequest = LoginUserToLoginRequestDto(user);
                var jsonContent = JsonSerializer.Serialize(loginRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(BaseUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    string token = "";
                    if (response.Headers.Contains("x-token"))
                    {
                        token = response.Headers.GetValues("x-token").FirstOrDefault() ?? "";
                    }

                    var userModel = LoginResponseToUser(loginResponse);

                    return (token, userModel, loginResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return ("", null, errorResponse?.Message ?? "Error desconocido");
                }
            }
            catch (HttpRequestException ex)
            {
                return ("", null, $"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return ("", null, $"Error al procesar la respuesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ("", null, $"Error inesperado: {ex.Message}");
            }

        }

        public Task<(string token, User userModel, string message)> RegisterAsync(User user)
        {

        }

        private LoginRequestDto LoginUserToLoginRequestDto(LoginUser user)
        {
            return new LoginRequestDto
            {
                Email = user.Email,
                Password = user.Password
            };
        }

        private User LoginResponseToUser(LoginResponseDto response)
        {
            return new User
            {
                Name = response.User.Name,
                Email = response.User.Email,
                ProfilePicture = response.User.ProfilePicture,
                Role = response.User.Role,
                Status = response.User.Status,
                BanEndDate = response.User.BanEndDate,
                FollowersCount = response.User.Followers,
                Id = response.User.Id
            };
        }
    }
}
