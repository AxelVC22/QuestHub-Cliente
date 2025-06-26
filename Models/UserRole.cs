using System;
using System.Text.Json.Serialization;

namespace QuestHubClient.Models
{
    public enum UserRole
    {
        Admin,
        Moderator,
        User,
        Guest
    }
}