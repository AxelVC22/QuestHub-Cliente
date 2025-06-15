using QuestHubClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public class PostViewModel
    {

        private INavigationService _navigationService;
        public PostViewModel()
        {

        }


        public PostViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

        }
    }
}
