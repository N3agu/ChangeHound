using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ChangeHound.Commands;

namespace ChangeHound.Helpers {
    public class NetworkInfo : INotifyPropertyChanged {
        public string AdapterName { get; }
        public ICommand ToggleVisibilityCommand { get; }

        private bool _isVisible = true;
        public bool IsVisible {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private double _uploadSpeedMbps;
        public double UploadSpeedMbps {
            get => _uploadSpeedMbps;
            private set => SetProperty(ref _uploadSpeedMbps, value);
        }

        private double _downloadSpeedMbps;
        public double DownloadSpeedMbps {
            get => _downloadSpeedMbps;
            private set => SetProperty(ref _downloadSpeedMbps, value);
        }

        private double _totalSentGB;
        public double TotalSentGB {
            get => _totalSentGB;
            private set => SetProperty(ref _totalSentGB, value);
        }

        private double _totalReceivedGB;
        public double TotalReceivedGB {
            get => _totalReceivedGB;
            private set => SetProperty(ref _totalReceivedGB, value);
        }

        public NetworkInfo(string adapterName, double upload, double download, double sent, double received) {
            AdapterName = adapterName;
            ToggleVisibilityCommand = new DelegateCommand(_ => IsVisible = !IsVisible);
            Update(upload, download, sent, received);
        }

        public void Update(NetworkInfo other) {
            Update(other.UploadSpeedMbps, other.DownloadSpeedMbps, other.TotalSentGB, other.TotalReceivedGB);
        }

        private void Update(double upload, double download, double sent, double received) {
            UploadSpeedMbps = upload;
            DownloadSpeedMbps = download;
            TotalSentGB = sent;
            TotalReceivedGB = received;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
}
