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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public partial class PostViewModel : BaseViewModel
    {


        private PostCardViewModel _postCard;

        public PostCardViewModel PostCard
        {
            get => _postCard;
            set
            {
                if (_postCard != value)
                {
                    _postCard = value;
                    OnPropertyChanged(nameof(PostCard));
                }
            }
        }

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


        private bool _isAnswerVisible;

        public bool IsAnswerVisible
        {
            get => _isAnswerVisible;
            set
            {
                if (_isAnswerVisible != value)
                {
                    _isAnswerVisible = value;
                    OnPropertyChanged(nameof(IsAnswerVisible));
                }
            }
        }


        public int Page { get; set; } = 1;

        public int Limit { get; set; } = 2;

        public ObservableCollection<AnswerCardViewModel> AnswerCards { get; } = new();


        private INavigationService _navigationService;

        private IAnswersService _answersService;

        private IRatingsService _ratingService;

        private IFollowingService _followingService;
        public PostViewModel()
        {

        }


        public PostViewModel(INavigationService navigationService, Post post, IAnswersService answersService, IRatingsService ratingsService, IPostsService postsService, IFollowingService followingService)
        {
            _navigationService = navigationService;
            _postCard = new PostCardViewModel(post, navigationService, postsService, answersService, followingService) { OnDeleted = OnPostDeleted};
            _answersService = answersService;
            _ratingService = ratingsService;
            _followingService = followingService;
            LoadAnswersAsync(Page, Limit);

        }

        private void OnPostDeleted(PostCardViewModel postCardViewModel)
        {

            WeakReferenceMessenger.Default.Send(new PostMessage(PostMessageType.Deleted, postCardViewModel.Post.Id));

            _navigationService.GoBack();
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

                var (answers, page, message) = await _answersService.GetAnswersByPostAsync(PostCard.Post.Id, pageNumber, limit, App.MainViewModel.User?.Id);

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


        private async void OnAnswerDeleted(AnswerCardViewModel answerCardViewModel)
        {
            AnswerCards.Remove(answerCardViewModel);
        }

    

    }
}
