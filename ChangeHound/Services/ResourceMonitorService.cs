using ChangeHound.Helpers;
using LibreHardwareMonitor.Hardware;
using System.Diagnostics;
using System.IO;
using System.Management;

namespace ChangeHound.Services {
    public class ResourceMonitorService : IResourceMonitorService {
        #region Fields
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _ramCounter;
        private readonly Computer _computer;
        private readonly IHardware? _gpu;
        private readonly IHardware? _cpu;
        #endregion

        #region Constructor & Lifecycle
        public ResourceMonitorService() {
            _cpuCounter = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");
            _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

            // Initialize LibreHardwareMonitor
            _computer = new Computer { IsCpuEnabled = true, IsGpuEnabled = true };
            _computer.Open();
            _computer.Accept(new UpdateVisitor());

            _gpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuIntel);
            _cpu = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

            // first read can be faulty
            _cpuCounter.NextValue();
        }

        public void Dispose() {
            _cpuCounter.Dispose();
            _ramCounter.Dispose();
            _computer.Close();
        }
        #endregion

        #region Private Methods
        public float GetCpuUsage() => _cpuCounter.NextValue();
        public float GetMemoryUsage() => _ramCounter.NextValue();

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
