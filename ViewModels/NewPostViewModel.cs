using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows;

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
       

        private INavigationService _navigationService;
        private ICategoriesService _categoriesService;
        private IPostsService _postsService;
        private MultimediaUploadService _multimediaService;

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

        [ObservableProperty]
        private ObservableCollection<MultimediaFile> selectedFiles = new();

        private readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp",
            ".mp4", ".avi"
        };

        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB
        private const int MaxFilesCount = 5; 

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
            SelectedFiles = new ObservableCollection<MultimediaFile>();
            //InitializeMultimediaService();
        }

        public NewPostViewModel(Post post, INavigationService navigationService, ICategoriesService categoriesService, IPostsService postsService, MultimediaUploadService multimediaUploadService)
        {
            _navigationService = navigationService;
            _categoriesService = categoriesService;
            _postsService = postsService;
            SelectedFiles = new ObservableCollection<MultimediaFile>();
            Categories = new ObservableCollection<Category>();

            LoadCategoriesAsync();
            if (post.Id != null)
            {
                Post = post;
                _postForUpdating = new Post
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    Category = post.Category != null ? new Category { Id = post.Category.Id, Name = post.Category.Name } : null,
                    Author = post.Author,
                };

                SelectedCategory = post.Category;

                IsUpdating = true;
                IsRegistering = false;
            }
            else
            {
                IsRegistering = true;
                IsUpdating = false;
            }

            _multimediaService = multimediaUploadService;
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                var (categories, message) = await _categoriesService.GetCategoriesAsync();

                if (categories != null && categories.Any())
                {
                    Page = Page + 1;

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
        private void SelectFiles()
        {
            if (SelectedFiles.Count >= MaxFilesCount)
            {
                new NotificationWindow($"Solo se permiten máximo {MaxFilesCount} archivos", 3).Show();
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivos multimedia",
                Filter = "Archivos multimedia|*.jpg;*.jpeg;*.png;*.gif;*.webp;*.mp4;*.avi|" +
                        "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.webp|" +
                        "Videos|*.mp4;*.avi;",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddFiles(openFileDialog.FileNames);
            }
        }

        public void AddFiles(string[] filePaths)
        {
            var filesToAdd = new List<string>();
            var remainingSlots = MaxFilesCount - SelectedFiles.Count;

            if (remainingSlots <= 0)
            {
                new NotificationWindow($"Solo se permiten máximo {MaxFilesCount} archivos", 3).Show();
                return;
            }

            if (filePaths.Length > remainingSlots)
            {
                filesToAdd.AddRange(filePaths.Take(remainingSlots));
                new NotificationWindow($"Solo se agregaron {remainingSlots} archivos. Límite máximo: {MaxFilesCount}", 3).Show();
            }
            else
            {
                filesToAdd.AddRange(filePaths);
            }

            foreach (var filePath in filesToAdd)
            {
                AddSingleFile(filePath);
            }
            OnPropertyChanged(nameof(SelectedFiles));
        }

        private void AddSingleFile(string filePath)
        {
            try
            {
                if (SelectedFiles.Count >= MaxFilesCount)
                {
                    new NotificationWindow($"Solo se permiten máximo {MaxFilesCount} archivos", 3).Show();
                    return;
                }

                if (!File.Exists(filePath))
                {
                    new NotificationWindow($"El archivo no existe: {Path.GetFileName(filePath)}", 3).Show();
                    return;
                }

                var fileInfo = new FileInfo(filePath);
                var extension = fileInfo.Extension.ToLowerInvariant();

                if (!_allowedExtensions.Contains(extension))
                {
                    new NotificationWindow($"Tipo de archivo no permitido: {extension}", 3).Show();
                    return;
                }

                if (fileInfo.Length > MaxFileSize)
                {
                    new NotificationWindow($"El archivo {fileInfo.Name} excede el tamaño máximo de 20MB", 3).Show();
                    return;
                }

                if (SelectedFiles.Any(f => f.FilePath == filePath))
                {
                    new NotificationWindow($"El archivo {fileInfo.Name} ya está agregado", 3).Show();
                    return;
                }

                var multimediaFile = new MultimediaFile
                {
                    FileName = fileInfo.Name,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    ContentType = GetContentType(extension),
                    FileIcon = GetFileIcon(extension)
                };

                SelectedFiles.Add(multimediaFile);
                //new NotificationWindow($"Archivo agregado: {fileInfo.Name} ({SelectedFiles.Count}/{MaxFilesCount})", 2).Show();
            }
            catch (Exception ex)
            {
                new NotificationWindow($"Error al agregar archivo: {ex.Message}", 3).Show();
            }
        }

        [RelayCommand]
        private void RemoveFile(MultimediaFile file)
        {
            if (file != null && SelectedFiles.Contains(file))
            {
                SelectedFiles.Remove(file);
                new NotificationWindow($"Archivo eliminado: {file.FileName} ({SelectedFiles.Count}/{MaxFilesCount})", 2).Show();
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                _ => "application/octet-stream"
            };
        }

        private string GetFileIcon(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => "🖼️",
                ".mp4" or ".avi" => "🎥",
                _ => "📁"
            };
        }

        private async Task<bool> UploadMultimediaFilesAsync(string postId)
        {
            if (!SelectedFiles.Any() || _multimediaService == null)
                return true;

            try
            {
                var totalFiles = SelectedFiles.Count;
                var uploadedCount = 0;

                foreach (var file in SelectedFiles)
                {
                    try
                    {
                        var fileUrl = await _multimediaService.UploadFileAsync(
                            postId,
                            file.FilePath,
                            file.ContentType
                        );

                        // file.UploadedUrl = fileUrl;
                        uploadedCount++;
                    }
                    catch (Exception ex)
                    {
                        ErrorMessage = $"Error al subir {file.FileName}: {ex.Message}";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al subir archivos: {ex.Message}";
                return false;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async void Submit()
        {
            Post.Category = SelectedCategory;
            Post.Author = App.MainViewModel.User;

            if (SelectedCategory == null)
            {
                new NotificationWindow("Debe seleccionar una categoría", 3).Show();
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

                var (createdPost, message) = await _postsService.CreatePostAsync(Post);

                if (createdPost == null)
                {
                    new NotificationWindow(message ?? "Error al crear la publicación", 3).Show();
                    return;
                }
                else
                {
                    if (SelectedFiles.Any())
                    {
                        var uploadSuccess = await UploadMultimediaFilesAsync(createdPost.Id);

                        if (!uploadSuccess)
                        {
                            new NotificationWindow("La publicación se creó pero hubo errores al subir algunos archivos", 4).Show();
                            return;
                        }
                    }
                }

                new NotificationWindow("Publicación creada con éxito", 3).Show();
                _navigationService.GoBack();
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
        private async void Save()
        {
            Post.Category = SelectedCategory;
            Post.Author = App.MainViewModel.User;

            if (SelectedCategory == null)
            {
                new NotificationWindow("Debe seleccionar una categoría", 3).Show();
                return;
            }

            bool categoryIsEqual = (_postForUpdating.Category?.Id == SelectedCategory?.Id);

            if (string.Equals(Post.Title, _postForUpdating.Title) &&
                string.Equals(Post.Content, _postForUpdating.Content) &&
                categoryIsEqual)
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

                var (updatedPost, message) = await _postsService.UpdatePostAsync(Post);

                if (updatedPost != null)
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
        private void SelectCategory(Category category)
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
    }
}