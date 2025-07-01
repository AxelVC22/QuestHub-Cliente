using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class PostCardViewModel : BaseViewModel
    {
        public bool IsOwner => App.MainViewModel.User?.Id == Post.Author.Id;

        public bool CanReport => !IsOwner;



        public bool CanEdit => IsOwner;
        public bool CanDelete => IsOwner || IsAdmin || IsModerator;

        private IPostsService _postsService;

        private INavigationService _navigationService;

        private IAnswersService _answersService;

        private IFollowingService _followingService;

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

        public Action<PostCardViewModel> OnDeleted { get; set; }


        public PostCardViewModel()
        {

        }

        public PostCardViewModel(Models.Post post, INavigationService navigationService, IPostsService postsService, IAnswersService answersService, IFollowingService followingService)
        {
            Post = post;
            _navigationService = navigationService;
            _postsService = postsService;
            IsAnswerVisible = false;
            _answersService = answersService;
            _followingService = followingService;
        }




        [RelayCommand]

        public void SeeUserDetails()
        {
        }

        [RelayCommand]
        public void SeeDetails()
        {
            _navigationService.NavigateTo<PostViewModel>(Post);
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void SendAnswer()
        {
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
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (answer, message) = _answersService.CreateAnswerAsync(Answer).Result;

                if (answer != null)
                {
                    new NotificationWindow("Respuesta enviado con éxito", 3).Show();
                    Answer.Content = string.Empty;

                    // await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);

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
            IsAnswerVisible = !IsAnswerVisible;
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void Follow()
        {
            try
            {
                var (message, success) = await _followingService.FollowUserAsync(Post.Author.Id, App.MainViewModel.User.Id);

                if (success)
                {
                    Post.Author.IsFollowed = true;

                }


                new NotificationWindow(message, 3).Show();


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
            try
            {
                var (message, success) = await _followingService.UnfollowUserAsync(Post.Author.Id, App.MainViewModel.User.Id);

                if (success)
                {
                    Post.Author.IsFollowed = false;

                }

                new NotificationWindow(message, 3).Show();


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
            _navigationService.NavigateTo<NewPostViewModel>(Post);
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public async void Delete()
        {

            var result = MessageBox.Show("¿Estás seguro que desea eliminar la publicación?",
                                      "Cerrar Sesión",
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
                    Post.Author.IsFollowed = false;
                    OnDeleted?.Invoke(this);

                }

                new NotificationWindow(message, 3).Show();


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
    }
}