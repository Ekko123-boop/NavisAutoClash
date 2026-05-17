using System.Collections.Generic;
using NavisAutoClash.Core.Domain.Models;

namespace NavisAutoClash.Core.Application.Contracts
{
    /// <summary>
    /// Creates and optionally runs clash tests in the active Navisworks document.
    /// </summary>
    public interface IClashService
    {
        /// <summary>
        /// Creates clash tests for each definition in <paramref name="definitions"/>.
        /// Tests are added to the document's Clash Detective but NOT run automatically.
        /// </summary>
        /// <param name="definitions">The list of tests to create. Must not be null or empty.</param>
        /// <returns>
        /// The display names of all tests that were successfully added.
        /// Any skipped tests (e.g. due to duplicate names) are excluded from the result.
        /// </returns>
        IReadOnlyList<string> CreateClashTests(IEnumerable<ClashTestDefinition> definitions);

        /// <summary>
        /// Runs a specific set of clash tests by name.
        /// Tests must already exist in the document.
        /// </summary>
        /// <param name="testNames">Names of the tests to run.</param>
        void RunClashTests(IEnumerable<string> testNames);
    }
}
