using System.Collections.Generic;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Core.Application.Contracts
{
    /// <summary>
    /// Retrieves selection sets from the active Navisworks document.
    /// Implemented in Infrastructure; never references Navisworks directly in Core.
    /// </summary>
    public interface ISelectionSetRepository
    {
        /// <summary>
        /// Returns all selection sets and search sets in the active document,
        /// flattened from the folder hierarchy.
        /// Returns an empty list (never null) if there are no sets or no active document.
        /// </summary>
        IReadOnlyList<SelectionSetInfo> GetAllSelectionSets();
    }
}
