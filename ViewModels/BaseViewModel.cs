using CommunityToolkit.Mvvm.ComponentModel;
using QuestHubClient.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace QuestHubClient.ViewModels
{
    public abstract class BaseViewModel : ObservableObject, INotifyPropertyChanged
    {

        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;


        private string _title = string.Empty;

        public static bool IsAdmin => App.MainViewModel.User?.Role == UserRole.Admin;

        public static bool IsModerator => App.MainViewModel.User?.Role == UserRole.Moderator;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged(nameof(this.ErrorMessage));
                }
            }
        }

        private string _commandActionReason;

        public string CommandActionReason
        {
            get => _commandActionReason;
            set
            {
                if (_commandActionReason != value)
                {
                    _commandActionReason = value;
                    OnPropertyChanged(nameof(CommandActionReason));
                }
            }
        }

        public bool CanDoOnlyLoggedAction()
        {
            if (!App.MainViewModel.IsRegistered)
            {
                CommandActionReason = "Necesitas iniciar sesión";
                return false;
            }

            CommandActionReason = null;
            return true;
        }

        public ICommand GoBackCommand { get; set; }

     

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        }
    }
}