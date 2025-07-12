namespace ChangeHound.Models {
    public class FileChange {
        public string? EventType { get; set; }
        public string? FilePath { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
