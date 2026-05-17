using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Navisworks.Api;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Infrastructure.Repositories
{
    /// <summary>
    /// Discovers NWC files appended into the active Navisworks document
    /// by enumerating <c>Document.Models</c> and filtering on the <c>.nwc</c> extension.
    /// Must be called on the Navisworks UI thread via <see cref="INavisDispatcher"/>.
    /// </summary>
    public sealed class NavisModelRepository : IModelRepository
    {
        private readonly INavisDispatcher _dispatcher;
        private readonly IAppLogger _logger;

        public NavisModelRepository(INavisDispatcher dispatcher, IAppLogger logger)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IReadOnlyList<NwcModelInfo> GetAppendedNwcModels()
        {
            return _dispatcher.Invoke(() =>
            {
                try
                {
                    var doc = Application.ActiveDocument;
                    if (doc == null)
                    {
                        _logger.Warn("GetAppendedNwcModels: No active document found.");
                        return (IReadOnlyList<NwcModelInfo>)Array.Empty<NwcModelInfo>();
                    }

                    var results = new List<NwcModelInfo>();
                    var models = doc.Models;

                    for (int i = 0; i < models.Count; i++)
                    {
                        var model = models[i];
                        if (model == null) continue;

                        var fileName = model.FileName;
                        if (string.IsNullOrWhiteSpace(fileName)) continue;

                        // Only include NWC files
                        var extension = Path.GetExtension(fileName);
                        if (!string.Equals(extension, ".nwc", StringComparison.OrdinalIgnoreCase))
                            continue;

                        results.Add(new NwcModelInfo(modelIndex: i, filePath: fileName));
                    }

                    _logger.Info($"Found {results.Count} appended NWC model(s).");
                    return results;
                }
                catch (Exception ex)
                {
                    _logger.Error("GetAppendedNwcModels failed.", ex);
                    return (IReadOnlyList<NwcModelInfo>)Array.Empty<NwcModelInfo>();
                }
            });
        }
    }
}
