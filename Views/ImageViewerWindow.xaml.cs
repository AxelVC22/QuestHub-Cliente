using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuestHubClient.Views
{
    public partial class ImageViewerWindow : Window
    {
        public MultimediaFile ImageFile { get; set; }

        public ImageViewerWindow(MultimediaFile imageFile)
        {
            InitializeComponent();
            ImageFile = imageFile;
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
