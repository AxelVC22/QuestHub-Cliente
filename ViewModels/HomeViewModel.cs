using CommunityToolkit.Mvvm.Input;
using QuestHubClient.Models;
using QuestHubClient.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuestHubClient.ViewModels
{
    public partial class HomeViewModel : BaseViewModel
    {

        public ObservableCollection<Post> Posts { get; set; }

        //services
        private INavigationService _navigationService;


        //commands

        public HomeViewModel()
        {

        }

        public HomeViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;


            Posts = new ObservableCollection<Post>
            {
                new Post
                {
                    Title = "¿Cómo funciona el binding en WPF?",
                    Author = new User {Name= "roguez"},
                    Content = "Estoy aprendiendo MVVM y no entiendo bien cómo se enlazan los datos...",
                    Category =  new Category{Name = "Diseño"} ,
                    AnswersCount = 4,
                    AverageRating = 5.4
                },

            };
        }


        [RelayCommand]

        public void MakePost()
        {
            if (App.MainViewModel.IsRegistered)
            {
                _navigationService.NavigateTo<NewPostViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<LoginViewModel>();
            }

        }

        [RelayCommand]

        public void SeeDetails()
        {
            if (App.MainViewModel.IsRegistered)
            {
                _navigationService.NavigateTo<PostViewModel>();
            }
            else
            {
                _navigationService.NavigateTo<LoginViewModel>();
            }
        }
    }
}
