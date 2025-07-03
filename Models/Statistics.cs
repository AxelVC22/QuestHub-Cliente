using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Statistics
    {
        public int TotalPosts { get; set; }
        public int TotalAnswers { get; set; }

        // Ejemplo: { 1: 3, 2: 8, 3: 5, 4: 2, 5: 1 }
        public Dictionary<string, int> Ratings { get; set; }
    }
}
