using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace QuestHubClient.ViewModels
{
    public partial class CreateUserViewModel : BaseViewModel
    {
        public User User { get; set; } 
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public CreateUserViewModel()
        {
        }

        public CreateUserViewModel(INavigationService navigationService, IAuthService authService, User user)
        {
            _authService = authService;
            _navigationService = navigationService;
            User = user;
        }

        [RelayCommand]
        private void Cancel()
        {
            App.MainViewModel.IsRegistered = false;
            App.MainViewModel.LoginCheck = true;
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async Task CreateUser()
        {
            ErrorMessage = string.Empty;
            var context = new ValidationContext(User);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(User, context, results, true);

            if (!string.IsNullOrWhiteSpace(User.Password))
            {
                if (User.Password.Length < 8 || User.Password.Length > 32)
                {
                    results.Add(new ValidationResult("La contraseña debe tener entre 8 y 32 caracteres", new[] { "Password" }));
                    isValid = false;
                }
            }
            else
            {
                results.Add(new ValidationResult("La contraseña es obligatoria", new[] { "Password" }));
                isValid = false;
            }

            if (!isValid)
            {
                ErrorMessage = results.First().ErrorMessage;
                return;
            }

            try
            {
                var (userModel, message) = await _authService.RegisterAsync(User);
                if (userModel != null)
                {
                    new NotificationWindow(message, 3).Show();
                    User = new User();
                }
                else
                {
                    ErrorMessage = message ?? "Error al registrarse. Por favor, inténtalo de nuevo.";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inesperado: {ex.Message}";
            }
        }
    }
}