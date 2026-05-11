using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NavisAutoClash.Core.Application.Contracts;

namespace NavisAutoClash.Core.Application.UseCases
{
    public class RunClashTestsUseCase
    {
        private readonly IClashTestRepository _repository;

        public RunClashTestsUseCase(IClashTestRepository repository)
        {
            _repository = repository;
        }

        public async Task ExecuteAsync(IEnumerable<string> selectedModelNames)
        {
            const string baseSetName = "Base Build";
            
            foreach (var modelName in selectedModelNames)
            {
                var cleanName = System.IO.Path.GetFileNameWithoutExtension(modelName);
                var testName = $"Base vs {cleanName}";
                
                await _repository.CreateClashTestAsync(testName, baseSetName, cleanName);
            }
        }
    }
}
