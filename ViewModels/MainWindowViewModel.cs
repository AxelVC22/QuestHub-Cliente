using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.ViewModels;
using QuestHubClient.Views;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class MainWindowViewModel : BaseViewModel
    {
        
        public User User { get; set; }

        [ObservableProperty]
        private string _selectedSection = "Dashboard";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private int _notificationCount = 0;

        public MainWindowViewModel()
        {

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
            var profileView = new ProfileView();
            profileView.DataContext = new ProfileViewModel(User);
            profileView.ShowDialog();
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
                var loginWindow = new Views.LoginView();
                loginWindow.Show();

                Application.Current.Windows.OfType<Window>()
                    .Where(w => w.GetType().Name == "MainWindow")
                    .FirstOrDefault()?.Close();
            }
        }

        [RelayCommand]
        private void CreateNewPost()
        {

        }
    }
}