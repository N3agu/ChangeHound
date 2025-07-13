using System.ComponentModel;

namespace ChangeHound.Services {
    public interface IFileSystemMonitorService {
        void StartMonitoring(string path);
        void StopMonitoring();
    }
}
