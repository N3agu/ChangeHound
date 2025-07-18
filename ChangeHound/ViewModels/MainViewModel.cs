﻿using ChangeHound.Commands;
using ChangeHound.Common;
using ChangeHound.Services;
using ModernWpf;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace ChangeHound.ViewModels {
    public class MainViewModel : ViewModelBase {
        #region Fields
        private IConfigurationService _configService;
        private readonly Dictionary<Type, ViewModelBase> _viewModelInstances = new Dictionary<Type, ViewModelBase>();
        public string ToggleThemeIcon =>
            CurrentTheme == ApplicationTheme.Dark ? "\uE706" : "\uE708";

        public string ToggleThemeLabel =>
            CurrentTheme == ApplicationTheme.Dark ? "Light Mode" : "Dark Mode";

        public bool MinimizeToTray => _configService.MinimizeToTray;
        #endregion

        #region Properties
        public bool IsLoading { get; private set; } = true;
        public ObservableCollection<object> NavigationItems { get; private set; } = new();
        private ApplicationTheme _currentTheme;
        public ApplicationTheme CurrentTheme {
            get => _currentTheme;
            set {
                // when CurrentTheme changes - notify the UI to update the Icon and Label
                if (SetProperty(ref _currentTheme, value)) {
                    OnPropertyChanged(nameof(ToggleThemeIcon));
                    OnPropertyChanged(nameof(ToggleThemeLabel));
                }
            }
        }

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel {
            get => _currentViewModel;
            set {
                _currentViewModel = value;
                OnPropertyChanged();
            }
        }

        private object _selectedItem;
        public object SelectedItem {
            get => _selectedItem;
            set {
                if (SetProperty(ref _selectedItem, value)) {
                    Navigate(_selectedItem);
                }
            }
        }

        // Theme Change Command
        public ICommand ToggleThemeCommand { get; private set; } = null!;
        #endregion

        #region Constructor & Lifecycle
        public MainViewModel() { }
        public void Dispose() {
            foreach (ViewModelBase viewModel in _viewModelInstances.Values) {
                if (viewModel is IDisposable disposable) {
                    disposable.Dispose();
                }
            }

            System.Windows.Application.Current.Exit -= OnApplicationExit;
        }
        #endregion

        #region Private Methods
        public static async Task<MainViewModel> CreateAsync() {
            MainViewModel viewModel = new MainViewModel();
            await viewModel.InitializeAsync();
            return viewModel;
        }

        private async Task InitializeAsync() {
            _configService = new ConfigurationService();
            _configService.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(IConfigurationService.MinimizeToTray)) {
                    OnPropertyChanged(nameof(MinimizeToTray));
                }
            };

            _viewModelInstances.Add(typeof(FileMonitorViewModel), new FileMonitorViewModel(_configService));
            _viewModelInstances.Add(typeof(ProcessMonitorViewModel), new ProcessMonitorViewModel(_configService));
            _viewModelInstances.Add(typeof(RegistryMonitorViewModel), new RegistryMonitorViewModel(_configService));
            _viewModelInstances.Add(typeof(SettingsViewModel), new SettingsViewModel(_configService));

            var resourceMonitorVM = await ResourceMonitorViewModel.CreateAsync(_configService);
            _viewModelInstances.Add(typeof(ResourceMonitorViewModel), resourceMonitorVM);

            NavigationItems.Add(new NavigationViewItem { Content = "File Monitor", Icon = new FontIcon { FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uED41" }, Tag = typeof(FileMonitorViewModel) });
            NavigationItems.Add(new NavigationViewItem { Content = "Process Monitor", Icon = new FontIcon { FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE9F5" }, Tag = typeof(ProcessMonitorViewModel) });
            NavigationItems.Add(new NavigationViewItem { Content = "Registry Monitor", Icon = new FontIcon { FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE9F9" }, Tag = typeof(RegistryMonitorViewModel) });
            NavigationItems.Add(new NavigationViewItem { Content = "Resource Monitor", Icon = new FontIcon { FontFamily = new System.Windows.Media.FontFamily("Segoe MDL2 Assets"), Glyph = "\uE9D9" }, Tag = typeof(ResourceMonitorViewModel) });
            NavigationItems.Add(new NavigationViewItem { Content = "Settings", Icon = new SymbolIcon { Symbol = Symbol.Setting }, Tag = typeof(SettingsViewModel) });

            ThemeManager.Current.ApplicationTheme ??= ApplicationTheme.Dark;
            CurrentTheme = ThemeManager.Current.ApplicationTheme.Value;
            ToggleThemeCommand = new DelegateCommand(ToggleTheme);

            CurrentViewModel = _viewModelInstances[typeof(FileMonitorViewModel)];
            SelectedItem = NavigationItems.First();

            System.Windows.Application.Current.Exit += OnApplicationExit;

            IsLoading = false;
            OnPropertyChanged(nameof(IsLoading));
        }

        private void OnApplicationExit(object sender, ExitEventArgs e) {
            Dispose();
        }
        private void Navigate(object selectedItem) {
            if (selectedItem is not NavigationViewItem item) return;

            if (item.Tag is Type viewModelType && _viewModelInstances.ContainsKey(viewModelType)) {
                CurrentViewModel = _viewModelInstances[viewModelType];
            }
        }

        private void ToggleTheme(object? parameter = null) {
            ApplicationTheme newTheme = CurrentTheme == ApplicationTheme.Dark
                ? ApplicationTheme.Light
                : ApplicationTheme.Dark;

            ThemeManager.Current.ApplicationTheme = newTheme;
            CurrentTheme = newTheme;
        }
        #endregion
    }
}
