using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Answer
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string PostId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAccepted { get; set; }

        public int AuthorUserId { get; set; }
    }
}
