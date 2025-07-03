using QuestHubClient.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace QuestHubClient.Views
{
    public partial class VideoPlayerWindow : Window
    {
        public MultimediaFile VideoFile { get; set; }
        private DispatcherTimer timer;
        private bool isDragging = false;
        private bool isPlaying = false;

        public VideoPlayerWindow(MultimediaFile videoFile)
        {
            InitializeComponent();
            VideoFile = videoFile;
            DataContext = this;

            InitializeTimer();
            SetupVideo();
        }

        private void InitializeTimer()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }

        private void SetupVideo()
        {
            VideoPlayer.Volume = VolumeSlider.Value;
            VideoPlayer.Loaded += VideoPlayer_Loaded;
        }

        private void VideoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                UpdateTimeDisplay();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (VideoPlayer.Source != null && VideoPlayer.NaturalDuration.HasTimeSpan && !isDragging)
            {
                ProgressSlider.Value = VideoPlayer.Position.TotalSeconds;
                UpdateTimeDisplay();
            }
        }

        private void UpdateTimeDisplay()
        {
            var current = VideoPlayer.Position;
            var total = VideoPlayer.NaturalDuration.HasTimeSpan ? VideoPlayer.NaturalDuration.TimeSpan : TimeSpan.Zero;

            TimeDisplay.Text = $"{current:mm\\:ss} / {total:mm\\:ss}";
        }

        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (isPlaying)
            {
                VideoPlayer.Pause();
                PlayPauseButton.Content = "▶";
                timer.Stop();
                isPlaying = false;
            }
            else
            {
                VideoPlayer.Play();
                PlayPauseButton.Content = "⏸";
                timer.Start();
                isPlaying = true;
            }
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            PlayPauseButton.Content = "▶";
            timer.Stop();
            isPlaying = false;
            ProgressSlider.Value = 0;
            UpdateTimeDisplay();
        }

        private void ProgressSlider_DragStarted(object sender, DragStartedEventArgs e)
        {
            isDragging = true;
        }

        private void ProgressSlider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isDragging = false;
            VideoPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging)
            {
                VideoPlayer.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.Volume = e.NewValue;
            }
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayPauseButton.Content = "▶";
            timer.Stop();
            isPlaying = false;
            ProgressSlider.Value = 0;
            UpdateTimeDisplay();
        }

        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show($"Error al reproducir el video: {e.ErrorException?.Message}",
                          "Error de reproducción",
                          MessageBoxButton.OK,
                          MessageBoxImage.Error);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            timer?.Stop();
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            VideoPlayer.Stop();
            timer?.Stop();
            base.OnClosed(e);
        }
    }
}