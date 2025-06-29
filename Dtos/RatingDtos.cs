using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class RatingRequestDto
    {
        [JsonPropertyName("qualification")]
        public int Qualification { get; set; }

        [JsonPropertyName("answer")]
        public string Answer { get; set; }

        [JsonPropertyName("author")]
        public string Author { get; set; }
    }

    public class RatingResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("qualification")]
        public int Qualification { get; set; }
        [JsonPropertyName("answer")]
        public string Answer { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }

    }

    public class RatingDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("author")]
        public UserDto Author { get; set; }
        [JsonPropertyName("qualification")]
        public int Qualification { get; set; }

        [JsonPropertyName("answer")]
        public AnswerDto Answer { get; set; }
    }
}
