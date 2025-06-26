using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface ICategoriesService
    {
        Task<(List<Category> categories, string message)> GetCategoriesAsync();
        Task<(Category category, string message)> GetCategoryByIdAsync(string categoryId);
        Task<(Category category, string message)> CreateCategoryAsync(CreateCategoryDto categoryDto);
        Task<(Category category, string message)> UpdateCategoryAsync(string categoryId, UpdateCategoryDto categoryDto);
        Task<(Category category, string message)> DeleteCategoryAsync(string categoryId);
    }

    public class CategoriesService : ICategoriesService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:3033/api/categories";

        public CategoriesService(HttpClient httpClient)
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

        public async Task<(List<Category> categories, string message)> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var categoriesDto = JsonSerializer.Deserialize<List<CategoryDto>>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var categories = categoriesDto.Select(dto => new Category
                    {
                        Id = dto.Id,
                        Name = dto.Name,
                        Description = dto.Description,
                        Status = dto.Status
                    }).ToList();

                    return (categories, "Categorías obtenidas exitosamente");
                }
                else
                {
                    var errorResponse = await TryDeserializeError(responseContent);
                    return (null, errorResponse?.Message ?? "Error desconocido al obtener las categorías");
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

        public async Task<(Category category, string message)> GetCategoryByIdAsync(string categoryId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/{categoryId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var categoryDto = JsonSerializer.Deserialize<CategoryDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var category = new Category
                    {
                        Id = categoryDto.Id,
                        Name = categoryDto.Name,
                        Description = categoryDto.Description,
                        Status = categoryDto.Status
                    };

                    return (category, "Categoría obtenida exitosamente");
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

        public async Task<(Category category, string message)> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            try
            {
                AddAuthHeader();

                var json = JsonSerializer.Serialize(categoryDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(BaseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var createdCategoryDto = JsonSerializer.Deserialize<CategoryDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var category = new Category
                    {
                        Id = createdCategoryDto.Id,
                        Name = createdCategoryDto.Name,
                        Description = createdCategoryDto.Description,
                        Status = createdCategoryDto.Status
                    };

                    return (category, "Categoría creada exitosamente");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = await TryDeserializeError(responseContent);
                    return (null, errorResponse?.Message ?? "Datos inválidos para crear la categoría");
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

        public async Task<(Category category, string message)> UpdateCategoryAsync(string categoryId, UpdateCategoryDto categoryDto)
        {
            try
            {
                AddAuthHeader();

                var json = JsonSerializer.Serialize(categoryDto, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{BaseUrl}/{categoryId}", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var updatedCategoryDto = JsonSerializer.Deserialize<CategoryDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var category = new Category
                    {
                        Id = updatedCategoryDto.Id,
                        Name = updatedCategoryDto.Name,
                        Description = updatedCategoryDto.Description,
                        Status = updatedCategoryDto.Status
                    };

                    return (category, "Categoría actualizada exitosamente");
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

        public async Task<(Category category, string message)> DeleteCategoryAsync(string categoryId)
        {
            try
            {
                AddAuthHeader();

                var response = await _httpClient.DeleteAsync($"{BaseUrl}/{categoryId}");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var deletedCategoryDto = JsonSerializer.Deserialize<CategoryDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var category = new Category
                    {
                        Id = deletedCategoryDto.Id,
                        Name = deletedCategoryDto.Name,
                        Description = deletedCategoryDto.Description,
                        Status = deletedCategoryDto.Status
                    };

                    return (category, "Categoría eliminada exitosamente");
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

        private async Task<ErrorResponseDto?> TryDeserializeError(string errorContent)
        {
            try
            {
                return JsonSerializer.Deserialize<ErrorResponseDto>(errorContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return null;
            }
        }
    }
}