namespace DeveDiskSpaceInfo.Models
{
    public class AnalysisResult
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public PartitionTable? PartitionTable { get; set; }
        public List<NtfsAnalysisResult>? NtfsAnalysisResults { get; set; }
    }

    public class NtfsAnalysisResult
    {
        public PartitionInfo Partition { get; set; } = new();
        public bool Success { get; set; }
        public string? Error { get; set; }
        public JsonFileSystemInfo? FileSystemInfo { get; set; }
    }
}
