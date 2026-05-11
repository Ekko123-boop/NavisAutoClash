using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autodesk.Navisworks.Api;
using NavisAutoClash.Core.Application.Contracts;

namespace NavisAutoClash.Infrastructure.Repositories
{
    public class NavisClashTestRepository : IClashTestRepository
    {
        private readonly INavisDispatcher _dispatcher;

        public NavisClashTestRepository(INavisDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public Task<IEnumerable<string>> GetAvailableModelsAsync()
        {
            return _dispatcher.InvokeAsync(() =>
            {
                var doc = Application.ActiveDocument;
                if (doc == null) return Enumerable.Empty<string>();

                return doc.Models
                    .Select(m => m.FileName)
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList()
                    .AsEnumerable();
            });
        }

        public Task CreateClashTestAsync(string testName, string selectionSetAName, string selectionSetBName)
        {
            return _dispatcher.InvokeAsync(() =>
            {
                // TODO: Implementation for clash test creation
            });
        }
    }
}
