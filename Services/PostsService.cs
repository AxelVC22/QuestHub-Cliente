using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IPostsService
    {
        Task<(List<Post>, Page page, string message )> GetPostsByCategoryAsync(int page, int limit);
    }

    public class PostsService : IPostsService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/posts";

        public PostsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<Post>, Page page, string message)> GetPostsByCategoryAsync(int page, int limit)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}?page={page}&limit={limit}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<PostsResponseDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var posts = ResponseToPosts(result);

                var pageResponse = new Page
                {
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalPosts
                };


                return (posts, pageResponse, result.Message);
            }
            else
            {
                throw new Exception("Error al obtener los posts por categoría");
            }
        }

        private List<Post> ResponseToPosts(PostsResponseDto responseDto)
        {
            return responseDto.Posts.Select(postDto => new Post
            {
                Id = postDto.Id,
                Title = postDto.Title,
                Content = postDto.Content,
                CategoryId = postDto.CategoryId,
                UserId = postDto.UserId,
                CreatedAt = postDto.CreatedAt,
                AnswersCount = postDto.AnswersCount,
                AverageRating = postDto.AverageRating,
                Author = new User
                {
                    Id = postDto.UserId,
                },
                Categories = postDto.Categories.Select(category => new Category
                {
                    Id = category.Id,
                    Name = category.Name
                }).ToList(),
            }).ToList();
        }




    }
}
