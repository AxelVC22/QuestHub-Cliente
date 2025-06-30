using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Dtos;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class UsersViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        private readonly INavigationService _navigationService;

        private ObservableCollection<User> _users;
        private User _selectedUser;
        private User _editingUser;
        private bool _isEditing;
        private bool _isCreating;
        private string _searchText;
        private string _selectedRole;
        private string _selectedStatus;

        public bool HideForm => !ShowForm;

        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                if (SetProperty(ref _users, value))
                {
                    if (_users != null)
                    {
                        _users.CollectionChanged -= Users_CollectionChanged;
                    }
                    if (value != null)
                    {
                        value.CollectionChanged += Users_CollectionChanged;
                    }
                    OnPropertyChanged(nameof(FilteredUsers));
                }
            }
        }

        public ObservableCollection<User> FilteredUsers
        {
            get
            {
                if (Users == null || !Users.Any())
                    return new ObservableCollection<User>();

                var filtered = Users.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(u =>
                        (u.Name?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (u.Email?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                if (!string.IsNullOrWhiteSpace(SelectedRole) && SelectedRole != "Todos")
                {
                    filtered = filtered.Where(u => u.Role.ToString() == SelectedRole);
                }

                if (!string.IsNullOrWhiteSpace(SelectedStatus) && SelectedStatus != "Todos")
                {
                    filtered = filtered.Where(u =>
                        string.Equals(u.Status, SelectedStatus, StringComparison.OrdinalIgnoreCase));
                }

                return new ObservableCollection<User>(filtered.ToList());
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    OnPropertyChanged(nameof(HasSelectedUser));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanEnable));
                }
            }
        }

        public User EditingUser
        {
            get => _editingUser;
            set => SetProperty(ref _editingUser, value);
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
                    OnPropertyChanged(nameof(FilteredUsers));
                }
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (SetProperty(ref _selectedRole, value))
                {
                    OnPropertyChanged(nameof(FilteredUsers));
                }
            }
        }

        public string SelectedStatus
        {
            get => _selectedStatus;
            set
            {
                if (SetProperty(ref _selectedStatus, value))
                {
                    OnPropertyChanged(nameof(FilteredUsers));
                }
            }
        }

        public string EditingUserRoleString
        {
            get => EditingUser?.Role.ToString();
            set
            {
                if (EditingUser != null && Enum.TryParse<UserRole>(value, out var role))
                {
                    EditingUser.Role = role;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EditingUser));
                }
            }
        }

        public bool IsNotEditing => !IsEditing;
        public bool IsNotCreating => !IsCreating;
        public bool ShowForm => IsEditing || IsCreating;
        public bool HasSelectedUser => SelectedUser != null;
        public bool CanEdit => HasSelectedUser && !IsEditing && !IsCreating;
        public bool CanEnable => HasSelectedUser && !IsEditing && !IsCreating && SelectedUser.Status == "Inactive";
        public bool CanCreate => !IsEditing && !IsCreating;
        public string FormTitle => IsCreating ? "Nuevo Usuario" : "Editar Usuario";

        public ObservableCollection<string> Roles { get; }
        public ObservableCollection<string> RoleFilters { get; }
        public ObservableCollection<string> StatusFilters { get; }

        public UsersViewModel()
        {
        }

        public UsersViewModel(INavigationService navigationService, IUserService userService)
        {
            _userService = userService;
            _navigationService = navigationService;

            Users = new ObservableCollection<User>();
            EditingUser = new User();

            Roles = new ObservableCollection<string> { "Admin", "Moderator", "User" };
            RoleFilters = new ObservableCollection<string> { "Todos", "Admin", "Moderator", "User" };
            StatusFilters = new ObservableCollection<string> { "Todos", "Active", "Inactive" };

            SelectedRole = "Todos";
            SelectedStatus = "Todos";

            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private async Task LoadUsers()
        {
            try
            {
                ErrorMessage = string.Empty;

                var (users, message) = await _userService.GetUsersAsync();
                Users?.Clear();

                if (users == null || !users.Any())
                {
                    ErrorMessage = "No se encontraron usuarios.";
                    if (Users == null)
                    {
                        Users = new ObservableCollection<User>();
                    }
                    return;
                }

                if (Users == null)
                {
                    Users = new ObservableCollection<User>();
                }                    

                foreach (var user in users)
                {
                    Users.Add(user);
                }

                OnPropertyChanged(nameof(FilteredUsers));
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = $"Error de conexión: {ex.Message}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar los usuarios: {ex.Message}";
            }
        }

        [RelayCommand]
        private void CreateUser()
        {
            EditingUser = new User
            {
                Name = string.Empty,
                Email = string.Empty,
                Role = UserRole.User,
                Status = "Active"
            };
            IsCreating = true;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingUser));
            OnPropertyChanged(nameof(EditingUserRoleString));
        }

        [RelayCommand]
        private void EditUser()
        {
            if (SelectedUser == null) return;

            EditingUser = new User
            {
                Id = SelectedUser.Id,
                Name = SelectedUser.Name,
                Email = SelectedUser.Email,
                Role = SelectedUser.Role,
                Status = SelectedUser.Status,
                ProfilePicture = SelectedUser.ProfilePicture
            };
            IsEditing = true;
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingUser));
            OnPropertyChanged(nameof(EditingUserRoleString)); 
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            if (EditingUser == null) return;

            var context = new ValidationContext(EditingUser);
            var results = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(EditingUser, context, results, true);

            if (!string.IsNullOrWhiteSpace(EditingUser.Password))
            {
                if (EditingUser.Password.Length < 8 || EditingUser.Password.Length > 32)
                {
                    results.Add(new ValidationResult("La contraseña debe tener entre 8 y 32 caracteres", new[] { "Password" }));
                    isValid = false;
                }
            }

            if (string.IsNullOrWhiteSpace(EditingUser.Role.ToString()))
            {
                results.Add(new ValidationResult("El rol es requerido", new[] { "Role" }));
                isValid = false;
            }

            if (!isValid)
            {
                ErrorMessage = results.First().ErrorMessage;
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(EditingUser.Password))
                {
                    var (userWithNewPassword, passwordMessage) = await _userService.UpdateUserPasswordAsync(EditingUser.Id, EditingUser.Password);
                    if (userWithNewPassword == null)
                    {
                        ErrorMessage = passwordMessage;
                        return;
                    }
                    EditingUser.Password = string.Empty;
                }

                if (IsCreating)
                {
                    var (createdUser, message) = await _userService.RegisterUserAsync(EditingUser);
                    if (createdUser != null)
                    {
                        CancelEdit();
                        OnPropertyChanged(nameof(FilteredUsers));
                        new NotificationWindow("Usuario registrado exitosamente", 3).Show();
                        ErrorMessage = string.Empty;
                    }
                    else
                    {
                        ErrorMessage = message ?? "Error al registrar el usuario. Por favor, inténtalo de nuevo.";
                    }
                }
                else if (IsEditing)
                {
                    var (savedUser, message) = await _userService.UpdateUserAsync(EditingUser.Id, EditingUser);
                    if (savedUser != null)
                    {
                        var existingUser = Users.FirstOrDefault(u => u.Id == savedUser.Id);
                        if (existingUser != null)
                        {
                            var index = Users.IndexOf(existingUser);
                            Users[index] = savedUser;
                        }

                        if (savedUser.Id == App.MainViewModel.User.Id)
                        {
                            App.MainViewModel.User = savedUser;
                        }

                        CancelEdit();
                        OnPropertyChanged(nameof(FilteredUsers));
                        new NotificationWindow("Usuario actualizado exitosamente", 3).Show();
                        ErrorMessage = string.Empty;
                    }
                    else
                    {
                        ErrorMessage = message ?? "Error al actualizar el usuario. Por favor, inténtalo de nuevo.";
                    }
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
        private void CancelEdit()
        {
            IsEditing = false;
            IsCreating = false;
            EditingUser = new User
            {
                Name = string.Empty,
                Email = string.Empty,
                Role = UserRole.User,
                Status = "Active"
            };
            ErrorMessage = string.Empty;
            OnPropertyChanged(nameof(EditingUser));
        }

        [RelayCommand]
        private async Task EnableUser()
        {
            if (SelectedUser == null || SelectedUser.Status != "Inactive") return;

            var result = MessageBox.Show(
                $"¿Está seguro de que desea habilitar al usuario '{SelectedUser.Name}'?",
                "Confirmar habilitación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                ErrorMessage = string.Empty;

                var enabledUser = new User
                {
                    Id = SelectedUser.Id,
                    Name = SelectedUser.Name,
                    Email = SelectedUser.Email,
                    Role = SelectedUser.Role,
                    Status = "Active",
                    ProfilePicture = SelectedUser.ProfilePicture
                };

                var (updatedUser, message) = await _userService.UpdateUserAsync(SelectedUser.Id, enabledUser);

                if (updatedUser == null)
                {
                    ErrorMessage = message;
                    return;
                }

                var existingUser = Users.FirstOrDefault(u => u.Id == updatedUser.Id);
                if (existingUser != null)
                {
                    var index = Users.IndexOf(existingUser);
                    Users[index] = updatedUser;
                }

                SelectedUser = updatedUser;
                OnPropertyChanged(nameof(FilteredUsers));
                OnPropertyChanged(nameof(CanEnable));
                new NotificationWindow("Usuario habilitado exitosamente", 3).Show();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al habilitar el usuario: {ex.Message}";
            }
        }

        [RelayCommand]
        private void RefreshUsers()
        {
            _ = LoadUsersAsync();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedRole = "Todos";
            SelectedStatus = "Todos";
        }

        private async Task LoadUsersAsync()
        {
            await LoadUsers();
        }

        private void Users_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(FilteredUsers));
        }
    }
}