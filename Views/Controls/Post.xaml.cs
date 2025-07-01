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

namespace QuestHubClient.Views.Controls
{
    /// <summary>
    /// Lógica de interacción para Post.xaml
    /// </summary>
    public partial class Post : UserControl
    {
        public Post()
        {
            InitializeComponent();

        }

        public static readonly DependencyProperty SeeUserDetailsCommandProperty =
           DependencyProperty.Register(
               nameof(SeeUserDetailsCommand),
               typeof(ICommand),
               typeof(Post),
               new PropertyMetadata(null)
           );

        public ICommand SeeUserDetailsCommand
        {
            get => (ICommand)GetValue(SeeUserDetailsCommandProperty);
            set => SetValue(SeeUserDetailsCommandProperty, value);
        }
    }
}
