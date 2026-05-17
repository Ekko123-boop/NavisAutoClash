using NavisAutoClash.Core.Domain.Models;
using NavisAutoClash.UI.Infrastructure;

namespace NavisAutoClash.UI.ViewModels
{
    /// <summary>
    /// Configures the clash rule settings: test type, tolerance, and naming pattern.
    /// </summary>
    public sealed class ClashRuleViewModel : ViewModelBase
    {
        private ClashType _testType = ClashType.Hard;
        private double _tolerance = 0.0;
        private string _namingPattern = "{SetA} vs {ModelB}";
        private string? _validationError;

        public ClashType TestType
        {
            get => _testType;
            set { SetProperty(ref _testType, value); OnPropertyChanged(nameof(ShowTolerance)); }
        }

        public double Tolerance
        {
            get => _tolerance;
            set { SetProperty(ref _tolerance, value); Validate(); }
        }

        public string NamingPattern
        {
            get => _namingPattern;
            set { SetProperty(ref _namingPattern, value); Validate(); }
        }

        /// <summary>Non-null when there is a validation error; null when config is valid.</summary>
        public string? ValidationError
        {
            get => _validationError;
            private set => SetProperty(ref _validationError, value);
        }

        /// <summary>True when the tolerance field should be shown (Clearance mode).</summary>
        public bool ShowTolerance => TestType == ClashType.Clearance;

        public bool IsValid => ValidationError == null;

        /// <summary>Builds an immutable <see cref="ClashRuleConfig"/> from current values, or null if invalid.</summary>
        public ClashRuleConfig? BuildConfig()
        {
            Validate();
            if (!IsValid) return null;

            return new ClashRuleConfig(
                presetName: "User Config",
                testType: TestType,
                tolerance: Tolerance,
                namingPattern: NamingPattern);
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(NamingPattern))
            { ValidationError = "Naming pattern is required."; return; }
            if (!NamingPattern.Contains("{SetA}") || !NamingPattern.Contains("{ModelB}"))
            { ValidationError = "Pattern must include {SetA} and {ModelB}."; return; }
            if (Tolerance < 0)
            { ValidationError = "Tolerance must be 0 or greater."; return; }

            ValidationError = null;
        }
    }
}
