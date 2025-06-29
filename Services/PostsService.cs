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
        Task<(List<Post>, Page page, string message)> GetPostsAsync(int page, int limit);

        Task<(Post post, string message)> CreatePostAsync(Post post);
    }

    public class PostsService : IPostsService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/posts";

        public PostsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<Post>, Page page, string message)> GetPostsAsync(int page, int limit)
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
               
                CreatedAt = postDto.CreatedAt,
                TotalAnswers = postDto.TotalAnswers,
                AverageRating = postDto.AverageRating,
                Author = new User
                {
                    Id = postDto.Author?.Id,
                   Name = postDto.Author?.Name
                },
                Categories = postDto.Categories.Select(category => new Category
                {
                    Id = category?.Id,
                    Name = category?.Name
                }).ToList(),
            }).ToList();
        }

        private Post ResponseToPost(PostResponseDto postResponse)
        {
            return new Post
            {
                Id = postResponse.Id,
                Title = postResponse.Title,
                Content = postResponse.Content,
              
                CreatedAt = postResponse.CreatedAt,
                TotalAnswers = postResponse.TotalAnswers,
                AverageRating = postResponse.AverageRating,
                Categories = postResponse.Categories.Select(c => new Category
                {
                    Id = c.ToString(),
                }).ToList(),
            };
        }

        public async Task<(Post post, string message)> CreatePostAsync(Post post)
        {
            try
            {
                var registerRequest = PostToPostRequestDto(post);

                var jsonContent = JsonSerializer.Serialize(registerRequest);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = _httpClient.PostAsync(_baseUrl, httpContent).Result;

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var postResponse = JsonSerializer.Deserialize<PostResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var createdPost = ResponseToPost(postResponse);

                    return (createdPost, postResponse.Message);
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


        private PostRequestDto PostToPostRequestDto(Post post)
        {
            return new PostRequestDto
            {
                
                Title = post.Title,
                Content = post.Content,
               
                Categories = post.Categories.Select(c =>

                      c.Id
                ).ToList(),
                Author
                    = post.Author.Id,

            };
        }
    }
}
