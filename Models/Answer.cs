using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Answer : ObservableObject
    {
        public string Id { get; set; }

        private string _content;

        [Required(ErrorMessage = "El contenido de la respuesta es obligatorio")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "El nombre no puede tener más de 255 caracteres ni menos de 3")]
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }
        public Post Post { get; set; }
        [Required(ErrorMessage = "Solo usuarios registrados pueden hacer comentarios")]

        public User Author { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAccepted { get; set; }

        private float _qualification;

        public float Qualification
        {
            get => _qualification;
            set
            {
                if (_qualification != value)
                {
                    _qualification = value;
                    OnPropertyChanged(nameof(Qualification));
                }
            }
        }

        public string ParentAnswerId { get; set; }

        public Answer ParentAnswer { get; set; }

        private int _totalRatings;

        public int TotalRatings
        {
            get => _totalRatings;
            set
            {
                if (_totalRatings != value)
                {
                    _totalRatings = value;
                    OnPropertyChanged(nameof(TotalRatings));
                }
            }
        }


    }
}
