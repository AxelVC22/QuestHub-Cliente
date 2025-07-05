using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuestHubClient.Dtos;
using System.Threading.Tasks;
using QuestHubClient.Models;

namespace QuestHubClient.Services
{
    public interface IAuthService
    {
        Task<(string token, User userModel, string message)> LoginAsync(LoginUser user);
        Task<(User userModel, string message)> RegisterAsync(User user);
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://10.48.138.135:3033/api/auth";

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

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
                    var loginResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, _jsonOptions);

                    string token = "";
                    if (response.Headers.Contains("x-token"))
                    {
                        token = response.Headers.GetValues("x-token").FirstOrDefault() ?? "";
                    }

                    var userModel = ResponseToUser(loginResponse);

                    return (token, userModel, loginResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, _jsonOptions);
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

        public async Task<(User userModel, string message)> RegisterAsync(User user)
        {
            try
            {
                var registerRequest = UserToLoginRequestDto(user);
                var jsonContent = JsonSerializer.Serialize(registerRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var registerResponse = JsonSerializer.Deserialize<AuthResponseDto>(responseContent, _jsonOptions);
                    var userModel = ResponseToUser(registerResponse);

                    return (userModel, registerResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, _jsonOptions);
                    return (null, errorResponse?.Message ?? "Error desconocido");
                }
            }
            catch (HttpRequestException ex)
            {
                return (null, $"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (null, $"Error al procesar la respuesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, $"Error inesperado: {ex.Message}");
            }
        }

        private LoginRequestDto LoginUserToLoginRequestDto(LoginUser user)
        {
            return new LoginRequestDto
            {
                Email = user.Email,
                Password = user.Password
            };
        }

        private RegisterRequestDto UserToLoginRequestDto(User user)
        {
            return new RegisterRequestDto
            {
                Name = user.Name,
                Email = user.Email,
                Password = user.Password
            };
        }

        private User ResponseToUser(AuthResponseDto response)
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