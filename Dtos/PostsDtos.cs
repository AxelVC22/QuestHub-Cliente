using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class PostRequestDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("answersCount")]
        public int AnswersCount { get; set; }

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; }
        [JsonPropertyName("multimedia")]
        public List<string> Multimedia { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; }
        [JsonPropertyName("isResolved")]
        public bool IsResolved { get; set; }
    }

    public class PostsResponseDto
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("answersCount")]
        public int AnswersCount { get; set; }

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; }
        [JsonPropertyName("multimedia")]
        public List<string> Multimedia { get; set; }
        [JsonPropertyName("author")]
        public UserDto Author { get; set; }
        [JsonPropertyName("categories")]
        public List<CategoryDto> Categories { get; set; }
        [JsonPropertyName("isResolved")]
        public bool IsResolved { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("currentPage")]

        public int CurrentPage { get; set; }
        [JsonPropertyName("totalPages")]

        public int TotalPages { get; set; }
        [JsonPropertyName("totalPosts")]

        public int TotalPosts { get; set; }
        public List<Post> Posts { get; set; }

    }

    public class PostResponseDto
    {

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("answersCount")]
        public int AnswersCount { get; set; }

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; }
        [JsonPropertyName("multimedia")]
        public List<string> Multimedia { get; set; }
        [JsonPropertyName("author")]
        public string Author { get; set; }
        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; }
        [JsonPropertyName("isResolved")]
        public bool IsResolved { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }
       

    }


    public class PostDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        [JsonPropertyName("categoryId")]
        public string CategoryId { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("answersCount")]
        public int AnswersCount { get; set; }

        [JsonPropertyName("averageRating")]
        public double AverageRating { get; set; }
        [JsonPropertyName("multimedia")]
        public List<string> Multimedia { get; set; }
        [JsonPropertyName("author")]
        public UserDto Author { get; set; }
        [JsonPropertyName("categories")]
        public List<CategoryDto> Categories { get; set; }
        [JsonPropertyName("isResolved")]
        public bool IsResolved { get; set; }

    }



}
