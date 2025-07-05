using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IUserService
    {
        Task<(User user, string message)> GetUserByIdAsync(string userId);
        Task<(List<User> users, string message)> GetUsersAsync();
        Task<(User user, string message)> UpdateUserAsync(string userId, User user);
        Task<(User user, string message)> DisableUserAsync(string userId, DateTime? banEndDate);
        Task<(string profilePicture, string message)> UpdateUserProfilePictureAsync(string userId, byte[] imageData, string fileName);
        Task<(User user, string message)> UpdateUserPasswordAsync(string userId, string newPassword);
        Task<(bool success, string message)> FollowUserAsync(string userIdToFollow, string currentUserId);
        Task<(bool success, string message)> UnfollowUserAsync(string userIdToUnfollow, string currentUserId);
        Task<(string[] followers, string message)> GetFollowersByUserIdAsync(string userId);
        Task<(byte[] imageData, string contentType, string message)> GetProfilePictureAsync(string userId);
        Task<(User user, string message)> RegisterUserAsync(User user);

        Task<(Statistics statistics, string message)> GetStatisticsAsync(string userId);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://10.48.138.135:3033/api/users";

        public UserService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private void AddAuthHeader()
        {
            var token = Properties.Settings.Default.JwtToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Remove("x-token");
                _httpClient.DefaultRequestHeaders.Add("x-token", token);
            }
        }

        public async Task<(User user, string message)> GetUserByIdAsync(string userId)
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{BaseUrl}/{userId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userDto = JsonSerializer.Deserialize<UserDetailDto>(responseContent, GetJsonSerializerOptions());

                    var user = MapUserDetailDtoToUser(userDto);
                    return (user, "Usuario obtenido exitosamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        public async Task<(List<User> users, string message)> GetUsersAsync()
        {
            try
            {
                AddAuthHeader();
                var response = await _httpClient.GetAsync(BaseUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var usersDto = JsonSerializer.Deserialize<List<UserDetailDto>>(responseContent, GetJsonSerializerOptions());

                    var users = usersDto?.Select(MapUserDetailDtoToUser).ToList() ?? new List<User>();
                    return (users, "Usuarios obtenidos exitosamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        public async Task<(User user, string message)> UpdateUserAsync(string userId, User user)
        {
            try
            {
                AddAuthHeader();
                var updateRequest = new UpdateUserRequestDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };

                var jsonContent = JsonSerializer.Serialize(updateRequest, GetJsonSerializerOptionsForSerialization());
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userDto = JsonSerializer.Deserialize<UserUpdateResponseDto>(responseContent, GetJsonSerializerOptions());

                    var updatedUser = MapUserUpdateDtoToUser(userDto);
                    return (updatedUser, "Usuario actualizado exitosamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        public async Task<(User user, string message)> DisableUserAsync(string userId, DateTime? banEndDate)
        {
            try
            {
                AddAuthHeader();
                var disableRequest = new DisableUserRequestDto
                {
                    BanEndDate = banEndDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var jsonContent = JsonSerializer.Serialize(disableRequest, GetJsonSerializerOptionsForSerialization());
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userDto = JsonSerializer.Deserialize<UserDetailDto>(responseContent, GetJsonSerializerOptions());

                    var disabledUser = MapUserDetailDtoToUser(userDto);
                    return (disabledUser, "Usuario deshabilitado exitosamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        public async Task<(string profilePicture, string message)> UpdateUserProfilePictureAsync(string userId, byte[] imageData, string fileName)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return (null, "No se ha proporcionado ninguna imagen");

                if (imageData.Length > 20 * 1024 * 1024) // 20MB
                    return (null, "El archivo es demasiado grande (máximo 20MB)");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(fileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                    return (null, "Formato no permitido. Solo imágenes JPG, PNG o JPEG");

                AddAuthHeader();

                using var form = new MultipartFormDataContent();
                var imageContent = new ByteArrayContent(imageData);

                var contentType = GetContentType(fileName);
                imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                form.Add(imageContent, "profilePicture", fileName);

                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}/profile-picture", form);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var updateResponse = JsonSerializer.Deserialize<ProfilePictureResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return (updateResponse.ProfilePicture, updateResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
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

        public async Task<(User user, string message)> UpdateUserPasswordAsync(string userId, string newPassword)
        {
            try
            {
                AddAuthHeader();
                var passwordRequest = new UpdatePasswordRequestDto
                {
                    Password = newPassword
                };

                var jsonContent = JsonSerializer.Serialize(passwordRequest, GetJsonSerializerOptionsForSerialization());
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}/password", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var passwordResponse = JsonSerializer.Deserialize<UpdatePasswordResponseDto>(responseContent, GetJsonSerializerOptions());

                    var user = new User
                    {
                        Id = passwordResponse.User.Id,
                        Name = passwordResponse.User.Name,
                        Email = passwordResponse.User.Email,
                        Role = passwordResponse.User.Role,
                        ProfilePicture = passwordResponse.User.ProfilePicture
                    };

                    return (user, passwordResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        public async Task<(bool success, string message)> FollowUserAsync(string userIdToFollow, string currentUserId)
        {
            try
            {
                AddAuthHeader();
                var followRequest = new FollowUserRequestDto
                {
                    UserId = currentUserId
                };

                var jsonContent = JsonSerializer.Serialize(followRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userIdToFollow}/follow", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var followResponse = JsonSerializer.Deserialize<FollowResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (true, followResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (false, errorResponse?.Message ?? "Error desconocido");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (false, $"Error al procesar la respuesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(bool success, string message)> UnfollowUserAsync(string userIdToUnfollow, string currentUserId)
        {
            try
            {
                AddAuthHeader();
                var unfollowRequest = new FollowUserRequestDto
                {
                    UserId = currentUserId
                };

                var jsonContent = JsonSerializer.Serialize(unfollowRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userIdToUnfollow}/unfollow", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var unfollowResponse = JsonSerializer.Deserialize<FollowResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (true, unfollowResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (false, errorResponse?.Message ?? "Error desconocido");
                }
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (false, $"Error al procesar la respuesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(string[] followers, string message)> GetFollowersByUserIdAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{userId}/followers");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var followersResponse = JsonSerializer.Deserialize<FollowersResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (followersResponse.Followers, "Seguidores obtenidos exitosamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

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

        public async Task<(byte[] imageData, string contentType, string message)> GetProfilePictureAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{userId}/profile-picture");

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    return (imageBytes, contentType, "Imagen obtenida exitosamente");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return (null, null, "Imagen no encontrada");
                }
                else
                {
                    var textResponse = await response.Content.ReadAsStringAsync();
                    return (null, null, $"Error: {textResponse}");
                }
            }
            catch (HttpRequestException ex)
            {
                return (null, null, $"Error de conexión: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, null, $"Error inesperado: {ex.Message}");
            }
        }

        public async Task<(User user, string message)> RegisterUserAsync(User user)
        {
            try
            {
                AddAuthHeader();
                var registerRequest = new RegisterUserRequestDto
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                    Role = user.Role
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter() },
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = JsonSerializer.Serialize(registerRequest, jsonOptions);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(BaseUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var registerResponse = JsonSerializer.Deserialize<RegisterUserResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter() }
                    });
                    var responseUser = new User
                    {
                        Id = registerResponse.Id,
                        Name = registerResponse.Name,
                        Email = registerResponse.Email,
                        Role = registerResponse.Role,
                        Status = registerResponse.Status
                    };
                    return (responseUser, registerResponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
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

        private User MapUserDetailDtoToUser(UserDetailDto dto)
        {
            return new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                ProfilePicture = dto.ProfilePicture,
                Role = dto.Role,
                Status = dto.Status,
                BanEndDate = dto.BanEndDate,
                FollowersCount = dto.Followers
            };
        }

        private User MapUserUpdateDtoToUser(UserUpdateResponseDto dto)
        {
            return new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                ProfilePicture = dto.ProfilePicture,
                Role = dto.Role,
                Status = dto.Status,
                BanEndDate = dto.BanEndDate
            };
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
        }

        private static JsonSerializerOptions GetJsonSerializerOptions()
        {
            return new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNameCaseInsensitive = true
            };
        }

        private static JsonSerializerOptions GetJsonSerializerOptionsForSerialization()
        {
            return new JsonSerializerOptions
            {
                Converters = { new JsonStringEnumConverter() },
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<(Statistics statistics, string message)> GetStatisticsAsync(string userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/statistics/{userId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var statisticsDto = JsonSerializer.Deserialize<StatisticsDto>(responseContent, GetJsonSerializerOptions());

                    var statics = StatisticsDtotoStatistics(statisticsDto);
                    return (statics, "Estadísticas cargadas correctamente");
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, GetJsonSerializerOptions());
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

        private Statistics StatisticsDtotoStatistics(StatisticsDto statisticsDto)
        {
            return new Statistics
            {
                TotalPosts = statisticsDto.TotalPosts,
                TotalAnswers = statisticsDto.TotalAnswers,
                Ratings = statisticsDto.RatingDistribution.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };
        }
    }
}