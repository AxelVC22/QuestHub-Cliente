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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuestHubClient.Views
{
    /// <summary>
    /// Lógica de interacción para PostView.xaml
    /// </summary>
    public partial class PostView : Page
    {
        public PostView(PostViewModel postViewModel)
        {
            InitializeComponent();
            this.DataContext = postViewModel;
        }
    }
}
