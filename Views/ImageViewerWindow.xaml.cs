using QuestHubClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuestHubClient.Views
{
    public partial class ImageViewerWindow : Window
    {
        public MultimediaFile ImageFile { get; set; }
        public ImageSource ImageSource { get; set; }

        public ImageViewerWindow(MultimediaFile imageFile)
        {
            InitializeComponent();
            ImageFile = imageFile;
            ImageSource = imageFile?.ImageSource;
            DataContext = this;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}