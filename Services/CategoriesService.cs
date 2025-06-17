using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface ICategoriesService
    {
        Task<(List<Category>, Page page, string message)> GetCategoriesAsync(int page, int limit);
    }
    public class CategoriesService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/categories";

        public CategoriesService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<Category>, Page page, string message)> GetCategoriesAsync(int page, int limit)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}?page={page}&limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = System.Text.Json.JsonSerializer.Deserialize<CategoriesResponseDto>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var categories = ResponseToCategories(result);

                var pageResponse = new Page
                {
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalCategories
                };

                return (categories, pageResponse, result.Message);
            }
            else
            {
                throw new Exception("Error al obtener las categorías");
            }
        }

        private List<Category> ResponseToCategories(CategoriesResponseDto responseDto)
        {

            return responseDto.Categories.Select(c => new Category
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                Status = c.Status
            }).ToList();
        }
    }
}
