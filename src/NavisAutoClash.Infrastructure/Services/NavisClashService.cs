using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.Clash;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Infrastructure.Services
{
    /// <summary>
    /// Creates and runs clash tests in the active Navisworks document using the Clash Detective API.
    /// All calls are dispatched to the Navisworks UI thread via <see cref="INavisDispatcher"/>.
    /// </summary>
    public sealed class NavisClashService : IClashService
    {
        private readonly INavisDispatcher _dispatcher;
        private readonly IAppLogger _logger;

        public NavisClashService(INavisDispatcher dispatcher, IAppLogger logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> CreateClashTests(IEnumerable<ClashTestDefinition> definitions)
        {
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));

            return _dispatcher.Invoke(() =>
            {
                var created = new List<string>();

                try
                {
                    var doc = Application.ActiveDocument;
                    if (doc == null)
                    {
                        _logger.Warn("CreateClashTests: No active document.");
                        return (IReadOnlyList<string>)created;
                    }

                    var documentClash = doc.GetClash();
                    if (documentClash == null)
                    {
                        _logger.Warn("CreateClashTests: Clash Detective is not available in this document.");
                        return (IReadOnlyList<string>)created;
                    }

                    // Build a set of existing test names to detect duplicates
                    var existingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (ClashTest existingTest in documentClash.TestsData.Tests)
                    {
                        if (existingTest?.DisplayName != null)
                            existingNames.Add(existingTest.DisplayName);
                    }

                    foreach (var definition in definitions)
                    {
                        try
                        {
                            CreateSingleTest(doc, documentClash, definition, existingNames, created);
                        }
                        catch (Exception ex)
                        {
                            // Log but continue — one failing test must not abort the rest
                            _logger.Error($"Failed to create test '{definition.TestName}'.", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("CreateClashTests encountered an unexpected error.", ex);
                }

                return (IReadOnlyList<string>)created;
            });
        }

        /// <inheritdoc/>
        public void RunClashTests(IEnumerable<string> testNames)
        {
            if (testNames == null) throw new ArgumentNullException(nameof(testNames));

            _dispatcher.Invoke(() =>
            {
                try
                {
                    var doc = Application.ActiveDocument;
                    if (doc == null) { _logger.Warn("RunClashTests: No active document."); return; }

                    var documentClash = doc.GetClash();
                    if (documentClash == null) { _logger.Warn("RunClashTests: Clash not available."); return; }

                    var nameSet = new HashSet<string>(testNames, StringComparer.OrdinalIgnoreCase);

                    foreach (ClashTest test in documentClash.TestsData.Tests)
                    {
                        if (test == null || !nameSet.Contains(test.DisplayName ?? string.Empty))
                            continue;

                        try
                        {
                            _logger.Info($"Running clash test: {test.DisplayName}");
                            documentClash.TestsData.TestsRunTest(test);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Failed to run test '{test.DisplayName}'.", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("RunClashTests encountered an unexpected error.", ex);
                }
            });
        }

        // ── private helpers ────────────────────────────────────────────────────

        private void CreateSingleTest(
            Document doc,
            DocumentClash documentClash,
            ClashTestDefinition definition,
            HashSet<string> existingNames,
            List<string> created)
        {
            // Duplicate-name guard
            if (existingNames.Contains(definition.TestName))
            {
                _logger.Warn($"Skipping '{definition.TestName}' — a test with this name already exists.");
                return;
            }

            // Find Selection A set by display name
            var setA = FindSelectionSet(doc, definition.SelectionAName);
            if (setA == null)
            {
                _logger.Warn($"Skipping '{definition.TestName}' — Selection A set '{definition.SelectionAName}' not found.");
                return;
            }

            // Find the NWC model items (Selection B = all items belonging to matching model)
            var modelBItems = FindModelItems(doc, definition.SelectionBDisplayName);
            if (modelBItems == null || modelBItems.Count == 0)
            {
                _logger.Warn($"Skipping '{definition.TestName}' — Selection B model '{definition.SelectionBDisplayName}' not found or empty.");
                return;
            }

            // Build the clash test
            var newTest = new ClashTest
            {
                DisplayName = definition.TestName,
                TestType = MapTestType(definition.RuleConfig.TestType),
                Tolerance = definition.RuleConfig.Tolerance
            };

            // Selection A — reference the existing selection set
            newTest.SelectionA.Selection.SelectionSources.Add(setA);

            // Selection B — explicit ModelItemCollection from the NWC model
            var selectionB = new ModelItemCollection(modelBItems);
            newTest.SelectionB.Selection.CopyFrom(selectionB);

            // Add to Clash Detective
            documentClash.TestsData.TestsAddCopy(newTest);
            existingNames.Add(definition.TestName);
            created.Add(definition.TestName);
            _logger.Info($"Created clash test: '{definition.TestName}'");
        }

        /// <summary>
        /// Recursively searches the selection-sets tree for a set with a matching display name.
        /// Returns the first match found (case-insensitive).
        /// </summary>
        private static SelectionSet? FindSelectionSet(Document doc, string displayName)
        {
            return FindInChildren(doc.SelectionSets.RootItem.Children, displayName);
        }

        private static SelectionSet? FindInChildren(IEnumerable<SavedItem> items, string displayName)
        {
            foreach (var item in items)
            {
                if (item.IsGroup)
                {
                    var found = FindInChildren(item.Children, displayName);
                    if (found != null) return found;
                }
                else if (item is SelectionSet ss &&
                         string.Equals(ss.DisplayName, displayName, StringComparison.OrdinalIgnoreCase))
                {
                    return ss;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns all root-level model items belonging to the NWC model whose
        /// file name (without extension) matches <paramref name="modelDisplayName"/>.
        /// </summary>
        private static List<ModelItem>? FindModelItems(Document doc, string modelDisplayName)
        {
            foreach (Model model in doc.Models)
            {
                if (model == null) continue;
                var name = System.IO.Path.GetFileNameWithoutExtension(model.FileName);
                if (!string.Equals(name, modelDisplayName, StringComparison.OrdinalIgnoreCase)) continue;

                // Return the root item's children so the clash test targets the whole model
                var items = new List<ModelItem>();
                if (model.RootItem != null)
                    items.Add(model.RootItem);
                return items;
            }
            return null;
        }

        /// <summary>Maps Core's <see cref="ClashType"/> to the Navisworks API <see cref="ClashTestType"/>.</summary>
        private static ClashTestType MapTestType(ClashType type) => type switch
        {
            ClashType.Hard => ClashTestType.Hard,
            ClashType.HardConservative => ClashTestType.HardConservative,
            ClashType.Clearance => ClashTestType.Clearance,
            ClashType.Duplicates => ClashTestType.Duplicates,
            _ => ClashTestType.Hard
        };
    }
}
