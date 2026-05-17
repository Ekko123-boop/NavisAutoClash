using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using NavisAutoClash.Core.Application.Contracts;
using NavisAutoClash.Core.Application.UseCases;
using NavisAutoClash.Core.Domain.Models;
using NavisAutoClash.UI.Infrastructure;

namespace NavisAutoClash.UI.ViewModels
{
    /// <summary>
    /// Root ViewModel — orchestrates the entire user workflow:
    /// load data → configure rules → preview → generate (→ optional run).
    /// Depends only on Core contracts; never references Navisworks directly.
    /// </summary>
    public sealed class MainViewModel : ViewModelBase
    {
        // ── dependencies ───────────────────────────────────────────────────────
        private readonly ISelectionSetRepository _selectionSetRepo;
        private readonly IModelRepository _modelRepo;
        private readonly GenerateClashTestsUseCase _generateUseCase;
        private readonly IClashService _clashService;
        private readonly IAppLogger _logger;

        // ── state ──────────────────────────────────────────────────────────────
        private string _statusMessage = "Ready. Click 'Refresh' to load data from the active document.";
        private string _searchTextA = string.Empty;
        private string _searchTextB = string.Empty;
        private bool _isBusy;
        private bool _runAfterGenerate;

        // ── collections ────────────────────────────────────────────────────────
        public ObservableCollection<SelectionSetRowViewModel> SelectionA { get; } = new();
        public ObservableCollection<NwcModelRowViewModel> SelectionB { get; } = new();
        public ObservableCollection<string> PreviewItems { get; } = new();

        // ── child VMs ──────────────────────────────────────────────────────────
        public ClashRuleViewModel ClashRule { get; } = new ClashRuleViewModel();

        // ── properties ─────────────────────────────────────────────────────────
        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string SearchTextA
        {
            get => _searchTextA;
            set { SetProperty(ref _searchTextA, value); RefreshFilteredA(); }
        }

        public string SearchTextB
        {
            get => _searchTextB;
            set { SetProperty(ref _searchTextB, value); RefreshFilteredB(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set { SetProperty(ref _isBusy, value); RefreshCommands(); }
        }

        /// <summary>When true, tests will be run immediately after generation.</summary>
        public bool RunAfterGenerate
        {
            get => _runAfterGenerate;
            set => SetProperty(ref _runAfterGenerate, value);
        }

        // ── filtered views (simple property-based filtering) ───────────────────
        public IEnumerable<SelectionSetRowViewModel> FilteredA =>
            string.IsNullOrWhiteSpace(SearchTextA)
                ? SelectionA
                : SelectionA.Where(s => s.DisplayName.IndexOf(SearchTextA, StringComparison.OrdinalIgnoreCase) >= 0);

        public IEnumerable<NwcModelRowViewModel> FilteredB =>
            string.IsNullOrWhiteSpace(SearchTextB)
                ? SelectionB
                : SelectionB.Where(m => m.DisplayName.IndexOf(SearchTextB, StringComparison.OrdinalIgnoreCase) >= 0);

        // ── commands ───────────────────────────────────────────────────────────
        public ICommand RefreshCommand { get; }
        public ICommand PreviewCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand SelectAllACommand { get; }
        public ICommand ClearAllACommand { get; }
        public ICommand SelectAllBCommand { get; }
        public ICommand ClearAllBCommand { get; }

        // ── constructor ────────────────────────────────────────────────────────
        public MainViewModel(
            ISelectionSetRepository selectionSetRepo,
            IModelRepository modelRepo,
            GenerateClashTestsUseCase generateUseCase,
            IClashService clashService,
            IAppLogger logger)
        {
            _selectionSetRepo = selectionSetRepo ?? throw new ArgumentNullException(nameof(selectionSetRepo));
            _modelRepo = modelRepo ?? throw new ArgumentNullException(nameof(modelRepo));
            _generateUseCase = generateUseCase ?? throw new ArgumentNullException(nameof(generateUseCase));
            _clashService = clashService ?? throw new ArgumentNullException(nameof(clashService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            RefreshCommand = new RelayCommand(ExecuteRefresh, () => !IsBusy);
            PreviewCommand = new RelayCommand(ExecutePreview, CanGenerate);
            GenerateCommand = new RelayCommand(ExecuteGenerate, CanGenerate);
            SelectAllACommand = new RelayCommand(() => SetAllA(true));
            ClearAllACommand = new RelayCommand(() => SetAllA(false));
            SelectAllBCommand = new RelayCommand(() => SetAllB(true));
            ClearAllBCommand = new RelayCommand(() => SetAllB(false));
        }

        // ── command implementations ────────────────────────────────────────────

        private void ExecuteRefresh()
        {
            IsBusy = true;
            StatusMessage = "Loading data from Navisworks…";
            PreviewItems.Clear();

            try
            {
                // Load Selection A
                var sets = _selectionSetRepo.GetAllSelectionSets();
                SelectionA.Clear();
                foreach (var s in sets)
                    SelectionA.Add(new SelectionSetRowViewModel(s));

                // Load Selection B
                var models = _modelRepo.GetAppendedNwcModels();
                SelectionB.Clear();
                foreach (var m in models)
                    SelectionB.Add(new NwcModelRowViewModel(m));

                OnPropertyChanged(nameof(FilteredA));
                OnPropertyChanged(nameof(FilteredB));

                StatusMessage = $"Loaded {SelectionA.Count} selection set(s) and {SelectionB.Count} NWC model(s).";
                _logger.Info(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading data: {ex.Message}";
                _logger.Error("ExecuteRefresh failed.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecutePreview()
        {
            PreviewItems.Clear();
            var config = ClashRule.BuildConfig();
            if (config == null) { StatusMessage = ClashRule.ValidationError ?? "Invalid configuration."; return; }

            var selectedA = GetSelectedA();
            var selectedB = GetSelectedB();

            var definitions = _generateUseCase.Preview(selectedA, selectedB, config);
            foreach (var def in definitions)
                PreviewItems.Add(def.TestName);

            StatusMessage = $"Preview: {PreviewItems.Count} test(s) will be created.";
        }

        private void ExecuteGenerate()
        {
            var config = ClashRule.BuildConfig();
            if (config == null) { StatusMessage = ClashRule.ValidationError ?? "Invalid configuration."; return; }

            var selectedA = GetSelectedA();
            var selectedB = GetSelectedB();

            IsBusy = true;
            StatusMessage = "Creating clash tests…";

            try
            {
                var created = _generateUseCase.Execute(selectedA, selectedB, config);

                if (RunAfterGenerate && created.Count > 0)
                {
                    StatusMessage = $"Running {created.Count} test(s)…";
                    _clashService.RunClashTests(created);
                    StatusMessage = $"Done — {created.Count} test(s) created and run.";
                }
                else
                {
                    StatusMessage = $"Done — {created.Count} test(s) created.";
                }

                _logger.Info(StatusMessage);
            }
            catch (InvalidOperationException ex)
            {
                StatusMessage = ex.Message;
                _logger.Warn(ex.Message);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _logger.Error("ExecuteGenerate failed.", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanGenerate() =>
            !IsBusy &&
            SelectionA.Any(s => s.IsSelected) &&
            SelectionB.Any(m => m.IsSelected);

        // ── helpers ────────────────────────────────────────────────────────────

        private void RefreshCommands() =>
            System.Windows.Application.Current?.Dispatcher.Invoke(
                System.Windows.Input.CommandManager.InvalidateRequerySuggested);

        private void RefreshFilteredA() => OnPropertyChanged(nameof(FilteredA));
        private void RefreshFilteredB() => OnPropertyChanged(nameof(FilteredB));

        private void SetAllA(bool selected)
        {
            foreach (var row in SelectionA) row.IsSelected = selected;
            RefreshCommands();
        }

        private void SetAllB(bool selected)
        {
            foreach (var row in SelectionB) row.IsSelected = selected;
            RefreshCommands();
        }

        private IReadOnlyList<SelectionSetInfo> GetSelectedA() =>
            SelectionA.Where(s => s.IsSelected).Select(s => s.Model).ToList();

        private IReadOnlyList<NwcModelInfo> GetSelectedB() =>
            SelectionB.Where(m => m.IsSelected).Select(m => m.Model).ToList();
    }
}
