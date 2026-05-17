using NavisAutoClash.Core.Domain.Models;
using NavisAutoClash.UI.Infrastructure;

namespace NavisAutoClash.UI.ViewModels
{
    /// <summary>Represents a single Selection B entry (an NWC model) in the list.</summary>
    public sealed class NwcModelRowViewModel : ViewModelBase
    {
        private bool _isSelected;

        public NwcModelInfo Model { get; }

        /// <summary>Friendly name (filename without extension) shown in the list.</summary>
        public string DisplayName => Model.DisplayName;

        /// <summary>Full file path shown as a tooltip.</summary>
        public string FilePath => Model.FilePath;

        /// <summary>Whether this entry is checked by the user.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public NwcModelRowViewModel(NwcModelInfo model)
        {
            Model = model;
        }
    }
}
