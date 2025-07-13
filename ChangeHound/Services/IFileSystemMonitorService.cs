namespace ChangeHound.Services {
    public interface IFileSystemMonitorService : IDisposable {
        void StartMonitoring(string path);
        void StopMonitoring();
    }
}
