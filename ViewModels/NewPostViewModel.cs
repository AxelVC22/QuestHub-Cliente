using QuestHubClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuestHubClient.ViewModels
{
    public class NewPostViewModel
    {

        private INavigationService _navigationService;
        public NewPostViewModel()
        {
            // Initialize properties or commands if needed
        }

        public NewPostViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

        }
    }
}
