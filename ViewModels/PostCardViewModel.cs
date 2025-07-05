using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace QuestHubClient.ViewModels
{
    public partial class PostCardViewModel : BaseViewModel
    {
        public bool IsOwner => App.MainViewModel.User?.Id == Post?.Author?.Id;
        public bool CanReport => !IsOwner && App.MainViewModel.User?.Id != null;
        public bool CanEdit => IsOwner;
        public bool CanDelete => IsOwner || IsAdmin || IsModerator;

        private IPostsService _postsService;
        private INavigationService _navigationService;
        private IAnswersService _answersService;
        private IFollowingService _followingService;
        private MultimediaUploadService _multimediaService;
        private IUserService _userService;

        private Models.Post _post;
        public Models.Post Post
        {
            get => _post;
            set
            {
                if (_post != value)
                {
                    _post = value;
                    OnPropertyChanged(nameof(Post));
                    OnPropertyChanged(nameof(IsOwner));
                    OnPropertyChanged(nameof(CanReport));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));

                    _ = LoadPostDataAsync();
                }
            }
        }

        private Answer _answer = new();
        public Answer Answer
        {
            get => _answer;
            set
            {
                if (_answer != value)
                {
                    _answer = value;
                    OnPropertyChanged(nameof(Answer));
                }
            }
        }

        private bool _isAnswerVisible;
        public bool IsAnswerVisible
        {
            get => _isAnswerVisible;
            set
            {
                if (_isAnswerVisible != value)
                {
                    _isAnswerVisible = value;
                    OnPropertyChanged(nameof(IsAnswerVisible));
                }
            }
        }

        private ObservableCollection<MultimediaFile> _multimediaFiles = new();
        public ObservableCollection<MultimediaFile> MultimediaFiles
        {
            get => _multimediaFiles;
            set
            {
                if (_multimediaFiles != value)
                {
                    _multimediaFiles = value;
                    OnPropertyChanged(nameof(MultimediaFiles));
                    OnPropertyChanged(nameof(HasMultimedia));
                    OnPropertyChanged(nameof(HasImages));
                    OnPropertyChanged(nameof(HasVideos));
                    OnPropertyChanged(nameof(Images));
                    OnPropertyChanged(nameof(Videos));
                }
            }
        }

        private ImageSource _authorProfileImage;
        public ImageSource AuthorProfileImage
        {
            get => _authorProfileImage;
            set => SetProperty(ref _authorProfileImage, value);
        }


        public bool HasMultimedia => MultimediaFiles?.Any() ?? false;
        public bool HasImages => MultimediaFiles?.Any(m => m?.IsImage == true) ?? false;
        public bool HasVideos => MultimediaFiles?.Any(m => m?.IsVideo == true) ?? false;

        public IEnumerable<MultimediaFile> Images => MultimediaFiles?.Where(m => m?.IsImage == true) ?? Enumerable.Empty<MultimediaFile>();
        public IEnumerable<MultimediaFile> Videos => MultimediaFiles?.Where(m => m?.IsVideo == true) ?? Enumerable.Empty<MultimediaFile>();

        public Action<PostCardViewModel> OnDeleted { get; set; }

        public PostCardViewModel()
        {
            _multimediaService = new MultimediaUploadService();
        }

        public PostCardViewModel(Models.Post post, INavigationService navigationService, IPostsService postsService,
            IAnswersService answersService, IFollowingService followingService, MultimediaUploadService multimediaService, IUserService userService)
        {
            _multimediaService = multimediaService;
            _userService = userService;
            Post = post;
            _navigationService = navigationService;
            _postsService = postsService;
            IsAnswerVisible = false;
            _answersService = answersService;
            _followingService = followingService;
        }

        private async Task LoadMultimediaAsync()
        {
            if (Post?.Id == null || _multimediaService == null)
                return;

            try
            {
                var multimediaFiles = await _multimediaService.GetMultimediaAsync(Post.Id.ToString());

                if (Application.Current?.Dispatcher == null)
                {
                    return;
                }

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        MultimediaFiles.Clear();

                        if (multimediaFiles != null && multimediaFiles.Any())
                        {
                            foreach (var file in multimediaFiles)
                            {
                                if (file != null)
                                {
                                    MultimediaFiles.Add(file);
                                }
                            }
                        }

                        OnPropertyChanged(nameof(HasMultimedia));
                        OnPropertyChanged(nameof(HasImages));
                        OnPropertyChanged(nameof(HasVideos));
                        OnPropertyChanged(nameof(Images));
                        OnPropertyChanged(nameof(Videos));
                    }
                    catch (Exception dispatcherEx)
                    {
                        ErrorMessage = "Error al actualizar la interfaz de multimedia";
                    }
                });
            }
            catch (ArgumentException argEx)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = "ID de post inválido para cargar multimedia";
                });
            }
            catch (Exception ex)
            {
                if (Application.Current?.Dispatcher != null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = $"Error al cargar multimedia: {ex.Message}";
                    });
                }
            }
        }

        [RelayCommand]
        public async Task RefreshMultimedia()
        {
            await LoadMultimediaAsync();
        }

        [RelayCommand]
        public void OpenImage(MultimediaFile imageFile)
        {
            if (imageFile?.IsImage == true && !string.IsNullOrEmpty(imageFile.FilePath))
            {
                try
                {
                    var imageViewer = new ImageViewerWindow(imageFile);
                    imageViewer.ShowDialog();
                }
                catch (Exception ex)
                {
                    ErrorMessage = "Error al abrir la imagen";
                }
            }
        }

        [RelayCommand]
        public void SeeUserDetails()
        {
            if (Post?.Author != null)
            {
                _navigationService.NavigateTo<ProfileViewModel>(Post.Author);
            }
        }

        [RelayCommand]
        public void SeeDetails()
        {
            if (Post != null && _navigationService != null)
            {
                _navigationService.NavigateTo<PostViewModel>(Post);
            }
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void SendAnswer()
        {
            if (Answer == null || Post?.Id == null || App.MainViewModel?.User == null)
                return;

            this.Answer.Author = App.MainViewModel.User;
            Answer.Post = new Models.Post { Id = Post.Id };

            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(Answer);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Answer, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.FirstOrDefault()?.ErrorMessage ?? "Error de validación";
                    return;
                }

                var (answer, message) = await _answersService.CreateAnswerAsync(Answer);

                if (answer != null)
                {
                    new NotificationWindow("Respuesta enviada con éxito", 3).Show();
                    Answer.Content = string.Empty;
                }
                else
                {
                    new NotificationWindow(message ?? "Error al enviar la respuesta", 3).Show();
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

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public void StartAnswer()
        {
            if (App.MainViewModel?.User == null)
            {
                new NotificationWindow("Inicia sesión para responder un post", 3).Show();
                return;
            }

            IsAnswerVisible = !IsAnswerVisible;
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void Follow()
        {
            if (Post?.Author?.Id == null || App.MainViewModel?.User?.Id == null)
                return;

            try
            {
                var (message, success) = await _followingService.FollowUserAsync(Post.Author.Id, App.MainViewModel.User.Id);

                if (success && Post.Author != null)
                {
                    Post.Author.IsFollowed = true;
                }

                new NotificationWindow(message ?? "Operación completada", 3).Show();
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

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void Unfollow()
        {
            if (Post?.Author?.Id == null || App.MainViewModel?.User?.Id == null)
                return;

            try
            {
                var (message, success) = await _followingService.UnfollowUserAsync(Post.Author.Id, App.MainViewModel.User.Id);

                if (success && Post.Author != null)
                {
                    Post.Author.IsFollowed = false;
                }

                new NotificationWindow(message ?? "Operación completada", 3).Show();
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

        [RelayCommand(CanExecute = nameof(CanEdit))]
        public void Edit()
        {
            if (Post != null && _navigationService != null)
            {
                _navigationService.NavigateTo<NewPostViewModel>(Post);
            }
        }

        [RelayCommand(CanExecute = nameof(CanReport))]
        public void Report()
        {
            if (Post == null || App.MainViewModel?.User == null)
                return;

            var result = MessageBox.Show("¿Estás seguro que desea reportar la publicación?",
                                      "Reportar",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            Report report = new Report
            {
                Post = Post,
                Reporter = App.MainViewModel.User,
            };

            _navigationService?.NavigateTo<NewReportViewModel>(report);
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public async void Delete()
        {
            if (Post?.Id == null)
                return;

            var result = MessageBox.Show("¿Estás seguro que desea eliminar la publicación?",
                                      "Eliminar",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                var (message, success) = await _postsService.DeletePostAsync(Post.Id);

                if (success)
                {
                    OnDeleted?.Invoke(this);
                }

                new NotificationWindow(message ?? "Operación completada", 3).Show();
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

        private async Task LoadAuthorProfilePictureAsync(string authorId)
        {
            try
            {
                var (imageData, contentType, message) = await _userService.GetProfilePictureAsync(authorId);
                if (imageData != null && imageData.Length > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AuthorProfileImage = LoadImage(imageData);
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AuthorProfileImage = null;
                    });
                }
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AuthorProfileImage = null;
                });
            }
        }

        private ImageSource LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            using var ms = new System.IO.MemoryStream(imageData);
            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        private async Task LoadPostDataAsync()
        {
            try
            {
                if (Post?.Id != null)
                {
                    await LoadMultimediaAsync();
                }

                if (Post?.Author?.Id != null)
                {
                    await LoadAuthorProfilePictureAsync(Post.Author.Id);
                }
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = "Error al cargar algunos datos del post";
                });
            }
        }

    }
}