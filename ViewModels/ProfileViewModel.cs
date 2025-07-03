using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace QuestHubClient.ViewModels
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IUserService _userService;

        private User _user;
        private ImageSource _profileImageSource;

        public User User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(IsOwnProfile));
                    OnPropertyChanged(nameof(IsNotOwnProfile));
                }
            }
        }

        public ImageSource ProfileImageSource
        {
            get => _profileImageSource;
            set => SetProperty(ref _profileImageSource, value);
        }

        public bool IsOwnProfile => User?.Id == App.MainViewModel?.User?.Id;
        public bool IsNotOwnProfile => !IsOwnProfile;

        private bool _isEditing = false;
        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged(nameof(IsEditing));
                    OnPropertyChanged(nameof(IsNotEditing));
                }
            }
        }

        public bool IsNotEditing => !IsEditing;

        private bool _isChangingAvatar = false;
        public bool IsChangingAvatar
        {
            get => _isChangingAvatar;
            set
            {
                if (_isChangingAvatar != value)
                {
                    _isChangingAvatar = value;
                    OnPropertyChanged(nameof(IsChangingAvatar));
                    OnPropertyChanged(nameof(IsNotChangingAvatar));
                    OnPropertyChanged(nameof(ProfileImageSource));
                }
            }
        }

        public bool IsNotChangingAvatar => !IsChangingAvatar;

        private byte[] _selectedImageData;
        private string _selectedImageFileName;

        public ProfileViewModel()
        {
            //User = new User();
        }

        public ProfileViewModel(INavigationService navigationService, IUserService userService, User user)
        {
            _navigationService = navigationService;
            _userService = userService;
            User = user;

            _ = LoadProfilePictureAsync();
        }

        [RelayCommand]
        private void Edit()
        {
            if (!IsOwnProfile) return;
            IsEditing = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            if (!IsOwnProfile) return;

            IsEditing = false;
            IsChangingAvatar = false;
            _selectedImageData = null;
            _selectedImageFileName = null;

            User = new User
            {
                Id = App.MainViewModel?.User?.Id,
                Name = App.MainViewModel?.User?.Name,
                Email = App.MainViewModel?.User?.Email,
                ProfilePicture = App.MainViewModel?.User?.ProfilePicture,
                Role = (UserRole)(App.MainViewModel?.User?.Role),
                Status = App.MainViewModel?.User?.Status,
                BanEndDate = App.MainViewModel?.User?.BanEndDate,
                FollowersCount = (int)App.MainViewModel?.User?.FollowersCount
            };

            OnPropertyChanged(nameof(ProfileImageSource));
        }

        [RelayCommand]
        private async Task SaveChanges()
        {
            if (!IsOwnProfile) return;

            var context = new ValidationContext(User);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(User, context, results, true);

            if (!string.IsNullOrWhiteSpace(User.Password))
            {
                if (User.Password.Length < 8 || User.Password.Length > 32)
                {
                    results.Add(new ValidationResult("La contraseña debe tener entre 8 y 32 caracteres", new[] { "Password" }));
                    isValid = false;
                }
            }

            if (!isValid)
            {
                ErrorMessage = results.First().ErrorMessage;
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(User.Password))
                {
                    var (userWithNewPassword, passwordMessage) = await _userService.UpdateUserPasswordAsync(User.Id, User.Password);
                    if (userWithNewPassword == null)
                    {
                        ErrorMessage = passwordMessage;
                        return;
                    }
                    User.Password = string.Empty;
                }

                var (updatedUser, message) = await _userService.UpdateUserAsync(User.Id, User);
                if (updatedUser != null)
                {
                    App.MainViewModel.User = updatedUser;
                    User = updatedUser;
                    User.Password = string.Empty;
                    IsEditing = false;
                    new NotificationWindow(message, 3).Show();
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = message ?? "Error al actualizar el perfil. Por favor, inténtalo de nuevo.";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inesperado: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ChangeAvatar()
        {
            if (!IsOwnProfile) return;

            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar imagen de perfil",
                Filter = "Archivos de imagen|*.jpg;*.jpeg;*.png|Todos los archivos|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = openFileDialog.FileName;
                    var fileName = Path.GetFileName(filePath);

                    var fileInfo = new FileInfo(filePath);
                    if (fileInfo.Length > 20 * 1024 * 1024)
                    {
                        ErrorMessage = "El archivo es demasiado grande (máximo 20MB)";
                        return;
                    }

                    _selectedImageData = File.ReadAllBytes(filePath);
                    _selectedImageFileName = fileName;

                    IsChangingAvatar = true;
                    ErrorMessage = string.Empty;

                    ProfileImageSource = LoadImage(_selectedImageData);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Error al cargar la imagen: {ex.Message}";
                }
            }
        }

        [RelayCommand]
        private async Task SaveAvatar()
        {
            if (!IsOwnProfile) return;

            if (_selectedImageData == null || string.IsNullOrEmpty(_selectedImageFileName))
            {
                ErrorMessage = "No se ha seleccionado ninguna imagen";
                return;
            }

            try
            {
                var (profilePicture, message) = await _userService.UpdateUserProfilePictureAsync(
                    User.Id, _selectedImageData, _selectedImageFileName);

                if (!string.IsNullOrEmpty(profilePicture))
                {
                    User.ProfilePicture = profilePicture;
                    if (App.MainViewModel?.User != null)
                    {
                        App.MainViewModel.User.ProfilePicture = profilePicture;
                    }
                    IsChangingAvatar = false;
                    _selectedImageData = null;
                    _selectedImageFileName = null;

                    await LoadProfilePictureAsync();

                    new NotificationWindow(message, 3).Show();
                    ErrorMessage = string.Empty;
                }
                else
                {
                    ErrorMessage = message ?? "Error al actualizar la foto de perfil";
                }
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inesperado: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelAvatarChange()
        {
            if (!IsOwnProfile) return;

            IsChangingAvatar = false;
            _selectedImageData = null;
            _selectedImageFileName = null;

            _ = LoadProfilePictureAsync();
        }

        public async Task LoadProfilePictureAsync()
        {
            try
            {
                var (imageData, contentType, message) = await _userService.GetProfilePictureAsync(User.Id);
                if (imageData != null && imageData.Length > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProfileImageSource = LoadImage(imageData);
                        OnPropertyChanged(nameof(ProfileImageSource));
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ProfileImageSource = null;
                        OnPropertyChanged(nameof(ProfileImageSource));
                    });
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ProfileImageSource = null;
                    OnPropertyChanged(nameof(ProfileImageSource));
                });
            }
        }

        private ImageSource LoadImage(byte[] imageData)
        {
            try
            {
                if (imageData == null || imageData.Length == 0)
                    return null;

                using var ms = new MemoryStream(imageData);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = ms;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar la imagen: {ex.Message}";
                return null;
            }
        }
    }
}