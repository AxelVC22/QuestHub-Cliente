using QuestHubClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace QuestHubClient.Views
{
    public partial class LoginView : Page
    {
        public LoginViewModel _loginViewModel;

        public LoginView()
        {
           
        }

        public LoginView(LoginViewModel loginViewModel)
        {
            InitializeComponent();
            _loginViewModel = loginViewModel;
            this.DataContext = _loginViewModel;


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