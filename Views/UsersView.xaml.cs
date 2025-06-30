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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuestHubClient.Views
{
    public partial class UsersView : Page
    {
        public UsersView()
        {
            InitializeComponent();
        }

        public UsersView(ViewModels.UsersViewModel usersViewModel)
        {
            InitializeComponent();
            this.DataContext = usersViewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.UsersViewModel viewModel)
            {
                viewModel.EditingUser.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
