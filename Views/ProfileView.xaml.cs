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
    public partial class ProfileView : Page
    {
        public ProfileViewModel ProfileViewModel { get; set; }
        public ProfileView()
        {
            InitializeComponent();
        }
        public ProfileView(ProfileViewModel profileViewModel)
        {
            InitializeComponent();
            this.DataContext = profileViewModel;
            ProfileViewModel = profileViewModel;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.ProfileViewModel viewModel)
            {
                viewModel.User.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}
