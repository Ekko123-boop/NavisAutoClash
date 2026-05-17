using NavisAutoClash.Core.Domain.Models;
using NavisAutoClash.UI.Infrastructure;

namespace NavisAutoClash.UI.ViewModels
{
    /// <summary>Represents a single Selection A entry (a selection set) in the list.</summary>
    public sealed class SelectionSetRowViewModel : ViewModelBase
    {
        private bool _isSelected;

        public SelectionSetInfo Model { get; }

        /// <summary>Display name shown in the list.</summary>
        public string DisplayName => Model.DisplayName;

        /// <summary>Full path shown as a tooltip for disambiguation.</summary>
        public string FullPath => Model.FullPath;

        /// <summary>Whether this entry is checked by the user.</summary>
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsSearchSet => Model.IsSearchSet;

        public SelectionSetRowViewModel(SelectionSetInfo model)
        {
            Model = model;
        }
    }
}
