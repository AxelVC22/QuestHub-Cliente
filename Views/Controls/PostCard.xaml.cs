using QuestHubClient.Models;
using System.Windows;
using System.Windows.Controls;

namespace QuestHubClient.Views.Controls
{
    public partial class PostCard : UserControl
    {
        public PostCard()
        {            
            InitializeComponent();
        }

        private void PlayVideo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is MultimediaFile videoFile)
            {
                var videoPlayer = new VideoPlayerWindow(videoFile);
                videoPlayer.Show();
            }
        }
    }
}
