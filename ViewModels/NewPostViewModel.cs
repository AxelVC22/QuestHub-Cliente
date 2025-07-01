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

        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;
        public Post Post { get; set; } = new Post();

        private INavigationService _navigationService;

        private ICategoriesService _categoriesService;

        private IPostsService _postsService;
        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ObservableCollection<Category> selectedCategories = new();
        public NewPostViewModel()
        {
        }


        public NewPostViewModel(INavigationService navigationService, ICategoriesService categoriesService, IPostsService postsService)
        {
            _navigationService = navigationService;

            _categoriesService = categoriesService;

            LoadCategoriesAsync();
            _postsService = postsService;
        }


        private async Task LoadCategoriesAsync()
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
                        Categories.Add(category);
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
