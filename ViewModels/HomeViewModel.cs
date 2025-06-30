using CommunityToolkit.Mvvm.Input;
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
        }

        private async Task LoadPostsAsync(int pageNumber, int limit)
        {
            try
            {
                ErrorMessage = string.Empty;

                var (posts, page, message) = await _postsService.GetPostsAsync(pageNumber, limit);

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (posts != null && posts.Any())
                {
                    Page = Page + 1;

                    foreach (var post in posts)
                    {
                        Posts.Add(new PostCardViewModel(post, _navigationService, _postsService, _answersService, _followingService));
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
