using System;
using System.IO;

namespace NavisAutoClash.Core.Domain.Models
{
    /// <summary>
    /// Represents an NWC file that has been appended into the active Navisworks document.
    /// </summary>
    public sealed class NwcModelInfo : IEquatable<NwcModelInfo>
    {
        /// <summary>
        /// Zero-based index of this model within <c>Document.Models</c>.
        /// Used to re-locate the model in Navisworks API calls.
        /// </summary>
        public int ModelIndex { get; }

        /// <summary>Full file path to the source NWC file (as reported by Navisworks).</summary>
        public string FilePath { get; }

        /// <summary>File name without extension, used as a human-friendly label.</summary>
        public string DisplayName => Path.GetFileNameWithoutExtension(FilePath);

        public NwcModelInfo(int modelIndex, string filePath)
        {
            if (modelIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(modelIndex), "Model index must be >= 0.");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path must not be empty.", nameof(filePath));

            ModelIndex = modelIndex;
            FilePath = filePath;
        }

        /// <inheritdoc/>
        public bool Equals(NwcModelInfo? other) =>
            other is not null && ModelIndex == other.ModelIndex &&
            string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as NwcModelInfo);

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + ModelIndex.GetHashCode();
            hash = hash * 31 + StringComparer.OrdinalIgnoreCase.GetHashCode(FilePath);
            return hash;
        }

        /// <inheritdoc/>
        public override string ToString() => DisplayName;
    }
}
