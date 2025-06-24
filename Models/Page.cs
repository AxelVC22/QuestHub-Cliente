using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{

    public class Page
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }
}
