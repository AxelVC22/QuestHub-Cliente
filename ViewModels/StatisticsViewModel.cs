using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using QuestHubClient.Models;
using QuestHubClient.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public partial class StatisticsViewModel : BaseViewModel
    {
        private readonly IUserService _userService;
        public event PropertyChangedEventHandler PropertyChanged;

        private int totalPosts;
        public int TotalPosts
        {
            get => totalPosts;
            set { totalPosts = value; OnPropertyChanged(nameof(TotalPosts)); }
        }

        private int totalAnswers;
        public int TotalAnswers
        {
            get => totalAnswers;
            set { totalAnswers = value; OnPropertyChanged(nameof(TotalAnswers)); }
        }


        public ObservableCollection<Axis> XAxes { get; set; }
        public ObservableCollection<Axis> YAxes { get; set; }
        public ObservableCollection<ISeries> RatingSeries { get; }



        public StatisticsViewModel()
        {
            // Define etiquetas de X
            XAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Labels = new[] { "1", "2", "3", "4", "5" },
                    Name = "Calificación"
                }
            };

            // Define eje Y
            YAxes = new ObservableCollection<Axis>
            {
                new Axis
                {
                    Name = "Cantidad",
                    // Opcional, definir min/max o etiquetas
                }
            };

            // Datos de la gráfica
            var values = new double[] { 3, 14, 21, 9, 4 };

            RatingSeries = new ObservableCollection<ISeries>
            {
                new ColumnSeries<double> { Values = values }
            };
        }

        public StatisticsViewModel(IUserService userService)
        {
            _userService = userService;

            _userService = userService;


            RatingSeries = new ObservableCollection<ISeries>();
            XAxes = new ObservableCollection<Axis>
            {
                new Axis { Labels = new List<string>(), Name = "Calificación" }
            };
            YAxes = new ObservableCollection<Axis>
            {
                new Axis { Name = "Cantidad" }
            };

            LoadStatisticsAsync();
        }


        private async void LoadStatisticsAsync()
        {

            try
            {
                var (statistics, message) = await _userService.GetStatisticsAsync(App.MainViewModel.User.Id);

                TotalPosts = statistics.TotalPosts;
                TotalAnswers = statistics.TotalAnswers;

                var labels = new List<string>();
                var values = new List<double>();

                // Asegura que los ratings de 1 a 5 estén en orden
                for (int i = 1; i <= 5; i++)
                {
                    string key = i.ToString();
                    labels.Add(key);
                    values.Add(statistics.Ratings != null && statistics.Ratings.ContainsKey(key)
                        ? statistics.Ratings[key]
                        : 0);
                }

                // Actualiza labels y series para la gráfica
                XAxes[0].Labels = labels;

                RatingSeries.Clear();
                RatingSeries.Add(new ColumnSeries<double> { Values = values });


                // Maneja error o mensaje


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
