using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Services;
using QuestHubClient.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace QuestHubClient.ViewModels
{
    public partial class ReportsViewModel : BaseViewModel
    {



        public ObservableCollection<ReportCardViewModel> Reports { get; set; } = new ObservableCollection<ReportCardViewModel>();


        private readonly IReportsService _reportsService;

        private readonly INavigationService _navigationService;

        private readonly IAnswersService _answersService;

        private readonly IPostsService _postsService;

        private readonly IRatingsService _ratingsService;

        private readonly IFollowingService _followingService;

        private readonly MultimediaUploadService _multimediaUploadService;
        public ReportsViewModel()
        {

        }

        public ReportsViewModel(IReportsService reportsService, INavigationService navigationService, IAnswersService answersService, IPostsService postsService, IRatingsService ratingsService, IFollowingService followingService, MultimediaUploadService multimediaUploadService)
        {
            _reportsService = reportsService;
            _navigationService = navigationService;
            _answersService = answersService;
            _postsService = postsService;
            _ratingsService = ratingsService;
            _followingService = followingService;
            _multimediaUploadService = multimediaUploadService;



            LoadReportsAsync(Page, Limit);

        }

        private async Task LoadReportsAsync(int pageNumber, int limit)
        {
            try
            {
                ErrorMessage = string.Empty;

                string id = App.MainViewModel.User?.Id;

                var (reports, page, message) = await _reportsService.GetReportsAsync(pageNumber, limit, "Pending");

                if (!string.IsNullOrEmpty(message))
                {
                    new NotificationWindow(message, 3).Show();
                }

                if (reports != null && reports.Any())
                {
                    Page = Page + 1;

                    foreach (var report in reports)
                    {
                        Reports.Add(new ReportCardViewModel(report, _navigationService, _postsService, _answersService, _followingService, _ratingsService, _reportsService, _multimediaUploadService));

                    }
                }
                else
                {
                    new NotificationWindow("No se encontraron más publicaciones", 3).Show();
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
        private void SeeMore()
        {

        }

    }
}
