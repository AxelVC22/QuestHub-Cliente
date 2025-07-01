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
using System.Windows;

namespace QuestHubClient.ViewModels
{
    public partial class AnswerCardViewModel : BaseViewModel
    {

        public bool IsOwner => App.MainViewModel.User?.Id == Answer.Author.Id;

        public bool CanReport => !IsOwner;

        public bool CanEdit => IsOwner;
        public bool CanDelete => IsOwner || IsAdmin || IsModerator;

        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;

        private Answer _answer;

        public Answer Answer
        {
            get => _answer;
            set
            {
                if (_answer != value)
                {
                    _answer = value;
                    OnPropertyChanged(nameof(Answer));
                }
            }
        }

        public Action<AnswerCardViewModel> OnDeleted { get; set; }


        [ObservableProperty]
        private Answer _childAnswer = new();

        public ObservableCollection<AnswerCardViewModel> AnswerCards { get; } = new();


        private int _editableRating;

        public int EditableRating
        {
            get => _editableRating;
            set
            {
                if (_editableRating != value)
                {
                    _editableRating = value;
                    OnPropertyChanged(nameof(EditableRating));
                }
            }
        }

        private bool _isRatingVisible;

        public bool IsRatingVisible
        {
            get => _isRatingVisible;
            set
            {
                if (_isRatingVisible != value)
                {
                    _isRatingVisible = value;
                    OnPropertyChanged(nameof(IsRatingVisible));
                }
            }
        }


        private bool _isChildAnswerVisible;

        public bool IsChildAnswerVisible
        {
            get => _isChildAnswerVisible;
            set
            {
                if (_isChildAnswerVisible != value)
                {
                    _isChildAnswerVisible = value;
                    OnPropertyChanged(nameof(IsChildAnswerVisible));
                }
            }
        }

        private bool _areAnswersVisible;

        public bool AreAnswersVisible
        {
            get => _areAnswersVisible;
            set
            {
                if (_areAnswersVisible != value)
                {
                    _areAnswersVisible = value;
                    OnPropertyChanged(nameof(AreAnswersVisible));
                }
            }
        }


        public ObservableCollection<int> Stars { get; } = new(Enumerable.Range(1, 5));



        private INavigationService _navigationService;

        private IAnswersService _answersService;

        private IRatingsService _ratingService;

        private IFollowingService _followingService;
        public Action<AnswerCardViewModel> OnRated { get; set; }
        public Action<Answer> OnReplied { get; set; }

        public AnswerCardViewModel()
        {

        }

        public AnswerCardViewModel(Answer answer, INavigationService navigationService, IAnswersService answersService, IRatingsService ratingsService, IFollowingService followingService)
        {
            _answer = answer;
            _isRatingVisible = false;
            _isChildAnswerVisible = false;
            _areAnswersVisible = false;
            _navigationService = navigationService;
            _answersService = answersService;
            _ratingService = ratingsService;
            _followingService = followingService; 
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public void StartRating()
        {
            IsRatingVisible = !IsRatingVisible;
        }

        [RelayCommand]
        public async void SeeAnswers()
        {
            LoadAnswersAsync(Page, Limit);

            AreAnswersVisible = !AreAnswersVisible;
        }

        [RelayCommand]
        public async void SeeMore()
        {
            LoadAnswersAsync(Page, Limit);
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public void StartChildAnswer()
        {
            IsChildAnswerVisible = !IsChildAnswerVisible;
        }

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async Task RateAnswer(int selectedRating)
        {
            EditableRating = selectedRating;
            if (Answer == null || EditableRating <= 0)
                return;

            Rating rating = new Rating
            {
                Qualification = EditableRating,
                Answer = Answer,
                Author = App.MainViewModel.User
            };

            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(rating);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(rating, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (updatedAnswer, message) = _ratingService.CreateRatingAsync(rating).Result;

                if (updatedAnswer != null)
                {
                    new NotificationWindow("Calificación enviada con éxito", 3).Show();
                    await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);
                    Answer = updatedAnswer;
                    OnRated?.Invoke(this);
                }
                else
                {
                    new NotificationWindow(message ?? "Error al enviar la calificación", 3).Show();
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

        public async Task SendChildAnswer()
        {
            ChildAnswer.Author = App.MainViewModel.User;
            ChildAnswer.ParentAnswer = new Answer { Id = Answer.Id };


            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(ChildAnswer);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(ChildAnswer, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (answer, message) = _answersService.CreateAnswerAsync(ChildAnswer).Result;

                if (answer != null)
                {
                    new NotificationWindow("Respuesta enviado con éxito", 3).Show();
                    await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);
                    ChildAnswer.Content = string.Empty; 
                    OnRated?.Invoke(this);

                }
                else
                {
                    new NotificationWindow(message ?? "Error al enviar la respuesta", 3).Show();
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

        private async Task LoadAnswersAsync(int pageNumber, int limit, bool refreshVisible = false)
        {
            try
            {
                ErrorMessage = string.Empty;

                var (answers, page, message) = await _answersService.GetAnswersByAnswerAsync(Answer.Id, pageNumber, limit, App.MainViewModel.User?.Id);

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (answers != null && answers.Any())
                {
                    if (refreshVisible)
                    {

                        foreach (var ans in answers)
                        {
                            var existingCard = AnswerCards.FirstOrDefault(ac => ac.Answer.Id == ans.Id);
                            if (existingCard != null)
                            {
                                int index = AnswerCards.IndexOf(existingCard);
                                AnswerCards[index].Answer = ans;
                            }

                        }
                    }
                    else
                    {
                        Page += 1;

                        foreach (var ans in answers)
                        {
                            var cardVM = new AnswerCardViewModel(ans, _navigationService, _answersService, _ratingService, _followingService)
                            {
                                OnRated = OnChildAnswerRated,
                                OnDeleted = OnChildAnswerDeleted,
                            };

                            AnswerCards.Add(cardVM);
                        }
                    }
                }
                else if (!refreshVisible)
                {
                    new NotificationWindow("Aún no se encontraron respuestas", 3).Show();
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


        private async void OnChildAnswerRated(AnswerCardViewModel answerCardViewModel)
        {
            await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);
        }

        private void OnChildAnswerDeleted(AnswerCardViewModel answerCardViewModel)
        {
            // Otra lógica
        }


        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void Follow()
        {
            try
            {
                var (message, success) = await _followingService.FollowUserAsync(Answer.Author.Id, App.MainViewModel.User.Id);

                if (success)
                {
                    Answer.Author.IsFollowed = true;

                }


                new NotificationWindow(message, 3).Show();


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

        [RelayCommand(CanExecute = nameof(CanDoOnlyLoggedAction))]
        public async void Unfollow()
        {
            try
            {
                var (message, success) = await _followingService.UnfollowUserAsync(Answer.Author.Id, App.MainViewModel.User.Id);

                if (success)
                {
                    Answer.Author.IsFollowed = false;

                }

                new NotificationWindow(message, 3).Show();


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

        [RelayCommand(CanExecute = nameof(CanEdit))]
        public void Edit()
        {
           // _navigationService.NavigateTo<NewPostViewModel>(Post);
        }

        [RelayCommand(CanExecute = nameof(CanDelete))]
        public async void Delete()
        {

            var result = MessageBox.Show("¿Estás seguro que desea eliminar la publicación?",
                                      "Cerrar Sesión",
                                      MessageBoxButton.YesNo,
                                      MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                //var (message, success) = await _postsService.DeletePostAsync(Post.Id);

                //if (success)
                //{
                //    Post.Author.IsFollowed = false;
                //    OnDeleted?.Invoke(this);

                //}

             //   new NotificationWindow(message, 3).Show();


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


    }
}
