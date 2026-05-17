using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Navisworks.Api;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Infrastructure.Repositories
{
    /// <summary>
    /// Reads selection sets from the active Navisworks document by recursively
    /// traversing the <c>Document.SelectionSets</c> folder tree.
    /// Must be called on the Navisworks UI thread via <see cref="INavisDispatcher"/>.
    /// </summary>
    public sealed class NavisSelectionSetRepository : ISelectionSetRepository
    {
        private readonly INavisDispatcher _dispatcher;
        private readonly IAppLogger _logger;

        public NavisSelectionSetRepository(INavisDispatcher dispatcher, IAppLogger logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IReadOnlyList<SelectionSetInfo> GetAllSelectionSets()
        {
            return _dispatcher.Invoke(() =>
            {
                try
                {
                    var doc = Application.ActiveDocument;
                    if (doc == null)
                    {
                        _logger.Warn("GetAllSelectionSets: No active document found.");
                        return (IReadOnlyList<SelectionSetInfo>)Array.Empty<SelectionSetInfo>();
                    }

                    var root = doc.SelectionSets.RootItem;
                    if (root == null)
                    {
                        _logger.Warn("GetAllSelectionSets: SelectionSets RootItem is null.");
                        return (IReadOnlyList<SelectionSetInfo>)Array.Empty<SelectionSetInfo>();
                    }

                    var results = new List<SelectionSetInfo>();
                    TraverseItems(root.Children, parentPath: string.Empty, results);
                    _logger.Info($"Found {results.Count} selection set(s).");
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.Error("GetAllSelectionSets failed.", ex);
                    return (IReadOnlyList<SelectionSetInfo>)Array.Empty<SelectionSetInfo>();
                }
            });
        }

        // ── private helpers ────────────────────────────────────────────────────

        private static void TraverseItems(
            IEnumerable<SavedItem> items,
            string parentPath,
            List<SelectionSetInfo> results)
        {
            foreach (var item in items)
            {
                var fullPath = string.IsNullOrEmpty(parentPath)
                    ? item.DisplayName
                    : $"{parentPath} / {item.DisplayName}";

                if (item.IsGroup)
                {
                    // Folder — recurse into children
                    TraverseItems(((GroupItem)item).Children, fullPath, results);
                }
                else if (item is SelectionSet set)
                {
                    var isSearchSet = set.HasSearch;
                    results.Add(new SelectionSetInfo(set.DisplayName, fullPath, isSearchSet));
                }
            }
        }
    }
}
