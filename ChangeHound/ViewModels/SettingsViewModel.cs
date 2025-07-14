using ChangeHound.Commands;
using ChangeHound.Common;
using ChangeHound.Services;
using System.Windows.Input;

namespace ChangeHound.ViewModels {
    public class SettingsViewModel : ViewModelBase {
        #region Fields
        private readonly IConfigurationService _configService;
        public string? MonitorPath => _configService.MonitorPath;
        #endregion

        #region Properties
        public ICommand SelectPathCommand { get; }
        public bool MinimizeToTray {
            get => _configService.MinimizeToTray;
            set => _configService.MinimizeToTray = value;
        }

        public bool AllowNotifications {
            get => _configService.AllowNotifications;
            set => _configService.AllowNotifications = value;
        }
        #endregion

        #region Constructor
        public SettingsViewModel(IConfigurationService configService) {
            _configService = configService;
            SelectPathCommand = new DelegateCommand(SelectPath);

            _configService.PropertyChanged += (s, e) => {
                OnPropertyChanged(nameof(MonitorPath));
                OnPropertyChanged(nameof(MinimizeToTray));
                OnPropertyChanged(nameof(AllowNotifications));
            };
        }
        #endregion

        #region Private Methods
        private void SelectPath(object? parameter) {
            FolderBrowserDialog dialog = new FolderBrowserDialog {
                ShowHiddenFiles = true,
                SelectedPath = _configService.MonitorPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == DialogResult.OK) {
                _configService.MonitorPath = dialog.SelectedPath;
            }
        }
        #endregion
    }
}
