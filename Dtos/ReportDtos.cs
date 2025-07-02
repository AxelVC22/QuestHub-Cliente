using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class ReportRequestDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName ("reason")]
        public string Reason { get; set; }

        [JsonPropertyName("reporter")]
        public string Reporter { get; set; }

        [JsonPropertyName("post")]

        public string Post { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }


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

        public PostDto Post { get; set; }

        [JsonPropertyName("answer")]
        public AnswerDto Answer { get; set; }


        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
