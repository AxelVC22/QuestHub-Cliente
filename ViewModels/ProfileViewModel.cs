using QuestHubClient.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QuestHubClient.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        public User User { get; set; }

        [ObservableProperty]
        private bool isEditing = false;

        public bool IsNotEditing => !IsEditing;

        public ProfileViewModel()
        {

        }

        public ProfileViewModel(User user)
        {
            Title = "QuestHub - Perfil de Usuario";
            User = user;
        }

        [RelayCommand]
        private void Edit()
        {
            IsEditing = true;
            OnPropertyChanged(nameof(IsNotEditing));
        }

        [RelayCommand]
        private void Cancel()
        {
            IsEditing = false;
            OnPropertyChanged(nameof(IsNotEditing));
        }

        [RelayCommand]
        private void SaveChanges()
        {
            // Aquí podrías implementar la lógica para guardar los cambios del perfil
            // Por ejemplo, llamar a un servicio de usuario para actualizar la información
            IsEditing = false;
            OnPropertyChanged(nameof(IsNotEditing));
        }
    }
}
