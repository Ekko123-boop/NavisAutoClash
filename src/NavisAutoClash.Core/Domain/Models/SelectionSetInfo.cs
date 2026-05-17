using System;

namespace NavisAutoClash.Core.Domain.Models
{
    /// <summary>
    /// Represents a Navisworks selection set or search set discovered from the active document.
    /// </summary>
    public sealed class SelectionSetInfo : IEquatable<SelectionSetInfo>
    {
        /// <summary>Display name shown in the Navisworks UI.</summary>
        public string DisplayName { get; }

        /// <summary>
        /// Full path within the selection-set folder hierarchy, e.g. "Arch / Level 01 / Walls".
        /// Useful for disambiguation when duplicate names exist in different folders.
        /// </summary>
        public string FullPath { get; }

        /// <summary>Whether this is a search set (dynamic) rather than a static selection set.</summary>
        public bool IsSearchSet { get; }

        public SelectionSetInfo(string displayName, string fullPath, bool isSearchSet)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name must not be empty.", nameof(displayName));
            if (string.IsNullOrWhiteSpace(fullPath))
                throw new ArgumentException("Full path must not be empty.", nameof(fullPath));

            DisplayName = displayName;
            FullPath = fullPath;
            IsSearchSet = isSearchSet;
        }

        /// <inheritdoc/>
        public bool Equals(SelectionSetInfo? other) =>
            other is not null && string.Equals(FullPath, other.FullPath, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as SelectionSetInfo);

        /// <inheritdoc/>
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(FullPath);

        /// <inheritdoc/>
        public override string ToString() => FullPath;
    }
}
