using System;
using System.Collections.Generic;
using System.Linq;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Core.Application.UseCases
{
    /// <summary>
    /// Generates the cartesian product of selected Selection A sets × Selection B NWC models,
    /// producing a list of <see cref="ClashTestDefinition"/> objects ready for the clash service.
    /// All logic here is pure C# — no Navisworks API dependency.
    /// </summary>
    public sealed class GenerateClashTestsUseCase
    {
        private readonly IClashService _clashService;
        private readonly IAppLogger _logger;

        public GenerateClashTestsUseCase(IClashService clashService, IAppLogger logger)
        {
            _clashService = clashService ?? throw new ArgumentNullException(nameof(clashService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Builds definitions from the A×B cartesian product and delegates creation to <see cref="IClashService"/>.
        /// </summary>
        /// <param name="selectedSets">Selection A — must contain at least one entry.</param>
        /// <param name="selectedModels">Selection B — must contain at least one entry.</param>
        /// <param name="ruleConfig">Rule configuration to apply to all generated tests.</param>
        /// <returns>Names of clash tests that were successfully created.</returns>
        /// <exception cref="InvalidOperationException">Thrown when inputs are empty.</exception>
        public IReadOnlyList<string> Execute(
            IEnumerable<SelectionSetInfo> selectedSets,
            IEnumerable<NwcModelInfo> selectedModels,
            ClashRuleConfig ruleConfig)
        {
            if (selectedSets == null) throw new ArgumentNullException(nameof(selectedSets));
            if (selectedModels == null) throw new ArgumentNullException(nameof(selectedModels));
            if (ruleConfig == null) throw new ArgumentNullException(nameof(ruleConfig));

            var sets = selectedSets.ToList();
            var models = selectedModels.ToList();

            if (sets.Count == 0)
                throw new InvalidOperationException("At least one Selection A set must be selected.");
            if (models.Count == 0)
                throw new InvalidOperationException("At least one Selection B model must be selected.");

            // Build A×B definitions
            var definitions = new List<ClashTestDefinition>(sets.Count * models.Count);
            foreach (var set in sets)
            {
                foreach (var model in models)
                {
                    var testName = ruleConfig.GenerateTestName(set.DisplayName, model.DisplayName);
                    definitions.Add(new ClashTestDefinition(
                        testName: testName,
                        selectionAName: set.DisplayName,
                        selectionBDisplayName: model.DisplayName,
                        ruleConfig: ruleConfig));
                }
            }

            _logger.Info($"Generating {definitions.Count} clash test(s) ({sets.Count} set(s) × {models.Count} model(s)).");

            var created = _clashService.CreateClashTests(definitions);

            _logger.Info($"Successfully created {created.Count} clash test(s).");
            return created;
        }

        /// <summary>
        /// Preview-only: generates definitions without calling the clash service.
        /// Used by the UI to show the user what tests will be created before committing.
        /// </summary>
        public IReadOnlyList<ClashTestDefinition> Preview(
            IEnumerable<SelectionSetInfo> selectedSets,
            IEnumerable<NwcModelInfo> selectedModels,
            ClashRuleConfig ruleConfig)
        {
            if (selectedSets == null) throw new ArgumentNullException(nameof(selectedSets));
            if (selectedModels == null) throw new ArgumentNullException(nameof(selectedModels));
            if (ruleConfig == null) throw new ArgumentNullException(nameof(ruleConfig));

            var definitions = new List<ClashTestDefinition>();
            foreach (var set in selectedSets)
            {
                foreach (var model in selectedModels)
                {
                    var testName = ruleConfig.GenerateTestName(set.DisplayName, model.DisplayName);
                    definitions.Add(new ClashTestDefinition(testName, set.DisplayName, model.DisplayName, ruleConfig));
                }
            }
            return definitions;
        }
    }
}
