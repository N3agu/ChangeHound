using ChangeHound.Commands;
using ChangeHound.Common;
using ChangeHound.Models;
using ChangeHound.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace ChangeHound.ViewModels {
    class MainViewModel : ViewModelBase {
        #region Fields
        private readonly FileSystemMonitorService _monitorService;
        public ObservableCollection<FileChange> FileChanges { get; } = new ObservableCollection<FileChange>();
        public ICollectionView FilteredFileChanges { get; }
        #endregion

        #region Properties
        // Properties for Filter
        private string _searchText = string.Empty;
        public string SearchText {
            get => _searchText;
            set {
                _searchText = value;
                // notify the ui and reapply the filter
                OnPropertyChanged();
                FilteredFileChanges.Refresh();
            }
        }

        private string _selectedFilterType;
        public string SelectedFilterType {
            get => _selectedFilterType;
            set {
                _selectedFilterType = value;
                OnPropertyChanged();
                // reapply the filter
                FilteredFileChanges.Refresh();
            }
        }

        // Properties for Export
        public List<string> EventTypes { get; }
        public string SelectedEventType { get; set; }
        public ICommand ExportCommand { get; }
        #endregion

        #region Constructor
        public MainViewModel() {
            _monitorService = new FileSystemMonitorService(FileChanges);
            _monitorService.StartMonitoring(@"C:\Test");

            // Initialize Export properties
            EventTypes = new List<string> { "All Events", "Created", "Modified", "Deleted" };
            SelectedEventType = EventTypes.First(); // Default value: "All Events"
            ExportCommand = new DelegateCommand(ExportToFile);

            // Initialize Filter properties
            _selectedFilterType = EventTypes.First();
            FilteredFileChanges = CollectionViewSource.GetDefaultView(FileChanges);
            FilteredFileChanges.Filter = ApplyFilter;
        }
        #endregion

        #region Private Methods
        private bool ApplyFilter(object item) {
            if (item is not FileChange change) {
                return false;
            }

            bool typeMatch = SelectedFilterType == "All Events" || change.EventType == SelectedFilterType;

            bool textMatch = string.IsNullOrWhiteSpace(SearchText) ||
                             change.FilePath.Contains(SearchText, StringComparison.OrdinalIgnoreCase);

            return typeMatch && textMatch;
        }

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
