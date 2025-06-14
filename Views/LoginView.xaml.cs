using QuestHubClient.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace QuestHubClient.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.User.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}