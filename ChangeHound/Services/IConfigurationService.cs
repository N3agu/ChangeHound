using System.ComponentModel;

namespace ChangeHound.Services {
    public interface IConfigurationService : INotifyPropertyChanged {
        string? MonitorPath { get; set; }
        bool MinimizeToTray { get; set; }
        bool FileNotificationsEnabled { get; set; }
        bool ProcessNotificationsEnabled { get; set; }
        bool RegistryNotificationsEnabled { get; set; }
    }
}
