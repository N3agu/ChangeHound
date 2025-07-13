using ChangeHound.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace ChangeHound.Services {
    class FileSystemMonitorService : IFileSystemMonitorService {
        private readonly FileSystemWatcher _watcher;
        private readonly ObservableCollection<FileChange> _fileChanges;

        public FileSystemMonitorService(ObservableCollection<FileChange> fileChanges) {
            _fileChanges = fileChanges;
            _watcher = new FileSystemWatcher();
        }

        public void StartMonitoring(string path) {
            _watcher.Path = path;
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _watcher.IncludeSubdirectories = true;

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;

            _watcher.EnableRaisingEvents = true;
        }

        public void StopMonitoring() {
            _watcher.EnableRaisingEvents = false;
        }

        private void OnChanged(object sender, FileSystemEventArgs e) {
            App.Current.Dispatcher.Invoke(() => {
                _fileChanges.Add(new FileChange { EventType = "Modified", FilePath = e.FullPath, Timestamp = DateTime.Now });
            });
        }

        private void OnCreated(object sender, FileSystemEventArgs e) {
            App.Current.Dispatcher.Invoke(() => {
                _fileChanges.Add(new FileChange { EventType = "Created", FilePath = e.FullPath, Timestamp = DateTime.Now });
            });
        }

        private void OnDeleted(object sender, FileSystemEventArgs e) {
            App.Current.Dispatcher.Invoke(() => {
                _fileChanges.Add(new FileChange { EventType = "Deleted", FilePath = e.FullPath, Timestamp = DateTime.Now });
            });
        }

        private void OnRenamed(object sender, RenamedEventArgs e) {
            App.Current.Dispatcher.Invoke(() => {
                _fileChanges.Add(new FileChange { EventType = "Renamed", FilePath = $"{e.OldFullPath} to {e.FullPath}", Timestamp = DateTime.Now });
            });
        }
    }
}
