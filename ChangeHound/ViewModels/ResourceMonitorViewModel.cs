using ChangeHound.Common;
using ChangeHound.Services;
using LiveCharts;
using LiveCharts.Wpf;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Threading;
using Brush = System.Windows.Media.Brush;

namespace ChangeHound.ViewModels {
    public class ResourceMonitorViewModel : ViewModelBase {
        #region Fields
        private IResourceMonitorService _performanceService;
        private DispatcherTimer _timer;
        #endregion

        #region Properties
        public SeriesCollection CpuSeries { get; set; }
        public SeriesCollection MemorySeries { get; set; }
        public SeriesCollection GpuSeries { get; set; }
        public float PowerUsage { get; set; }
        public ObservableCollection<DiskInfo> Disks { get; set; }
        public SystemInfo? SystemInfo { get; set; }
        #endregion

        #region Constructor & Lifecycle
        public ResourceMonitorViewModel(IConfigurationService configService) {
            // initialize charts
            CpuSeries = CreateDonutSeries(0);
            MemorySeries = CreateDonutSeries(0);
            GpuSeries = CreateDonutSeries(0);

            // initialize collection
            Disks = new ObservableCollection<DiskInfo>();
        }

        public void Dispose() {
            _timer.Stop();
            _performanceService.Dispose();
        }
        #endregion

        #region Private Methods
        private async Task InitializeAsync() {
            _performanceService = new ResourceMonitorService();
            await _performanceService.InitializeAsync();

            // Run the slow WMI query on a background thread
            SystemInfo = await Task.Run(() => _performanceService.GetSystemInfo());
            OnPropertyChanged(nameof(SystemInfo));

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
            _timer.Tick += UpdatePerformanceData;
            _timer.Start();

            UpdatePerformanceData(null, EventArgs.Empty);
        }

        public static async Task<ResourceMonitorViewModel> CreateAsync(IConfigurationService configService) {
            var viewModel = new ResourceMonitorViewModel(configService);
            await viewModel.InitializeAsync();
            return viewModel;
        }

        private void UpdatePerformanceData(object? sender, EventArgs e) {
            UpdateSeries(CpuSeries, _performanceService.GetCpuUsage());
            UpdateSeries(MemorySeries, _performanceService.GetMemoryUsage());
            UpdateSeries(GpuSeries, _performanceService.GetGpuUsage());

            PowerUsage = _performanceService.GetPowerUsage();
            OnPropertyChanged(nameof(PowerUsage));

            List<DiskInfo> currentDisks = _performanceService.GetDiskInfo();
            Disks.Clear();
            foreach (DiskInfo disk in currentDisks) Disks.Add(disk);
        }

        private SeriesCollection CreateDonutSeries(float initialValue) {
            Brush usedBrush = (Brush)new BrushConverter().ConvertFromString("#707070");
            Brush freeBrush = (Brush)new BrushConverter().ConvertFromString("#b8b8b8");

            return new SeriesCollection {
                new PieSeries { Title = "Used", Values = new ChartValues<double> { initialValue }, DataLabels = false, Fill = usedBrush },
                new PieSeries { Title = "Free", Values = new ChartValues<double> { 100 - initialValue }, DataLabels = false, Fill = freeBrush }
            };
        }

        private void UpdateSeries(SeriesCollection series, float newValue) {
            // ensure value >= 0 && value <= 100
            float cappedValue = Math.Max(0, Math.Min(100, newValue));
            series[0].Values[0] = (double)cappedValue;
            series[1].Values[0] = 100.0 - cappedValue;
        }
        #endregion
    }
}
