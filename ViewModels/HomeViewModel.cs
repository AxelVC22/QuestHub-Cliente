using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QuestHubClient.Messaging;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuestHubClient.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {

        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;
        public ObservableCollection<PostCardViewModel> Posts { get; set; } = new ObservableCollection<PostCardViewModel>();

        //services
        private INavigationService _navigationService;

        private IPostsService _postsService;

        private IAnswersService _answersService;

        private IFollowingService _followingService;



        public HomeViewModel()
        {

        }

        public HomeViewModel(INavigationService navigationService, IPostsService postsService, IAnswersService answersService, IFollowingService followingService)
        {
            _navigationService = navigationService;

            _postsService = postsService;

            _answersService = answersService;

            _followingService = followingService;

            LoadPostsAsync(Page, Limit);

            WeakReferenceMessenger.Default.Register<PostMessage>(this, (r, message) =>
            {
                switch (message.Type)
                {
                    case PostMessageType.Deleted:
                        var toRemove = Posts.FirstOrDefault(p => p.Post.Id == message.PostId);
                        if (toRemove != null)
                            Posts.Remove(toRemove);
                        break;

                    case PostMessageType.Updated:
                        var updated = message.Payload as Post;
                        var toUpdate = Posts.FirstOrDefault(p => p.Post.Id == updated?.Id);
                        if (toUpdate != null)
                        {
                            toUpdate = new PostCardViewModel(updated, _navigationService, _postsService, _answersService, _followingService)
                            {
                                OnDeleted = OnAnswerDeleted,
                            };
                        }
                        break;
                }
            });
        }

        private void OnAnswerDeleted(PostCardViewModel postCardViewModel)
        {
            Posts.Remove(postCardViewModel);
        }

        private async Task LoadPostsAsync(int pageNumber, int limit)
        {
            try
            {
                ErrorMessage = string.Empty;

                string id = App.MainViewModel.User?.Id;

                var (posts, page, message) = await _postsService.GetPostsAsync(pageNumber, limit, id);

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (posts != null && posts.Any())
                {
                    Page = Page + 1;

                    foreach (var post in posts)
                    {
                        Posts.Add(new PostCardViewModel(post, _navigationService, _postsService, _answersService, _followingService)
                        {
                            OnDeleted = OnAnswerDeleted,
                        });
                    }
                }
                else
                {
                    new NotificationWindow("No se encontraron más publicaciones", 3).Show();
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

        public void MakePost()
        {
            if (App.MainViewModel.IsRegistered)
            {
                _navigationService.NavigateTo<NewPostViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<LoginViewModel>();
            }

        }

        [RelayCommand]

        public void SeeDetails(Post post)
        {


            _navigationService.NavigateTo<PostViewModel>(post);

        }

        [RelayCommand]
        public async Task SeeMoreAsync()
        {
            await LoadPostsAsync(Page, Limit);
        }


       

       
    }
}
