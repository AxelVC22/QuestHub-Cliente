using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
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
        private void CreateUser()
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

                //servicio




            }   
            catch (Exception ex)
            {
                ErrorMessage = $"Error al crear el usuario: {ex.Message}";
            }
        }
    }
}
