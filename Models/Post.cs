using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.Models
{
    public class Post : ObservableObject
    {
        public string Id { get; set; }

        private string _title;

        [Required(ErrorMessage = "El titulo de la publicación es obligatorio")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "El titulo no puede tener más de 255 caracteres ni menos de 5")]
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }


        private string _content;

        [Required(ErrorMessage = "El contenido de la publicación es obligatorio")]
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
        public string CategoryId { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int TotalAnswers { get; set; }
        public double AverageRating { get; set; }

        // Navigation properties para UI
        public User Author { get; set; }


        private List<Category> _categories;

        [Required(ErrorMessage = "Las categorias son obligatorias")]

        

        public List<Category> Categories
        {
            get => _categories;
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(Categories));
                }
            }

        }
    }
}
