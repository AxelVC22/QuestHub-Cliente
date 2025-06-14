using CommunityToolkit.Mvvm.ComponentModel;

namespace QuestHubClient.Models
{
    public partial class MenuItemModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _icon = string.Empty;

        [ObservableProperty]
        private bool _isSelected;
    }
}