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
using System.Windows.Controls;

namespace QuestHubClient.Services
{
    public interface IReportsService
    {
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
    }
}
