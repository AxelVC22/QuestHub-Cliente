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
    public interface IRatingsService
    {
        //  Task<(List<Rating> ratings, string message)> GetRatingsAsync(int page, int limit);

        Task<(Answer answer, string message)> CreateRatingAsync(Rating rating);
    }

    public class RatingsService : IRatingsService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://10.48.138.135:3033/api/ratings";
        public RatingsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<(Answer answer, string message)> CreateRatingAsync(Rating rating)
        {
            try
            {
                var registerRequest = RatingToRatingRequestDto(rating);

                var jsonContent = JsonSerializer.Serialize(registerRequest);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = _httpClient.PostAsync(_baseUrl, httpContent).Result;

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var answerResponse = JsonSerializer.Deserialize<AnswerResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var updatedAnswer = ResponseToAnswer(answerResponse);

                    return (updatedAnswer, answerResponse.Message);
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

        private RatingRequestDto RatingToRatingRequestDto(Rating rating)
        {
            return new RatingRequestDto
            {
                Qualification = rating.Qualification,
                Answer = rating.Answer.Id,
                Author = rating.Author.Id
            };
        }

        private Rating ResponseToRating(RatingResponseDto ratingResponse)
        {
            return new Rating
            {
                Qualification = ratingResponse.Qualification,
                Id = ratingResponse.Id,
                Author = new User
                {
                    Id = ratingResponse.Author,
                },
                Answer = new Answer
                {
                    Id = ratingResponse.Answer,
                },
            };
        }

        private static Answer ResponseToAnswer(AnswerResponseDto answerResponseDto)
        {
            return new Answer
            {
                Author = new User { Id = answerResponseDto.Author },
                Content = answerResponseDto.Content,
                CreatedAt = answerResponseDto.CreatedAt,
                Id = answerResponseDto.Id,
                Post = new Post { Id = answerResponseDto.Post },
                Qualification = answerResponseDto.Qualification,
                TotalRatings = answerResponseDto.TotalRatings
            };
        }


    }




}
