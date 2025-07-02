using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.ViewModels;
using QuestHubClient.Views;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        private INavigationService _navigationService;

        public ObservableCollection<MenuOption> MenuOptions { get; } = new ObservableCollection<MenuOption>();

        public User User { get; set; }
        public bool _isRegistered;
        public bool _loginCheck = true;

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

        [RelayCommand]
        private void Search()
        {
        }

        [RelayCommand]
        private void SignIn()
        {
            App.MainViewModel.LoginCheck = false;
            _navigationService.NavigateTo<CreateUserViewModel>();
        }

        [RelayCommand]
        private void Login()
        {
            App.MainViewModel.LoginCheck = false;
            _navigationService.NavigateTo<LoginViewModel>();
        }

        [RelayCommand]
        private void GoHome()
        {
            _navigationService.NavigateTo<HomeViewModel>();
        }

        [RelayCommand]
        private void ShowNotifications()
        {
        }

        [RelayCommand]
        private void ShowProfile()
        {
            _navigationService.NavigateTo<ProfileViewModel>();
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
                User = new User();
                IsRegistered = false;
                LoginCheck = true;
                MenuOptions.Clear(); // Limpiar opciones del menú
                Properties.Settings.Default.JwtToken = string.Empty;
                Properties.Settings.Default.Save();
                _navigationService.NavigateTo<HomeViewModel>();
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

        public bool LoginCheck
        {
            get => _loginCheck;
            set
            {
                if (_loginCheck != value)
                {
                    _loginCheck = value;
                    OnPropertyChanged(nameof(LoginCheck));
                }
            }
        }

        public void UpdateUserInfo(User loggedInUser)
        {
            User = loggedInUser;
            IsRegistered = true;
            LoginCheck = true;
            LoadMenuOptionsByRole();
            NavigateToStartPageByRole(); 
        }

        private void LoadMenuOptionsByRole()
        {
            MenuOptions.Clear();
            if (User?.Role == null) return;

            switch (User.Role)
            {
                case UserRole.Admin:
                    AddMenuOption("📝 Categorias", () => _navigationService.NavigateTo<CategoriesViewModel>());
                    AddMenuOption("📊 Dashboard", () => _navigationService.NavigateTo<HomeViewModel>());
                    AddMenuOption("👥 Usuarios", () => _navigationService.NavigateTo<UsersViewModel>());
                    AddMenuOption("🚩 Reportes", () => _navigationService.NavigateTo<ReportsViewModel>());
                    break;

                case UserRole.Moderator:
                    //AddMenuOption("📊 Dashboard", () => _navigationService.NavigateTo<DashboardViewModel>());
                    //AddMenuOption("📝 Posts", () => _navigationService.NavigateTo<PostsViewModel>());
                    //AddMenuOption("👥 Usuarios", () => _navigationService.NavigateTo<UsersViewModel>());
                    //AddMenuOption("🚩 Reportes", () => _navigationService.NavigateTo<ReportsViewModel>());
                    break;

                case UserRole.User:
                    AddMenuOption("🏠 Inicio", () => _navigationService.NavigateTo<HomeViewModel>());
                    break;
            }
        }

        private void NavigateToStartPageByRole()
        {
            if (User?.Role == null) return;

            switch (User.Role)
            {
                case UserRole.Admin:
                    _navigationService.NavigateTo<CategoriesViewModel>();
                    SelectedSection = "Dashboard";
                    break;

                case UserRole.Moderator:
                    _navigationService.NavigateTo<HomeViewModel>();
                    SelectedSection = "Dashboard";
                    break;

                case UserRole.User:
                    _navigationService.NavigateTo<HomeViewModel>();
                    SelectedSection = "Inicio";
                    break;

                case UserRole.Guest:
                    _navigationService.NavigateTo<HomeViewModel>();
                    SelectedSection = "Inicio";
                    break;
            }
        }

        private void AddMenuOption(string name, Action action)
        {
            var parts = name.Split(' ', 2);
            var icon = parts.Length > 1 ? parts[0] : "";
            var displayName = parts.Length > 1 ? parts[1] : name;

            MenuOptions.Add(new MenuOption
            {
                Name = displayName,
                Icon = icon,
                Command = new RelayCommand(() =>
                {
                    action?.Invoke();
                    SelectedSection = displayName;
                })
            });
        }
    }
}