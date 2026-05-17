using System;

namespace NavisAutoClash.Core.Domain.Models
{
    /// <summary>
    /// Clash test type mirroring the Navisworks API enum so Core has no direct API dependency.
    /// </summary>
    public enum ClashType
    {
        Hard = 0,
        HardConservative = 1,
        Clearance = 2,
        Duplicates = 3
    }

    /// <summary>
    /// Configuration for a single clash rule / template.
    /// Immutable after construction; clone-and-modify to change settings.
    /// </summary>
    public sealed class ClashRuleConfig
    {
        /// <summary>Optional human-readable preset name (e.g. "Default Hard Clash").</summary>
        public string PresetName { get; }

        /// <summary>Type of clash to detect.</summary>
        public ClashType TestType { get; }

        /// <summary>
        /// Tolerance in model units (metres by default in Navisworks).
        /// Must be >= 0. Only meaningful when <see cref="TestType"/> is Clearance.
        /// </summary>
        public double Tolerance { get; }

        /// <summary>
        /// Naming pattern for generated tests.
        /// Supported tokens: {SetA} = Selection A name, {ModelB} = NWC model display name.
        /// Example: "{SetA} vs {ModelB}"
        /// </summary>
        public string NamingPattern { get; }

        /// <summary>Default configuration used when no preset is loaded.</summary>
        public static ClashRuleConfig Default => new ClashRuleConfig(
            presetName: "Default",
            testType: ClashType.Hard,
            tolerance: 0.0,
            namingPattern: "{SetA} vs {ModelB}");

        public ClashRuleConfig(string presetName, ClashType testType, double tolerance, string namingPattern)
        {
            if (string.IsNullOrWhiteSpace(presetName))
                throw new ArgumentException("Preset name must not be empty.", nameof(presetName));
            if (tolerance < 0)
                throw new ArgumentOutOfRangeException(nameof(tolerance), "Tolerance must be >= 0.");
            if (string.IsNullOrWhiteSpace(namingPattern))
                throw new ArgumentException("Naming pattern must not be empty.", nameof(namingPattern));
            if (!namingPattern.Contains("{SetA}") || !namingPattern.Contains("{ModelB}"))
                throw new ArgumentException("Naming pattern must contain '{SetA}' and '{ModelB}' tokens.", nameof(namingPattern));

            PresetName = presetName;
            TestType = testType;
            Tolerance = tolerance;
            NamingPattern = namingPattern;
        }

        /// <summary>Generates the test name for a given A/B pair using this config's naming pattern.</summary>
        public string GenerateTestName(string selectionSetName, string modelDisplayName)
        {
            if (string.IsNullOrWhiteSpace(selectionSetName))
                throw new ArgumentException("Selection set name must not be empty.", nameof(selectionSetName));
            if (string.IsNullOrWhiteSpace(modelDisplayName))
                throw new ArgumentException("Model display name must not be empty.", nameof(modelDisplayName));

            return NamingPattern
                .Replace("{SetA}", selectionSetName)
                .Replace("{ModelB}", modelDisplayName);
        }
    }
}
