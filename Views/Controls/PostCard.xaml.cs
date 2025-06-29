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
    /// Lógica de interacción para PostCard.xaml
    /// </summary>
    public partial class PostCard : UserControl
    {
        public PostCard()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty SeeDetailsCommandProperty =
            DependencyProperty.Register(
                nameof(SeeDetailsCommand),
                typeof(ICommand),
                typeof(PostCard),
                new PropertyMetadata(null)
            );

        public ICommand SeeDetailsCommand
        {
            get => (ICommand)GetValue(SeeDetailsCommandProperty);
            set => SetValue(SeeDetailsCommandProperty, value);
        }


    }
}
