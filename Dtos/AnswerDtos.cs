using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class AnswerRequestDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("content")]

        public string Content { get; set; }
        [JsonPropertyName("author")]


        public string AuthorId { get; set; }
        [JsonPropertyName("qualification")]


        public float Qualification { get; set; }
        [JsonPropertyName("totalRatings")]


        public int TotalRatings { get; set; }
        [JsonPropertyName("post")]

        public string PostId { get; set; }
        [JsonPropertyName("createdAt")]

        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("parentAnswer")]

        public string ParentAnswerId { get; set; }
    }

    public class AnswersResponseDto
    {

        public string Message { get; set; }
        [JsonPropertyName("currentPage")]

        public int CurrentPage { get; set; }
        [JsonPropertyName("totalPages")]

        public int TotalPages { get; set; }
        [JsonPropertyName("totalAnswers")]

        public int TotalAnswers { get; set; }
        public List<AnswerDto> Answers { get; set; }

    }

    public class AnswerResponseDto
    {
         [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("qualification")]
        public float Qualification { get; set; }
        [JsonPropertyName("totalRatings")]
        public int TotalRatings { get; set; }
      

        [JsonPropertyName("post")]
        public string Post { get; set; }
        [JsonPropertyName("createdAt")]

        public DateTime CreatedAt { get; set; }
       
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }


    public class AnswerDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("content")]
        public string Content { get; set; }
        [JsonPropertyName("author")]
        public UserDto Author { get; set; }
        [JsonPropertyName("qualification")]
        public float Qualification { get; set; }
        [JsonPropertyName("totalRatings")]
        public int TotalRatings { get; set; }
        [JsonPropertyName("postId")]
        public string PostId { get; set; }
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("parentAnswerId")]
        public string ParentAnswerId { get; set; }

        [JsonPropertyName("post")]
        public PostDto Post { get; set; }
    }


}
