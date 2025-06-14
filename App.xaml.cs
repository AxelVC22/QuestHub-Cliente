using QuestHubClient.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuestHubClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MainWindowViewModel MainViewModel { get; set; } = new MainWindowViewModel();

    }

}
