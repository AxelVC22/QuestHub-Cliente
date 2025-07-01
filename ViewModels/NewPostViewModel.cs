using CommunityToolkit.Mvvm.ComponentModel;
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
using System.Windows.Media;

namespace QuestHubClient.ViewModels
{
    public partial class NewPostViewModel : BaseViewModel
    {
        private Post _post = new();

        private Post _postForUpdating;

        public Post Post
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
        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;

        private INavigationService _navigationService;

        private ICategoriesService _categoriesService;

        private IPostsService _postsService;
        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ObservableCollection<Category> selectedCategories = new();

        private bool _isRegistering;

        private bool _isUpdating;

        public bool IsRegistering
        {
            get => _isRegistering;
            set
            {
                if (_isRegistering != value)
                {
                    _isRegistering = value;
                    OnPropertyChanged(nameof(IsRegistering));
                }
            }
        }

        public bool IsUpdating
        {
            get => _isUpdating;
            set
            {
                if (_isUpdating != value)
                {
                    _isUpdating = value;
                    OnPropertyChanged(nameof(IsUpdating));
                }
            }
        }
        public NewPostViewModel()
        {
        }


        public NewPostViewModel(Post post, INavigationService navigationService, ICategoriesService categoriesService, IPostsService postsService)
        {
            _navigationService = navigationService;

            _categoriesService = categoriesService;

            LoadCategoriesAsync(post);
            _postsService = postsService;

            if (post != null)
            {
                Post = post;
                _postForUpdating = new Post
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Categories = post.Categories.Select(c => new Category { Id = c.Id, Name = c.Name }).ToList(),
                    Author = post.Author,
                };
                SelectedCategories = new ObservableCollection<Category>(post.Categories);
                IsUpdating = true;
            }
            else
            {
                IsRegistering = true;
            }

        }


        private async Task LoadCategoriesAsync(Post post)
        {
            try
            {
                ErrorMessage = string.Empty;

                var (categories, message) = await _categoriesService.GetCategoriesAsync();

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (categories != null && categories.Any())
                {
                    Page = Page + 1;

                    foreach (var category in categories)
                    {
                        if (post == null || !post.Categories.Any(c => c.Id == category.Id))
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
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private void Submit()
        {
            Post.Categories = SelectedCategories.ToList();
            Post.Author = App.MainViewModel.User;

            if (Post.Categories.Count == 0)
            {
                new NotificationWindow("Debe seleccionar al menos una categoría", 3).Show();
                return;
            }

            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(Post);

                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Post, context, results, true);
                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (createdPost, message) = _postsService.CreatePostAsync(Post).Result;

                if (createdPost != null)
                {
                    new NotificationWindow("Publicación creada con éxito", 3).Show();
                    _navigationService.GoBack();
                }
                else
                {
                    new NotificationWindow(message ?? "Error al crear la publicación", 3).Show();

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
        private void Save()
        {
            Post.Categories = SelectedCategories.ToList();
            Post.Author = App.MainViewModel.User;

            if (Post.Categories.Count == 0)
            {
                new NotificationWindow("Debe seleccionar al menos una categoría", 3).Show();
                return;
            }

            bool categoriesAreEqual = _postForUpdating.Categories.Count == Post.Categories.Count &&
                _postForUpdating.Categories.All(c => Post.Categories.Any(pc => pc.Id == c.Id));

            if (string.Equals(Post.Title, _postForUpdating.Title) && string.Equals(Post.Content, _postForUpdating.Content) && categoriesAreEqual)
            {
                new NotificationWindow("No se han realizado cambios en la publicación", 3).Show();
                return;
            }

            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(Post);

                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Post, context, results, true);
                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (createdPost, message) = _postsService.UpdatePostAsync(Post).Result;

                if (createdPost != null)
                {
                    new NotificationWindow("Publicación actualizada con éxito", 3).Show();
                    _navigationService.GoBack();
                }
                else
                {
                    new NotificationWindow(message ?? "Error al actualizar la publicación", 3).Show();

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
        private void ToggleCategory(Category category)
        {
            if (SelectedCategories.Contains(category))
                SelectedCategories.Remove(category);
            else
                SelectedCategories.Add(category);

            if (Categories.Contains(category))
                Categories.Remove(category);
            else
                Categories.Add(category);

            if (category != null)
                category.IsSelected = !category.IsSelected;
        }

    }
}
