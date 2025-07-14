using System.ComponentModel;

namespace ChangeHound.Services {
    public interface IConfigurationService : INotifyPropertyChanged {
        string? MonitorPath { get; set; }
        bool MinimizeToTray { get; set; }
    }
}
