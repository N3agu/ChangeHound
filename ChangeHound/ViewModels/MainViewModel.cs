using System.Collections.ObjectModel;
using ChangeHound.Models;

namespace ChangeHound.ViewModels {
    class MainViewModel {
        public ObservableCollection<FileChange> FileChanges { get; } = new ObservableCollection<FileChange>();
    }
}
