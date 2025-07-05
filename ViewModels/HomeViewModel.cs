using CommunityToolkit.Mvvm.ComponentModel;
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
        private string _searchText;

        public string? SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        private Category _selectedCategory;

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }




        public ObservableCollection<PostCardViewModel> Posts { get; set; } = new ObservableCollection<PostCardViewModel>();

        //services
        private INavigationService _navigationService;

        private IPostsService _postsService;

        private IAnswersService _answersService;

        private IFollowingService _followingService;

        private MultimediaUploadService _multimediaUploadService;

        private IUserService _userService;
        private ICategoriesService _categoriesService;

        public HomeViewModel()
        {

        }

        public HomeViewModel(INavigationService navigationService, IPostsService postsService, IAnswersService answersService, 
            IFollowingService followingService, MultimediaUploadService multimediaUploadService, IUserService userService, ICategoriesService categoriesService)
        {
            _navigationService = navigationService;

            _postsService = postsService;

            _answersService = answersService;

            _followingService = followingService;

            _multimediaUploadService = multimediaUploadService;
            _categoriesService = categoriesService;

            _userService = userService;

            LoadPostsAsync(Page, Limit);
            LoadCategoriesAsync();


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
                            toUpdate = new PostCardViewModel(updated, _navigationService, _postsService, _answersService, _followingService, _multimediaUploadService, _userService)
                            {
                                OnDeleted = OnAnswerDeleted,
                            };
                        }
                        break;
                }
            });
            _userService = userService;
        }

        private void OnAnswerDeleted(PostCardViewModel postCardViewModel)
        {
            Posts.Remove(postCardViewModel);
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var (categories, message) = await _categoriesService.GetCategoriesAsync();

                if (categories != null && categories.Any())
                {

                    foreach (var category in categories)
                    {
                        if (SelectedCategory == null || SelectedCategory.Id != category.Id)
                        {
                            Categories.Add(category);
                        }
                    }
                }
                else
                {
                    new NotificationWindow("No se encontraron más categorias", 3).Show();
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
        private async Task SelectCategory(Category category)
        {
            if (category != null)
            {
                if (SelectedCategory != null)
                {
                    SelectedCategory.IsSelected = false;
                    if (!Categories.Contains(SelectedCategory))
                    {
                        Categories.Add(SelectedCategory);
                    }
                }

                SelectedCategory = category;
                category.IsSelected = true;

                if (Categories.Contains(category))
                {
                    Categories.Remove(category);
                }

                Page = 1;

                Posts.Clear();

                LoadPostsAsync(1, Limit, category.Id, SearchText);
            }
        }

        [RelayCommand]
        private void ClearSelectedCategory()
        {
            if (SelectedCategory != null)
            {
                SelectedCategory.IsSelected = false;

                if (!Categories.Contains(SelectedCategory))
                {
                    Categories.Add(SelectedCategory);
                }

                SelectedCategory = null;
            }
        }
        private async Task LoadPostsAsync(int pageNumber, int limit, string category = null, string content = null)
        {
            try
            {
                ErrorMessage = string.Empty;

                string id = App.MainViewModel.User?.Id;

                var (posts, page, message) = await _postsService.GetPostsAsync(pageNumber, limit, id, category, false, content);

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (posts != null && posts.Any())
                {
                    Page = Page + 1;

                    foreach (var post in posts)
                    {
                        Posts.Add(new PostCardViewModel(post, _navigationService, _postsService, _answersService, _followingService, _multimediaUploadService, _userService)
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
                App.MainViewModel.LoginCheck = false;
                _navigationService.NavigateTo<LoginViewModel>();
                new NotificationWindow("Necesitas iniciar sesión para crear una publicación", 3).Show();
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
            LoadPostsAsync(Page, Limit, SelectedCategory?.Id, SearchText);
        }


        [RelayCommand]
        public async Task Search()
        {
            Page = 1;

            Posts.Clear(); // Limpiar las publicaciones actualess

            LoadPostsAsync(Page, Limit, SelectedCategory?.Id, SearchText);

        }


    }
}
