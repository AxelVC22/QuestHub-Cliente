using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace QuestHubClient.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {

        //services

        private INavigationService _navigationService;

        public User User { get; set; }

        public bool _isRegistered;

        [ObservableProperty]
        private string _selectedSection = "Dashboard";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _notificationCount = 0;

        public MainWindowViewModel()
        {
            IsRegistered = false;
        }

        public MainWindowViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Title = "QuestHub - Menu Principal";
        }
        public MainWindowViewModel(User user)
        {
            User = user;
            Title = "QuestHub - Menu Principal";
        }

        [RelayCommand]
        private void Search()
        {

        }

        [RelayCommand]
        private void ShowNotifications()
        {

        }

        [RelayCommand]
        private void ShowProfile()
        {

        }

        [RelayCommand]
        private void Logout()
        {
            var result = MessageBox.Show("¿Estás seguro que deseas cerrar sesión?",
                                       "Cerrar Sesión",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                //var loginWindow = new Views.LoginView();
                //loginWindow.Show();

                Application.Current.Windows.OfType<Window>()
                    .Where(w => w.GetType().Name == "MainWindow")
                    .FirstOrDefault()?.Close();
            }
        }

        [RelayCommand]
        private void CreateNewPost()
        {

        }


        public bool IsRegistered
        {
            get => _isRegistered;
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    OnPropertyChanged(nameof(IsRegistered));
                }
            }
        }
    }
}