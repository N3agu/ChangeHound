using System.Diagnostics;
using System.IO;
using System.Management;
using ChangeHound.Helpers;
using LibreHardwareMonitor.Hardware;
using Computer = LibreHardwareMonitor.Hardware.Computer;

namespace ChangeHound.Services {
    public class ResourceMonitorService : IResourceMonitorService {
        #region Fields
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;
        private Computer _computer;

        private IHardware? _gpu;
        private IHardware? _cpu;
        private List<IHardware>? _networks;
        #endregion

        #region Constructor & Lifecycle
        public ResourceMonitorService() { }

        public void Dispose() {
            _cpuCounter?.Dispose();
            _ramCounter?.Dispose();
            _computer?.Close();
        }
        #endregion

        #region Private Methods
        public async Task InitializeAsync() {
            await Task.Run(() => {
                _cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                // initialize LibreHardwareMonitor
                _computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsNetworkEnabled = true
                };
                _computer.Open();
                _computer.Accept(new UpdateVisitor());

                _gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel);
                _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
                _networks = _computer.Hardware.Where(h => h.HardwareType == HardwareType.Network).ToList();

                // first read can be faulty
                _cpuCounter.NextValue();
            });
        }

        public List<NetworkInfo> GetNetworkInfo() {
            List<NetworkInfo>? networkInfoList = new List<NetworkInfo>();

            if (_networks != null)
            {
                foreach (IHardware? network in _networks) {
                    network.Update();

                    ISensor? uploadSpeedSensor = network.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Throughput && s.Name == "Upload Speed");
                    ISensor? downloadSpeedSensor = network.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Throughput && s.Name == "Download Speed");
                    ISensor? totalUploadedSensor = network.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Data Uploaded");
                    ISensor? totalDownloadedSensor = network.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Data && s.Name == "Data Downloaded");

                    double uploadSpeed = (uploadSpeedSensor?.Value ?? 0) * 8 / (1000 * 1000); // bps to mbps
                    double downloadSpeed = (downloadSpeedSensor?.Value ?? 0) * 8 / (1000 * 1000); // bps to mbps
                    double totalSent = totalUploadedSensor?.Value ?? 0; // gb
                    double totalReceived = totalDownloadedSensor?.Value ?? 0; // gb

                    networkInfoList.Add(new NetworkInfo(network.Name, uploadSpeed, downloadSpeed, totalSent, totalReceived));
                }
            }
            return networkInfoList;
        }

        public float GetCpuUsage() => _cpuCounter?.NextValue() ?? 0;
        public float GetMemoryUsage() => _ramCounter?.NextValue() ?? 0;

        public float GetGpuUsage() {
            _gpu?.Update();
            // find GPU load sensor
            ISensor? gpuLoadSensor = _gpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Load && s.Name.Contains("D3D"));
            return gpuLoadSensor?.Value ?? 0;
        }

        public float GetPowerUsage() {
            _cpu?.Update();
            // find CPU package power sensor
            ISensor? powerSensor = _cpu?.Sensors.FirstOrDefault(s => s.SensorType == SensorType.Power && s.Name == "CPU Package");
            return powerSensor?.Value ?? 0;
        }

        public List<DiskInfo> GetDiskInfo() {
            List<DiskInfo> diskInfoList = new List<DiskInfo>();
            foreach (DriveInfo drive in DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == DriveType.Fixed)) {
                double totalSpace = drive.TotalSize / (1024.0 * 1024 * 1024);
                double freeSpace = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                double usedSpace = totalSpace - freeSpace;
                double percentUsed = (usedSpace / totalSpace) * 100;
                diskInfoList.Add(new DiskInfo(drive.Name, totalSpace, usedSpace, freeSpace, percentUsed));
            }
            return diskInfoList;
        }

        public SystemInfo GetSystemInfo() {
            string cpuName = "N/A", gpuName = "N/A", formFactor = "N/A";
            double totalRam = 0;

            using (ManagementObjectSearcher? searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                cpuName = searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["Name"]?.ToString() ?? "N/A";

            using (ManagementObjectSearcher? searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
                gpuName = searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["Name"]?.ToString() ?? "N/A";

            using (ManagementObjectSearcher? searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
                totalRam = Math.Round(Convert.ToDouble(searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["TotalPhysicalMemory"]) / (1024 * 1024 * 1024), 2);

            using (ManagementObjectSearcher? searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure")) {
                ushort[]? chassisTypes = searcher.Get().Cast<ManagementObject>().FirstOrDefault()?["ChassisTypes"] as ushort[];
                if (chassisTypes != null && chassisTypes.Any(t => t == 3 || t == 4 || t == 5 || t == 6 || t == 7)) formFactor = "Desktop";
                else if (chassisTypes != null && chassisTypes.Any(t => t == 8 || t == 9 || t == 10 || t == 11 || t == 12 || t == 14)) formFactor = "Laptop";
                else formFactor = "Unknown";
            }

            return new SystemInfo(cpuName, gpuName, totalRam, formFactor);
        }
        #endregion
    }
}
