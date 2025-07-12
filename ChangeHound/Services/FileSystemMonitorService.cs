using ChangeHound.Models;
using System.Collections.ObjectModel;
using System.IO;

namespace ChangeHound.Services {
    class FileSystemMonitorService {
        private readonly FileSystemWatcher _watcher;
        private readonly ObservableCollection<FileChange> _fileChanges;

        public FileSystemMonitorService(ObservableCollection<FileChange> fileChanges) {
            _fileChanges = fileChanges;
            _watcher = new FileSystemWatcher();
        }


    }
}
