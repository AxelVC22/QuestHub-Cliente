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
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 2;

        private INavigationService _navigationService;
        private ICategoriesService _categoriesService;
        private IPostsService _postsService;

        [ObservableProperty]
        private ObservableCollection<Category> categories = new();

        [ObservableProperty]
        private ObservableCollection<Category> selectedCategories = new();

        [ObservableProperty]
        private ObservableCollection<MultimediaFile> selectedFiles = new();

        // Tipos de archivo permitidos
        private readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp",
            ".mp4", ".avi", ".mov", ".wmv", ".mkv",
            ".pdf", ".txt", ".doc", ".docx"
        };

        private const long MaxFileSize = 20 * 1024 * 1024; // 20MB

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
        }


        public NewPostViewModel(Post post, INavigationService navigationService, ICategoriesService categoriesService, IPostsService postsService)
        {
            _navigationService = navigationService;
            _categoriesService = categoriesService;
            _postsService = postsService;

            SelectedFiles = new ObservableCollection<MultimediaFile>();
            Categories = new ObservableCollection<Category>();
            SelectedCategories = new ObservableCollection<Category>();

            LoadCategoriesAsync();
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
                        if (Post?.Categories == null || !Post.Categories.Any(c => c.Id == category.Id))
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
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleccionar archivos multimedia",
                Filter = "Archivos multimedia|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.pdf;*.txt;*.doc;*.docx|" +
                        "Imágenes|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                        "Videos|*.mp4;*.avi;*.mov;*.wmv;*.mkv|" ,
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                AddFiles(openFileDialog.FileNames);
            }
        }

        public void AddFiles(string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                AddSingleFile(filePath);
            }
            OnPropertyChanged(nameof(SelectedFiles));
            //// Forzar actualización de la UI
            //Application.Current?.Dispatcher?.Invoke(() =>
            //{
                
            //});
        }

        private void AddSingleFile(string filePath)
        {
            try
            {
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
                new NotificationWindow($"Archivo agregado: {fileInfo.Name}", 2).Show();
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
                new NotificationWindow($"Archivo eliminado: {file.FileName}", 2).Show();
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".mp4" => "video/mp4",
                ".avi" => "video/avi",
                ".mov" => "video/quicktime",
                ".wmv" => "video/x-ms-wmv",
                ".mkv" => "video/x-matroska",
                ".pdf" => "application/pdf",
                ".txt" => "text/plain",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        private string GetFileIcon(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" => "🖼️",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" => "🎥",
                ".pdf" => "📄",
                ".txt" => "📝",
                ".doc" or ".docx" => "📃",
                _ => "📁"
            };
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async Task Submit()
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

                // Validar el post
                var context = new ValidationContext(Post);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Post, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                // Crear el post primero
                var (createdPost, message) = await _postsService.CreatePostAsync(Post);

                if (createdPost == null)
                {
                    new NotificationWindow(message ?? "Error al crear la publicación", 3).Show();
                    return;
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