using QuestHubClient.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace QuestHubClient
{
    public partial class App : Application
    {
        public static MainWindowViewModel MainViewModel { get; set; } = new MainWindowViewModel();

    }

}
