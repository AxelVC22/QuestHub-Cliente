using CommunityToolkit.Mvvm.ComponentModel;

namespace QuestHubClient.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        private string _title = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}