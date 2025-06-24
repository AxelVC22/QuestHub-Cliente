using System;
using System.Text.Json.Serialization;

namespace QuestHubClient.Dtos
{
    public class UpdateUserRequestDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }
    }

    public class DisableUserRequestDto
    {
        [JsonPropertyName("banEndDate")]
        public string BanEndDate { get; set; }
    }

    public class UpdatePasswordRequestDto
    {
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    public class FollowUserRequestDto
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
    }

    public class ProfilePictureResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("profilePicture")]
        public string ProfilePicture { get; set; }
    }

    public class UpdatePasswordResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("user")]
        public UserPasswordDto User { get; set; }
    }

    public class UserPasswordDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("profilePicture")]
        public string ProfilePicture { get; set; }
    }

    public class FollowResponseDto
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
    }

    public class FollowersResponseDto
    {
        [JsonPropertyName("followers")]
        public string[] Followers { get; set; }
    }

    public class UserDetailDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("profilePicture")]
        public string ProfilePicture { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("followers")]
        public int Followers { get; set; }

        [JsonPropertyName("banEndDate")]
        public DateTime? BanEndDate { get; set; }
    }
}