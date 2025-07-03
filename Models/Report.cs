using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Report : ObservableObject
    {
        public string Id { get; set; }
        [Required(ErrorMessage = "Los motivos son obligatorios")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Los motivos no pueden tener más de 50 caracteres ni menos de s3")]

        public string Reason { get; set; }
        public Post Post { get; set; }
        public Answer Answer { get; set; }
        public User Reporter { get; set; }

        public string Status;

        public DateTime? EndBanDate { get; set; }
    }
}
