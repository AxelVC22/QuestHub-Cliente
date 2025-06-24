using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IUserService
    {
        Task<(User user, string message)> GetUserByIdAsync(string userId);
        Task<(User user, string message)> UpdateUserAsync(string userId, User user);
        Task<(User user, string message)> DisableUserAsync(string userId, DateTime? banEndDate);
        Task<(string profilePicture, string message)> UpdateUserProfilePictureAsync(string userId, byte[] imageData, string fileName);
        Task<(User user, string message)> UpdateUserPasswordAsync(string userId, string newPassword);
        Task<(bool success, string message)> FollowUserAsync(string userIdToFollow, string currentUserId);
        Task<(bool success, string message)> UnfollowUserAsync(string userIdToUnfollow, string currentUserId);
        Task<(string[] followers, string message)> GetFollowersByUserIdAsync(string userId);
        Task<(byte[] imageData, string contentType, string message)> GetProfilePictureAsync(string userId);
    }

    public class UserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:3033/api/users";

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
                    var userDto = JsonSerializer.Deserialize<UserDetailDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var user = MapUserDetailDtoToUser(userDto);
                    return (user, "Usuario obtenido exitosamente");
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

                var jsonContent = JsonSerializer.Serialize(updateRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userDto = JsonSerializer.Deserialize<UserDetailDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var updatedUser = MapUserDetailDtoToUser(userDto);
                    return (updatedUser, "Usuario actualizado exitosamente");
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

        public async Task<(User user, string message)> DisableUserAsync(string userId, DateTime? banEndDate)
        {
            try
            {
                AddAuthHeader();
                var disableRequest = new DisableUserRequestDto
                {
                    BanEndDate = banEndDate?.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var jsonContent = JsonSerializer.Serialize(disableRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Note: The API route shows PUT method for disable, but the URL suggests it should be:
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var userDto = JsonSerializer.Deserialize<UserDetailDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var disabledUser = MapUserDetailDtoToUser(userDto);
                    return (disabledUser, "Usuario deshabilitado exitosamente");
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

        public async Task<(string profilePicture, string message)> UpdateUserProfilePictureAsync(string userId, byte[] imageData, string fileName)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return (null, "No se ha proporcionado ninguna imagen");

                if (imageData.Length > 20 * 1024 * 1024) // 20MB
                    return (null, "El archivo es demasiado grande (máximo 20MB)");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png"};
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

                var jsonContent = JsonSerializer.Serialize(passwordRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{BaseUrl}/{userId}/password", httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var passwordResponse = JsonSerializer.Deserialize<UpdatePasswordResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

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
                // Note: This endpoint doesn't seem to require authentication based on the router
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
                AddAuthHeader();
                var response = await _httpClient.GetAsync($"{BaseUrl}/{userId}/profile-picture");

                if (response.IsSuccessStatusCode)
                {
                    var imageBytes = await response.Content.ReadAsByteArrayAsync();
                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                    return (imageBytes, contentType, "Imagen obtenida exitosamente");
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (null, null, errorResponse?.Message ?? "Error desconocido al obtener la imagen");
                }
            }
            catch (HttpRequestException ex)
            {
                return (null, null, $"Error de conexión: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return (null, null, $"Error al procesar la respuesta: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (null, null, $"Error inesperado: {ex.Message}");
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
    }
}