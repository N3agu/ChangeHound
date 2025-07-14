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

        private bool _fileNotificationsEnabled;
        public bool FileNotificationsEnabled {
            get => _fileNotificationsEnabled;
            set => SetProperty(ref _fileNotificationsEnabled, value);
        }

        private bool _processNotificationsEnabled;
        public bool ProcessNotificationsEnabled {
            get => _processNotificationsEnabled;
            set => SetProperty(ref _processNotificationsEnabled, value);
        }

        private bool _registryNotificationsEnabled;
        public bool RegistryNotificationsEnabled {
            get => _registryNotificationsEnabled;
            set => SetProperty(ref _registryNotificationsEnabled, value);
        }

        #endregion

        #region Constructor
        public ConfigurationService() {
            // default values
            _monitorPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            MinimizeToTray = false;
            FileNotificationsEnabled = false;
            ProcessNotificationsEnabled = false;
            RegistryNotificationsEnabled = false;
        }
        #endregion
    }
}
