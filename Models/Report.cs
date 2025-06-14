using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string Reason { get; set; }
        public int PostId { get; set; }
        public int AnswerId { get; set; }
        public int ReporterUserId { get; set; }
    }
}
