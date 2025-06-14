using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {   
        public LoginUser User { get; set; } = new LoginUser();

        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        public LoginViewModel()
        {
            Title = "QuestHub - Login";
            _authService = new AuthService(new HttpClient());
        }
        public LoginViewModel(IAuthService authService)
        {
            Title = "QuestHub - Login";
            _authService = authService;
        }

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(User);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(User, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (token, userModel, message) = await _authService.LoginAsync(User);

                if (!string.IsNullOrEmpty(token) && userModel != null)
                {
                    Properties.Settings.Default.JwtToken = token;
                    Properties.Settings.Default.Save();

                    var mainWindow = new MainWindow();
                    mainWindow.DataContext = new MainWindowViewModel(userModel);
                    mainWindow.Show();

                    Application.Current.Windows.OfType<LoginView>().FirstOrDefault()?.Close();
                }
                else
                {
                    ErrorMessage = message ?? "Credenciales incorrectas. Por favor, inténtalo de nuevo.";
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

        [RelayCommand]
        private void Register()
        {
            var createUserView = new CreateUserView();
            createUserView.DataContext = new CreateUserViewModel(_authService);
            createUserView.Show();

            Application.Current.Windows.OfType<LoginView>().FirstOrDefault()?.Close();
        }
    }
}