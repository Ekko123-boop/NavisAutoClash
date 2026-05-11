using CommunityToolkit.Mvvm.ComponentModel;

namespace NavisAutoClash.UI.ViewModels
{
    public partial class NwcModelRowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private bool _isSelected;

        public NwcModelRowViewModel(string name)
        {
            Name = name;
        }
    }
}
