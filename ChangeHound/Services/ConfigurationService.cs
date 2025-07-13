using ChangeHound.Common;

namespace ChangeHound.Services {
    public class ConfigurationService : ViewModelBase, IConfigurationService {
        private string? _monitorPath;
        public string? MonitorPath {
            get => _monitorPath;
            set => SetProperty(ref _monitorPath, value);
        }

        public ConfigurationService() {
            _monitorPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }
    }
}
