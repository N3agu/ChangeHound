using ChangeHound.Commands;
using ChangeHound.Models;
using ChangeHound.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ChangeHound.ViewModels {
    class MainViewModel {
        #region Fields
        public ObservableCollection<FileChange> FileChanges { get; } = new ObservableCollection<FileChange>();
        private readonly FileSystemMonitorService _monitorService;
        #endregion

        #region Properties
        public List<string> EventTypes { get; }
        public string SelectedEventType { get; set; }
        public ICommand ExportCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel() {
            _monitorService = new FileSystemMonitorService(FileChanges);
            _monitorService.StartMonitoring(@"C:\Test");

            EventTypes = new List<string> { "All Events", "Created", "Modified", "Deleted" };
            SelectedEventType = EventTypes.First(); // Default value: "All Events"
            ExportCommand = new DelegateCommand(ExportToFile);
        }
        #endregion

        #region Private Methods
        private void ExportToFile(object? parameter) {
        
        }
        #endregion
    }
}
