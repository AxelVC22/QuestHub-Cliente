using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Post
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string CategoryId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalAnswers { get; set; }
        public double AverageRating { get; set; }

        // Navigation properties para UI
        public User Author { get; set; }
        public List<Category> Categories { get; set; }
    }
}
