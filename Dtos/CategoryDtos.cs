using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class CategoryRequestDto
    {
     
    }

    public class CategoriesResponseDto
    {

        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("currentPage")]

        public int CurrentPage { get; set; }
        [JsonPropertyName("totalPages")]

        public int TotalPages { get; set; }
        [JsonPropertyName("totalCategories")]

        public int TotalCategories { get; set; }
        public List<CategoryDto> Categories { get; set; }

    }


    public class CategoryDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]

        public string Name { get; set; }
        [JsonPropertyName("description")]

        public string Description { get; set; }
        [JsonPropertyName("status")]

        public string Status { get; set; }
    }
}
