using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public User Author { get; set; }
        public int Qualification { get; set; } // 1-5
        public Answer Answer { get; set; }
    }
}
