using System;

namespace NavisAutoClash.Core.Domain.Models
{
    /// <summary>
    /// Represents a single clash test to be created — the output of the generation use case.
    /// This is a data-transfer object passed to <c>IClashService.CreateClashTests</c>.
    /// </summary>
    public sealed class ClashTestDefinition
    {
        /// <summary>The name that will appear in Navisworks Clash Detective.</summary>
        public string TestName { get; }

        /// <summary>Display name of the Selection A set.</summary>
        public string SelectionAName { get; }

        /// <summary>Display name of the Selection B NWC model.</summary>
        public string SelectionBDisplayName { get; }

        /// <summary>The rule configuration (type, tolerance) to apply to this test.</summary>
        public ClashRuleConfig RuleConfig { get; }

        public ClashTestDefinition(
            string testName,
            string selectionAName,
            string selectionBDisplayName,
            ClashRuleConfig ruleConfig)
        {
            if (string.IsNullOrWhiteSpace(testName))
                throw new ArgumentException("Test name must not be empty.", nameof(testName));
            if (string.IsNullOrWhiteSpace(selectionAName))
                throw new ArgumentException("Selection A name must not be empty.", nameof(selectionAName));
            if (string.IsNullOrWhiteSpace(selectionBDisplayName))
                throw new ArgumentException("Selection B display name must not be empty.", nameof(selectionBDisplayName));

            TestName = testName;
            SelectionAName = selectionAName;
            SelectionBDisplayName = selectionBDisplayName;
            RuleConfig = ruleConfig ?? throw new ArgumentNullException(nameof(ruleConfig));
        }

        /// <inheritdoc/>
        public override string ToString() => TestName;
    }
}
