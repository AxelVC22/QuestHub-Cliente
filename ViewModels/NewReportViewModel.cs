using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using QuestHubClient.Views;
using QuestHubClient.Views.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace QuestHubClient.ViewModels
{
    public partial class NewReportViewModel : BaseViewModel
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

        private Report _report;

        public Report Report
        {
            get => _report;
            set
            {
                if (_report != value)
                {
                    _report = value;
                    OnPropertyChanged(nameof(Report));
                }
            }
        }

        private INavigationService _navigationService;

        private IPostsService _postsService;

        private IAnswersService _answersService;

        private IFollowingService _followingService;

        private IReportsService _reportsService;


        public NewReportViewModel(Report report, INavigationService navigationService, IPostsService postsService, IAnswersService answersService, IFollowingService followingService, IReportsService reportsService, MultimediaUploadService multimediaUploadService)
        {
            Report = report;
            _postCard = new PostCardViewModel(report.Post, navigationService, postsService, answersService, followingService, multimediaUploadService) { OnDeleted = OnPostDeleted };

            _navigationService = navigationService;
            _postsService = postsService;
            _answersService = answersService;
            _followingService = followingService;
            _reportsService = reportsService;

        }

        private void OnPostDeleted(PostCardViewModel postCardViewModel)
        {

        }


        [RelayCommand]
        private void Cancel()
        {
            _navigationService.GoBack();
        }

        [RelayCommand]
        private async void Submit()
        {
            try
            {
                ErrorMessage = string.Empty;

                var context = new ValidationContext(Report);

                var results = new List<ValidationResult>();
                bool isValid = Validator.TryValidateObject(Report, context, results, true);
                if (!isValid)
                {
                    ErrorMessage = results.First().ErrorMessage;
                    return;
                }

                var (createdPost, message) = _reportsService.CreateReportAsync(Report).Result;

                if (createdPost != null)
                {
                    new NotificationWindow("Reporte creado con éxito", 3).Show();
                }
                else
                {
                    new NotificationWindow(message ?? "Error al crear el reporte", 3).Show();

                }

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
    }
}
