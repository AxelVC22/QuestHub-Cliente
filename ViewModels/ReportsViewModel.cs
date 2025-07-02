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

        public ReportsViewModel()
        {

        }

        public ReportsViewModel(IReportsService reportsService)
        {
            _reportsService = reportsService;

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
                        Reports.Add(new ReportCardViewModel(report));

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

    }
}
