using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class LoginViewModel : BaseViewModel
    {
        public LoginUser User { get; set; }
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        public LoginViewModel()
        {
            Title = "QuestHub - Login";
            _authService = new AuthService(new HttpClient());
        }

        public LoginViewModel(INavigationService navigationService, IAuthService authService, LoginUser user)
        {
            Title = "QuestHub - Login";
            _authService = authService;
            _navigationService = navigationService;
            User = user;
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
                    App.MainViewModel.LoginCheck = false;
                    App.MainViewModel.UpdateUserInfo(userModel);
                    new NotificationWindow(message, 3).Show();
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
        private void Back()
        {
            App.MainViewModel.LoginCheck = true;
            App.MainViewModel.IsRegistered = false;
            _navigationService.GoBack();
        }
    }
}