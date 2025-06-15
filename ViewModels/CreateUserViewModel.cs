using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace QuestHubClient.ViewModels
{
    public partial class CreateUserViewModel : BaseViewModel
    {
        public User User { get; set; } = new User();

        //services

        private readonly IAuthService _authService;

        private readonly INavigationService _navigationService;

        //commands

        //[ObservableProperty]
        //private ObservableCollection<string> roles = new()
        //{
        //    "Administrador",
        //    "Moderador",
        //    "Usuario"
        //};

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public CreateUserViewModel()
        {
        }

        public CreateUserViewModel(INavigationService navigationService, IAuthService authService)
        {
            Title = "QuestHub - Crear Usuario";
            _authService = authService;
            _navigationService = navigationService;
        }

       

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async Task CreateUser()
        {
            ErrorMessage = string.Empty;
            var context = new ValidationContext(User);
            var results = new List<ValidationResult>();
            if (!Validator.TryValidateObject(User, context, results, true))
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

                    //var loginView = new LoginView();
                    //loginView.DataContext = new LoginViewModel(_authService);
                    //loginView.Show();
                    //Application.Current.Windows.OfType<CreateUserView>().FirstOrDefault()?.Close();
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
