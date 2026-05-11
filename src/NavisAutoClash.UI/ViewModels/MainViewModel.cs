using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Application.UseCases;

namespace NavisAutoClash.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IClashTestRepository _repository;
        private readonly INavisDispatcher _dispatcher;
        private readonly RunClashTestsUseCase _runClashTestsUseCase;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        public ObservableCollection<NwcModelRowViewModel> Models { get; } = new();
        
        public ICollectionView FilteredModels { get; }

        public MainViewModel(IClashTestRepository repository, INavisDispatcher dispatcher, RunClashTestsUseCase runClashTestsUseCase)
        {
            _repository = repository;
            _dispatcher = dispatcher;
            _runClashTestsUseCase = runClashTestsUseCase;

            FilteredModels = CollectionViewSource.GetDefaultView(Models);
            FilteredModels.Filter = FilterModels;

            LoadModelsCommand = new AsyncRelayCommand(LoadModelsAsync);
            RunCommand = new AsyncRelayCommand(RunClashTestsAsync);
        }

        public IAsyncRelayCommand LoadModelsCommand { get; }
        public IAsyncRelayCommand RunCommand { get; }

        partial void OnSearchTextChanged(string value)
        {
            FilteredModels.Refresh();
        }

        private bool FilterModels(object obj)
        {
            if (obj is not NwcModelRowViewModel model) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;

            return model.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase);
        }

        private async Task LoadModelsAsync()
        {
            StatusMessage = "Loading models...";
            try
            {
                var models = await _repository.GetAvailableModelsAsync();
                await _dispatcher.InvokeAsync(() =>
                {
                    Models.Clear();
                    foreach (var model in models)
                    {
                        Models.Add(new NwcModelRowViewModel(model));
                    }
                });
                StatusMessage = $"Loaded {Models.Count} models";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task RunClashTestsAsync()
        {
            var selected = Models.Where(m => m.IsSelected).ToList();
            if (!selected.Any())
            {
                StatusMessage = "No models selected";
                return;
            }

            StatusMessage = $"Running tests for {selected.Count} models...";
            try
            {
                await _runClashTestsUseCase.ExecuteAsync(selected.Select(m => m.Name));
                StatusMessage = "Successfully created clash tests!";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
}
