using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Dtos;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class CategoriesViewModel : BaseViewModel
    {
        private readonly ICategoriesService _categoriesService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<Category> _categories;
        private Category _selectedCategory;
        private Category _editingCategory;
        private bool _isEditing;
        private bool _isCreating;
        private string _searchText;

        public bool HideForm => !ShowForm;

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public ObservableCollection<Category> FilteredCategories
        {
            get
            {
                if (Categories == null) return new ObservableCollection<Category>();

                if (string.IsNullOrWhiteSpace(SearchText))
                    return Categories;

                var filtered = Categories.Where(c =>
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                return new ObservableCollection<Category>(filtered);
            }
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCategory));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                    OnPropertyChanged(nameof(CanActivate));
                }
            }
        }

        public Category EditingCategory
        {
            get => _editingCategory;
            set => SetProperty(ref _editingCategory, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    OnPropertyChanged(nameof(IsNotEditing));
                    OnPropertyChanged(nameof(CanCreate));
                    OnPropertyChanged(nameof(ShowForm));
                    OnPropertyChanged(nameof(HideForm));
                }
            }
        }

        public bool IsCreating
        {
            get => _isCreating;
            set
            {
                if (SetProperty(ref _isCreating, value))
                {
                    OnPropertyChanged(nameof(IsNotCreating));
                    OnPropertyChanged(nameof(ShowForm));
                    OnPropertyChanged(nameof(HideForm));
                    OnPropertyChanged(nameof(CanCreate));
                    OnPropertyChanged(nameof(FormTitle));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    OnPropertyChanged(nameof(FilteredCategories));
                }
            }
        }

        public bool IsNotEditing => !IsEditing;
        public bool IsNotCreating => !IsCreating;
        public bool ShowForm => IsEditing || IsCreating;
        public bool HasSelectedCategory => SelectedCategory != null;
        public bool CanEdit => HasSelectedCategory && !IsEditing && !IsCreating;
        public bool CanDelete => HasSelectedCategory && !IsEditing && !IsCreating && SelectedCategory.Status == "Active";
        public bool CanActivate => HasSelectedCategory && !IsEditing && !IsCreating && SelectedCategory.Status == "Inactive";
        public bool CanCreate => !IsEditing && !IsCreating;
        public string FormTitle => IsCreating ? "Nueva Categoría" : "Editar Categoría";

        public CategoriesViewModel()
        {
        }

        public CategoriesViewModel(INavigationService navigationService, ICategoriesService categoriesService)
        {
            _categoriesService = categoriesService;
            _navigationService = navigationService;
            Categories = new ObservableCollection<Category>();
            EditingCategory = new Category();

            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private async Task LoadCategories()
        {
            try
            {
                ErrorMessage = string.Empty;

                var (categories, message) = await _categoriesService.GetCategoriesAsync();
                Categories.Clear();

                if (categories == null || !categories.Any())
                {
                    ErrorMessage = "No se encontraron categorías.";
                    return;
                }
                else
                {
                    foreach (var category in categories)
                    {
                        Categories.Add(category);
                    }
                }

                OnPropertyChanged(nameof(FilteredCategories));
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar las categorías: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CreateCategory()
        {
            EditingCategory = new Category
            {
                Name = string.Empty,
                Description = string.Empty,
                Status = "Active"
            };
            IsCreating = true;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingCategory));
        }

        [RelayCommand]
        private void EditCategory()
        {
            if (SelectedCategory == null) return;

            EditingCategory = new Category
            {
                Id = SelectedCategory.Id,
                Name = SelectedCategory.Name,
                Description = SelectedCategory.Description,
                Status = SelectedCategory.Status 
            };
            IsEditing = true;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingCategory));
        }

        [RelayCommand]
        private async Task SaveCategory()
        {
            if (EditingCategory == null) return;

            if (string.IsNullOrWhiteSpace(EditingCategory.Name))
            {
                ErrorMessage = "El nombre es requerido";
                return;
            }

            if (string.IsNullOrWhiteSpace(EditingCategory.Description))
            {
                ErrorMessage = "La descripción es requerida";
                return;
            }

            try
            {
                ErrorMessage = string.Empty;

                if (IsCreating)
                {
                    var createDto = new CreateCategoryDto
                    {
                        Name = EditingCategory.Name,
                        Description = EditingCategory.Description,
                        Status = "Active"
                    };

                    var (savedCategory, message) = await _categoriesService.CreateCategoryAsync(createDto);

                    if (savedCategory == null)
                    {
                        ErrorMessage = message;
                        return;
                    }
                    else
                    {
                        Categories.Add(savedCategory);
                        new NotificationWindow("Categoría creada exitosamente", 3).Show();
                    }
                }
                else if (IsEditing)
                {
                    var updateDto = new UpdateCategoryDto
                    {
                        Name = EditingCategory.Name,
                        Description = EditingCategory.Description,
                        Status = EditingCategory.Status 
                    };

                    var (savedCategory, message) = await _categoriesService.UpdateCategoryAsync(EditingCategory.Id, updateDto);

                    if (savedCategory == null)
                    {
                        ErrorMessage = message;
                        return;
                    }

                    var existingCategory = Categories.FirstOrDefault(c => c.Id == savedCategory.Id);
                    if (existingCategory != null)
                    {
                        var index = Categories.IndexOf(existingCategory);
                        Categories[index] = savedCategory;
                        new NotificationWindow("Categoría actualizada exitosamente", 3).Show();
                    }
                    else
                    {
                        ErrorMessage = message;
                        return;
                    }
                }

                CancelEdit();
                OnPropertyChanged(nameof(FilteredCategories));
            }
            catch (ArgumentException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar la categoría: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;
            IsCreating = false;
            EditingCategory = new Category
            {
                Name = string.Empty,
                Description = string.Empty,
                Status = "Active"
            };
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingCategory));
        }

        [RelayCommand]
        private async Task DeleteCategory()
        {
            if (SelectedCategory == null) return;

            var result = MessageBox.Show(
                $"¿Está seguro de que desea eliminar la categoría '{SelectedCategory.Name}'?\n\nEsta acción marcará la categoría como inactiva.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                ErrorMessage = string.Empty;

                var (deletedCategory, message) = await _categoriesService.DeleteCategoryAsync(SelectedCategory.Id);

                var existingCategory = Categories.FirstOrDefault(c => c.Id == deletedCategory.Id);
                if (existingCategory != null)
                {
                    var index = Categories.IndexOf(existingCategory);
                    Categories[index] = deletedCategory; 
                }

                SelectedCategory = null;
                OnPropertyChanged(nameof(FilteredCategories));
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanActivate));
                new NotificationWindow("Categoría eliminada exitosamente", 3).Show();
            }
            catch (KeyNotFoundException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar la categoría: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ActivateCategory()
        {
            if (SelectedCategory == null || SelectedCategory.Status != "Inactive") return;

            var result = MessageBox.Show(
                $"¿Está seguro de que desea activar la categoría '{SelectedCategory.Name}'?",
                "Confirmar activación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                ErrorMessage = string.Empty;

                var updateDto = new UpdateCategoryDto
                {
                    Name = SelectedCategory.Name,
                    Description = SelectedCategory.Description,
                    Status = "Active"
                };

                var (activatedCategory, message) = await _categoriesService.UpdateCategoryAsync(SelectedCategory.Id, updateDto);

                if (activatedCategory == null)
                {
                    ErrorMessage = message;
                    return;
                }

                var existingCategory = Categories.FirstOrDefault(c => c.Id == activatedCategory.Id);
                if (existingCategory != null)
                {
                    var index = Categories.IndexOf(existingCategory);
                    Categories[index] = activatedCategory;
                }

                SelectedCategory = activatedCategory;
                OnPropertyChanged(nameof(FilteredCategories));
                OnPropertyChanged(nameof(CanDelete));
                OnPropertyChanged(nameof(CanActivate));
                new NotificationWindow("Categoría activada exitosamente", 3).Show();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al activar la categoría: {ex.Message}";
            }
        }

        [RelayCommand]
        private void RefreshCategories()
        {
            _ = LoadCategoriesAsync();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        private async Task LoadCategoriesAsync()
        {
            await LoadCategories();
        }
    }
}