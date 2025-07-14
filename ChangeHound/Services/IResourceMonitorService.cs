namespace ChangeHound.Services {
    public interface IResourceMonitorService : IDisposable {
        float GetCpuUsage();
        float GetMemoryUsage();
        float GetGpuUsage();
        float GetPowerUsage();
        List<DiskInfo> GetDiskInfo();
        SystemInfo GetSystemInfo();
    }

    public record DiskInfo(string Name, double TotalSpaceGB, double UsedSpaceGB, double FreeSpaceGB, double PercentUsed);
    public record SystemInfo(string CpuName, string GpuName, double TotalRamGB, string FormFactor);
}
