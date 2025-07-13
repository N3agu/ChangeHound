using ChangeHound.Commands;
using ChangeHound.Common;
using ModernWpf;
using ModernWpf.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChangeHound.ViewModels {
    public class MainViewModel : ViewModelBase {
        #region Fields
        private readonly Dictionary<Type, ViewModelBase> _viewModelInstances = new Dictionary<Type, ViewModelBase>();
        public ObservableCollection<object> NavigationItems { get; }
        public string ToggleThemeIcon =>
            CurrentTheme == ApplicationTheme.Dark ? "\uE706" : "\uE708";

        public string ToggleThemeLabel =>
            CurrentTheme == ApplicationTheme.Dark ? "Light Mode" : "Dark Mode";
        #endregion

        #region Properties
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
        public ICommand ToggleThemeCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel() {
            _viewModelInstances.Add(typeof(MonitorViewModel), new MonitorViewModel());
            _viewModelInstances.Add(typeof(SettingsViewModel), new SettingsViewModel());

            ThemeManager.Current.ApplicationTheme ??= ApplicationTheme.Dark;
            _currentTheme = ThemeManager.Current.ApplicationTheme.Value;

            NavigationItems = new ObservableCollection<object> {
                new NavigationViewItem {
                    Content = "File Monitor",
                    Icon = new SymbolIcon { Symbol = Symbol.Document },
                    Tag = typeof(MonitorViewModel)
                },
                new NavigationViewItem {
                    Content = "Settings",
                    Icon = new SymbolIcon { Symbol = Symbol.Setting },
                    Tag = typeof(SettingsViewModel)
                }
            };


            ToggleThemeCommand = new DelegateCommand(ToggleTheme);

            CurrentViewModel = _viewModelInstances[typeof(MonitorViewModel)];
            _selectedItem = NavigationItems.First();
        }
        #endregion

        #region Private Methods
        private void Navigate(object selectedItem) {
            if (selectedItem is not NavigationViewItem item) return;

            if (item.Tag is Type viewModelType && _viewModelInstances.ContainsKey(viewModelType)) {
                CurrentViewModel = _viewModelInstances[viewModelType];
            }
        }

        private void ToggleTheme(object? parameter = null) {
            var newTheme = CurrentTheme == ApplicationTheme.Dark
                ? ApplicationTheme.Light
                : ApplicationTheme.Dark;

            ThemeManager.Current.ApplicationTheme = newTheme;
            CurrentTheme = newTheme;
        }
        #endregion
    }
}
