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
    public interface IAnswersService
    {
        Task<(List<Answer>, Page page, string message)> GetAnswersByPostAsync(string postId, int page, int limit, string userId);

        Task<(List<Answer>, Page page, string message)> GetAnswersByAnswerAsync(string answerId, int page, int limit, string userId);

        Task<(Answer answer, string message)> CreateAnswerAsync(Answer answer);
    }
    public class AnswersService : IAnswersService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/answers";

        public AnswersService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(List<Answer>, Page page, string message)> GetAnswersByPostAsync(string postId, int page, int limit, string userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/post/{postId}?page={page}&limit={limit}&user={userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AnswersResponseDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var answers = ResponseToAnswers(result);

                var pageResponse = new Page
                {
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalAnswers
                };

                return (answers, pageResponse, result.Message);
            }
            else
            {
                throw new Exception("Error al obtener las respuestas del post");
            }
        }

        public async Task<(List<Answer>, Page page, string message)> GetAnswersByAnswerAsync(string answerId, int page, int limit, string userId)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/answer/{answerId}?page={page}&limit={limit}&user={userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AnswersResponseDto>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var answers = ResponseToAnswers(result);

                var pageResponse = new Page
                {
                    CurrentPage = result.CurrentPage,
                    TotalPages = result.TotalPages,
                    TotalItems = result.TotalAnswers
                };

                return (answers, pageResponse, result.Message);
            }
            else
            {
                throw new Exception("Error al obtener las respuestas de esta respuesta");
            }
        }

        private List<Answer> ResponseToAnswers(AnswersResponseDto response)
        {
            return response.Answers.Select(a => new Answer
            {
                Author = new User
                {
                    Id = a.Author.Id,
                    Name = a.Author.Name,
                    IsFollowed = a.Author?.IsFollowed??false,
                },
                Id = a.Id,
                Content = a.Content,
                Qualification = a.Qualification,
                TotalRatings = a.TotalRatings,
                Post = new Post { Id = a.PostId },
                CreatedAt = DateTime.SpecifyKind(a.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
                ParentAnswerId = a.ParentAnswerId
            }).ToList();
        }

        public async Task<(Answer answer, string message)> CreateAnswerAsync(Answer answer)
        {
            try
            {
                var registerRequest = AnswerToAnswerRequestDto(answer);
                var jsonContent = JsonSerializer.Serialize(registerRequest);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                var response = _httpClient.PostAsync(_baseUrl, httpContent).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var answerReponse = JsonSerializer.Deserialize<AnswerResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });

                    var createdAnswer = ResponseToAnswer(answerReponse);

                    return (createdAnswer, answerReponse.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (null, errorResponse?.Message ?? "Error al crear la respuesta");
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

        private AnswerRequestDto AnswerToAnswerRequestDto(Answer answer)
        {
            return new AnswerRequestDto
            {
                Content = answer.Content,
                AuthorId = answer.Author.Id,
                PostId = answer.Post?.Id,
                ParentAnswerId = answer.ParentAnswer?.Id
            };
        }

        private static Answer ResponseToAnswer(AnswerResponseDto answerResponseDto)
        {
            return new Answer
            {
                Author = new User { Id = answerResponseDto.Author },
                Content = answerResponseDto.Content,
                CreatedAt = DateTime.SpecifyKind(answerResponseDto.CreatedAt, DateTimeKind.Utc).ToLocalTime(),
                Id = answerResponseDto.Id,
                Post = new Post { Id = answerResponseDto.Post },
                Qualification = answerResponseDto.Qualification,
                TotalRatings = answerResponseDto.TotalRatings
            };
        }
    }

}
