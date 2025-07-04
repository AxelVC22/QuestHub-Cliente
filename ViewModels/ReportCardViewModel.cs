using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
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
    public partial class ReportCardViewModel : BaseViewModel
    {

        private IReportsService _reportsService;

        private Report _report;

        public bool IsPostVisible { get; set; }

        public bool IsAnswerVisible { get; set; }
        public PostCardViewModel PostCard { get; set; }

        public AnswerCardViewModel AnswerCard { get; set; }

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

        private bool _isSanctionVisible;

        public bool IsSanctionVisible
        {
            get => _isSanctionVisible;
            set
            {
                if (_isSanctionVisible != value)
                {
                    _isSanctionVisible = value;
                    OnPropertyChanged(nameof(IsSanctionVisible));
                }
            }
        }

        public ObservableCollection<string> SuspensionOptions { get; }

        public Dictionary<string, int?> SuspensionMap { get; }

        private string _selectedOption;
        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                SetProperty(ref _selectedOption, value);
                CalculateEndDate();
            }
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }
        }


        public ReportCardViewModel() { }

        public ReportCardViewModel(Report report, INavigationService navigationService, IPostsService postsService, IAnswersService answersService, IFollowingService followingService, 
            IRatingsService ratingsService, IReportsService reportsService, MultimediaUploadService multimediaUploadService, IUserService userService)
        {
            Report = report;
            IsSanctionVisible = false;
            _reportsService = reportsService;

            if (report.Post != null)
            {
                PostCard = new PostCardViewModel(report.Post, navigationService, postsService, answersService, followingService, multimediaUploadService, userService);
                IsPostVisible = true;
            }
            else if (report.Answer != null)
            {
                AnswerCard = new AnswerCardViewModel(report.Answer, navigationService, answersService, ratingsService, followingService);
                IsAnswerVisible = true;
            }

            SuspensionMap = new Dictionary<string, int?>
            {
                { "1 día", 1 },
                { "3 días", 3 },
                { "7 días", 7 },
                { "Definitivo", null }
            };

            SuspensionOptions = new ObservableCollection<string>(SuspensionMap.Keys);

        }

        private void CalculateEndDate()
        {
            if (SuspensionMap.TryGetValue(SelectedOption, out int? days) && days.HasValue)
            {
                EndDate = DateTime.Now.AddDays(days.Value);
            }
            else
            {
                EndDate = null;
            }
        }

        [RelayCommand]
        private void Sanction()
        {
            IsSanctionVisible = true;
        }

        [RelayCommand]
        private void Ignore()
        {

        }

        [RelayCommand]
        private async void Send()
        {
            try
            {
                _report.Status = "Sanctioned";
                _report.EndBanDate = EndDate;

                var(checkedReport, message) = await _reportsService.UpdateReportAsync(_report);

                if (checkedReport != null)
                {
                    new NotificationWindow("Reporte actualizado con éxito", 3).Show();
                    Report = checkedReport;
                    IsSanctionVisible = false;
                }
                else
                {
                    ErrorMessage = message ?? "Error al actualizar el reporte.";
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
        private void Cancel()
        {
            IsSanctionVisible = false;
        }
    }
}
