using ChangeHound.Commands;
using ChangeHound.Models;
using ChangeHound.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
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
            IEnumerable<FileChange> filteredChanges = FileChanges.AsEnumerable();
            if (SelectedEventType != "All Events") {
                filteredChanges = FileChanges.Where(fc => fc.EventType == SelectedEventType);
            }

            if (!filteredChanges.Any()) {
                MessageBox.Show("There are no Events to export!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog {
                Filter = "CSV file (*.csv)|*.csv",
                Title = "Export File Changes",
                FileName = $"ChangeHound_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true) {
                StringBuilder csvString = new StringBuilder();
                csvString.AppendLine("EventType,FilePath,Timestamp"); // header for the csv

                foreach (var change in filteredChanges) {
                    csvString.AppendLine($"{change.EventType},\"{change.FilePath}\",{change.Timestamp:o}"); // Use ISO 8601 for timestamp
                }

                File.WriteAllText(saveFileDialog.FileName, csvString.ToString());
            }
        }
        #endregion
    }
}
