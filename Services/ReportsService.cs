using QuestHubClient.Dtos;
using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuestHubClient.Services
{
    public interface IReportsService
    {
        Task<(List<Report>, Page page, string message)> GetReportsAsync(int page, int limit, string status);
        Task<(Report report, string message)> CreateReportAsync(Report report);
    }
    public class ReportsService : IReportsService
    {
        private readonly HttpClient _httpClient;

        private const string _baseUrl = "http://localhost:3033/api/reports";

        public ReportsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(Report report, string message)> CreateReportAsync(Report report)
        {
            try
            {
                var registerRequest = ReportToPostRequestDto(report);

                var jsonContent = JsonSerializer.Serialize(registerRequest);

                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = _httpClient.PostAsync(_baseUrl, httpContent).Result;

                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var reportResponse = JsonSerializer.Deserialize<ReportResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var createdPost = ResponseToPost(reportResponse);

                    return (createdPost, reportResponse.Message);
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

        private ReportRequestDto ReportToPostRequestDto(Report report)
        {
            return new ReportRequestDto
            {
               Post = report.Post?.Id,
               Answer = report.Answer?.Id,
               Reporter = report.Reporter.Id,
               Reason = report.Reason
                
            };
        }

        private Report ResponseToPost(ReportResponseDto reportResponse)
        {

            return new Report
            {
                Post = new Post
                {
                    Id = reportResponse.Post,
                   
                },
                Answer = new Answer
                {

                    
                    Id = reportResponse.Answer,
                   
                },
                Reporter = new User
                {
                    Id = reportResponse.Reporter,
                   
                }
            };
          
        }

        public async Task<(List<Report>, Page page, string message)> GetReportsAsync(int page, int limit, string status)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}?page={page}&limit={limit}&status={status}");

                var responseContent = response.Content.ReadAsStringAsync().Result;


                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var result = JsonSerializer.Deserialize<ReportsResponseDto>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var reports = ResponseToReports(result);

                    var pageResponse = new Page
                    {
                        CurrentPage = result.CurrentPage,
                        TotalPages = result.TotalPages,
                        TotalItems = result.TotalReports
                    };


                    return (reports, pageResponse, result.Message);
                }
                else
                {
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponseDto>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return (null, null, errorResponse?.Message ?? "Error desconocido");
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

        private List<Report> ResponseToReports(ReportsResponseDto reportsResponseDto)
        {
            return reportsResponseDto
                .Reports
                .Select(report => new Report
                {
                    Id = report.Id,
                    Post = new Post
                    {
                        Id = report.Post.Id,
                        Title = report.Post.Title,
                        Content = report.Post.Content,
                        CreatedAt = report.Post.CreatedAt,
                        Author = new User
                        {
                            Id = report.Post.Author.Id,
                            Name = report.Post.Author.Name,
                        }
                    },
                    Answer = report.Answer != null ? new Answer
                    {
                        Id = report.Answer.Id,
                        Content = report.Answer.Content,
                        CreatedAt = report.Answer.CreatedAt,
                        Author = new User
                        {
                            Id = report.Answer.Author.Id,
                            Name = report.Answer.Author.Name,
                        }
                    } : null,
                    Reporter = new User
                    {
                        Id = report.Reporter.Id,
                        Name = report.Reporter.Name,
                    },
                    Reason = report.Reason,
                    Status = report.Status
                }).ToList();
        }
    }
}
