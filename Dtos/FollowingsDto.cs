using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QuestHubClient.Dtos
{
    public class FollowingRequestDto
    {
        [JsonPropertyName("userId")]
        public string Id { get; set; }
      
    }

    public class FollowingsResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
      
    }
}
