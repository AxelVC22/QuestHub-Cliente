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

namespace QuestHubClient.ViewModels
{
    public partial class PostViewModel : BaseViewModel
    {


        [ObservableProperty]
        private Post _post;

        private Answer _answer = new Answer();

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

        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;

        public ObservableCollection<AnswerCardViewModel> AnswerCards { get; } = new();


        private INavigationService _navigationService;

        private IAnswersService _answersService;

        private IRatingsService _ratingService;
        public PostViewModel()
        {

        }


        public PostViewModel(INavigationService navigationService, Post post, IAnswersService answersService, IRatingsService ratingsService)
        {
            _navigationService = navigationService;
            _post = post;
            _answersService = answersService;
            _ratingService = ratingsService;
            LoadAnswersAsync(Page, Limit);

        }

        private bool CanSendAnswer()
        {
            return App.MainViewModel.IsRegistered;
        }

        [RelayCommand]
        public void SeeUserDetails()
        {

        }

        private async Task LoadAnswersAsync(int pageNumber, int limit, bool refreshVisible = false)
        {
            try
            {
                ErrorMessage = string.Empty;

                var (answers, page, message) = await _answersService.GetAnswersByPostAsync(Post.Id, pageNumber, limit);

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
                            var cardVM = new AnswerCardViewModel(ans, _navigationService, _answersService, _ratingService)
                            {
                                OnRated = OnAnswerRated,
                                OnDeleted = OnAnswerDeleted,
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


        [RelayCommand(CanExecute = nameof(CanSendAnswer))]
        public async void SendAnswer()
        {
            this.Answer.Author = App.MainViewModel.User;
            Answer.Post = new Post { Id = Post.Id };


            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(Answer);
                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Answer, context, results, true);

                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (answer, message) = _answersService.CreateAnswerAsync(Answer).Result;

                if (answer != null)
                {
                    new NotificationWindow("Respuesta enviado con éxito", 3).Show();
                    await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);
                    Answer.Content = "enviado jaja"; 

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




        [RelayCommand]

        public void SeeDetails(Post post)
        {
            _navigationService.NavigateTo<PostViewModel>(post);
        }

        [RelayCommand]
        public async Task SeeMoreAsync()
        {
            await LoadAnswersAsync(Page, Limit);
        }





        private async void OnAnswerRated(AnswerCardViewModel updatedCard)
        {
           

            await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);

        }


        private async void OnAnswerDeleted(Answer a)
        {
            await LoadAnswersAsync(1, Page * Limit, refreshVisible: true);
        }

    }
}
