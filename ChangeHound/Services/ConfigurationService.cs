using ChangeHound.Common;

namespace ChangeHound.Services {
    public class ConfigurationService : ViewModelBase, IConfigurationService {
        #region Properties
        private string? _monitorPath;
        public string? MonitorPath {
            get => _monitorPath;
            set => SetProperty(ref _monitorPath, value);
        }

        private bool _minimizeToTray;
        public bool MinimizeToTray {
            get => _minimizeToTray;
            set => SetProperty(ref _minimizeToTray, value);
        }

        private bool _allowNotifications;
        public bool AllowNotifications {
            get => _allowNotifications;
            set => SetProperty(ref _allowNotifications, value);
        }
        #endregion

        #region Constructor
        public ConfigurationService() {
            // default values
            _monitorPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            MinimizeToTray = false;
            AllowNotifications = true;
        }
        #endregion
    }
}
