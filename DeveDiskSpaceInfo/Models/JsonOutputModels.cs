using System.Text.Json.Serialization;

namespace DeveDiskSpaceInfo.Models
{
    public class JsonOutputResult
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("device_path")]
        public string? DevicePath { get; set; }

        [JsonPropertyName("partition_table")]
        public JsonPartitionTable? PartitionTable { get; set; }

        [JsonPropertyName("ntfs_analysis")]
        public List<JsonNtfsAnalysis>? NtfsAnalysis { get; set; }
    }

    public class JsonPartitionTable
    {
        [JsonPropertyName("device")]
        public string Device { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("sector_size")]
        public long SectorSize { get; set; }

        [JsonPropertyName("first_lba")]
        public long FirstLba { get; set; }

        [JsonPropertyName("last_lba")]
        public long LastLba { get; set; }

        [JsonPropertyName("total_sectors")]
        public long TotalSectors { get; set; }

        [JsonPropertyName("total_size_bytes")]
        public long TotalSizeBytes { get; set; }

        [JsonPropertyName("partitions")]
        public List<JsonPartitionInfo> Partitions { get; set; } = new();
    }

    public class JsonPartitionInfo
    {
        [JsonPropertyName("node")]
        public string Node { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("size_bytes")]
        public long SizeBytes { get; set; }

        [JsonPropertyName("start")]
        public long Start { get; set; }

        [JsonPropertyName("start_offset_bytes")]
        public long StartOffsetBytes { get; set; }

        [JsonPropertyName("is_ntfs")]
        public bool IsNtfs { get; set; }

        [JsonPropertyName("attrs")]
        public string? Attrs { get; set; }
    }

    public class JsonNtfsAnalysis
    {
        [JsonPropertyName("partition")]
        public JsonPartitionInfo Partition { get; set; } = new();

        [JsonPropertyName("analysis_success")]
        public bool AnalysisSuccess { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("file_system_info")]
        public JsonFileSystemInfo? FileSystemInfo { get; set; }
    }

    public class JsonFileSystemInfo
    {
        [JsonPropertyName("total_clusters")]
        public long TotalClusters { get; set; }

        [JsonPropertyName("free_clusters")]
        public long FreeClusters { get; set; }

        [JsonPropertyName("cluster_size_bytes")]
        public long ClusterSizeBytes { get; set; }

        [JsonPropertyName("total_size_bytes")]
        public long TotalSizeBytes { get; set; }

        [JsonPropertyName("free_size_bytes")]
        public long FreeSizeBytes { get; set; }

        [JsonPropertyName("used_size_bytes")]
        public long UsedSizeBytes { get; set; }

        [JsonPropertyName("used_percentage")]
        public double UsedPercentage { get; set; }
    }
}