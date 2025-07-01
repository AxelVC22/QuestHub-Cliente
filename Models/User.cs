using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace QuestHubClient.Models
{
    public class User : ObservableObject
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre no puede tener más de 50 caracteres ni menos de 3")]
        public string Name { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        public string Email { get; set; }

        //[Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(32, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; }

        public UserRole Role { get; set; }

        public string ProfilePicture { get; set; }

        public string Status { get; set; }

        public DateTime? BanEndDate { get; set; }

        public int FollowersCount { get; set; }

        public User()
        {
            Role = UserRole.Guest;
        }

        private bool _isFollowed;

        public bool IsFollowed
        {
            get => _isFollowed;
            set
            {
                if (_isFollowed != value)
                {
                    _isFollowed = value;
                    OnPropertyChanged(nameof(IsFollowed));
                }
            }
        }

    }
}
