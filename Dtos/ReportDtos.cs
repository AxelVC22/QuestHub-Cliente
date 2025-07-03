using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{

    public class ReportsResponseDto
    {
        [JsonPropertyName("message")]

        public string Message { get; set; }
        [JsonPropertyName("currentPage")]

        public int CurrentPage { get; set; }
        [JsonPropertyName("totalPages")]

        public int TotalPages { get; set; }
        [JsonPropertyName("totalReports")]

        public int TotalReports { get; set; }
        public List<ReportDto> Reports { get; set; }
    }

    public class VerdictRequestDto
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("user")]
        public string User { get; set; }

        [JsonPropertyName("endBanDate")]

        public DateTime? EndBanDate { get; set; }
    }
    public class ReportRequestDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("reporter")]
        public string Reporter { get; set; }

        [JsonPropertyName("post")]

        public string Post { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }


    }

    public class FullReportResponseDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("reporter")]
        public UserDto Reporter { get; set; }

        [JsonPropertyName("post")]

        public PostDto Post { get; set; }

        [JsonPropertyName("answer")]
        public AnswerDto Answer { get; set; }


        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }


    }


    public class ReportResponseDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("reporter")]
        public string Reporter { get; set; }

        [JsonPropertyName("post")]

        public string Post { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }


        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }


    }

    public class ReportDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("reporter")]
        public UserDto Reporter { get; set; }

        [JsonPropertyName("post")]

        public PostDto? Post { get; set; }

        [JsonPropertyName("answer")]
        public AnswerDto? Answer { get; set; }


        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
