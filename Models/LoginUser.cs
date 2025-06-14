using System.ComponentModel.DataAnnotations;

namespace QuestHubClient.Models
{
    public class LoginUser
    {
        public string Name { get; set; } = "Axel";

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo no es válido.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; }

        public string ProfilePicture { get; set; } = "default_profile_picture.png";
        public string Role { get; set; } = "User";
    }

}
