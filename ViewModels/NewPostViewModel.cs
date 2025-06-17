using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public partial class NewPostViewModel : BaseViewModel
    {

        public Post Post { get; set; } = new Post();

        private INavigationService _navigationService;

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ObservableCollection<Category> selectedCategories = new();
        public NewPostViewModel()
        {
        }


        public NewPostViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            Categories = new ObservableCollection<Category>
        {
            new Category { Id = "1", Name = "Programación" },
            new Category { Id = "2", Name = "Redes" },
            new Category { Id = "3", Name = "Seguridad" },
            new Category { Id = "4", Name = "Base de Datos" }
        };

        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private void Submit()
        {

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

            new NotificationWindow($"Categoría {category.Name} {(category.IsSelected ? "seleccionada" : "deseleccionada")}", 2).Show();
        }

    }
}
