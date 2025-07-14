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
        #endregion

        #region Constructor
        public SettingsViewModel(IConfigurationService configService) {
            _configService = configService;
            SelectPathCommand = new DelegateCommand(SelectPath);

            _configService.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(IConfigurationService.MonitorPath)) {
                    OnPropertyChanged(nameof(MonitorPath));
                    OnPropertyChanged(nameof(MinimizeToTray));
                }
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
