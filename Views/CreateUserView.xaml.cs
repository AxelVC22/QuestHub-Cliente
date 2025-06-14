using QuestHubClient.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace QuestHubClient.Views
{
    public partial class CreateUserView : Page
    {
        public CreateUserViewModel _createUserViewModel;
        public CreateUserView(CreateUserViewModel createUserViewModel)
        {
            InitializeComponent();
            _createUserViewModel = createUserViewModel;
            this.DataContext = _createUserViewModel;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateUserViewModel viewModel)
            {
                viewModel.User.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
