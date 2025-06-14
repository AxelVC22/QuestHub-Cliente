using System;
using System.Windows;
using System.Windows.Threading;

namespace QuestHubClient.Views
{
    public partial class NotificationWindow : Window
    {
        private readonly DispatcherTimer _closeTimer;

        public NotificationWindow(string message, int durationSeconds = 3)
        {
            InitializeComponent();
            MessageText.Text = message;

            Loaded += NotificationWindow_Loaded;

            _closeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };
            _closeTimer.Tick += (s, e) =>
            {
                _closeTimer.Stop();
                Close();
            };
        }

        private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            Left = desktopWorkingArea.Right - Width - 20;
            Top = desktopWorkingArea.Bottom - Height - 20;

            _closeTimer.Start();
        }
    }
}
