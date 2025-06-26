using CommunityToolkit.Mvvm.Input;

namespace QuestHubClient.Models
{
    public class MenuOption
    {
        public string Name { get; set; }
        public string Icon { get; set; } 
        public RelayCommand Command { get; set; }
    }
}