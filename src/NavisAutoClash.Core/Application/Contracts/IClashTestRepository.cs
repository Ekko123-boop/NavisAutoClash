using System.Collections.Generic;
using System.Threading.Tasks;

namespace NavisAutoClash.Core.Application.Contracts
{
    public interface IClashTestRepository
    {
        Task<IEnumerable<string>> GetAvailableModelsAsync();
        Task CreateClashTestAsync(string testName, string selectionSetAName, string selectionSetBName);
    }
}
