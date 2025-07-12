using ChangeHound.Models;
using ChangeHound.Services;
using System.Collections.ObjectModel;

namespace ChangeHound.ViewModels {
    class MainViewModel {
        public ObservableCollection<FileChange> FileChanges { get; } = new ObservableCollection<FileChange>();
        private readonly FileSystemMonitorService _monitorService;

        public MainViewModel() {
            _monitorService = new FileSystemMonitorService(FileChanges);
            _monitorService.StartMonitoring(@"C:\Test");
        }
    }
}
