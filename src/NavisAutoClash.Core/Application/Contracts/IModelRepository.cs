using System.Collections.Generic;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Core.Application.Contracts
{
    /// <summary>
    /// Retrieves appended model information from the active Navisworks document.
    /// </summary>
    public interface IModelRepository
    {
        /// <summary>
        /// Returns all NWC models that are currently appended in the active document.
        /// Returns an empty list (never null) if there are no NWC files or no active document.
        /// </summary>
        IReadOnlyList<NwcModelInfo> GetAppendedNwcModels();
    }
}
