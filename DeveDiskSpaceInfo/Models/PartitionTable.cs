using System.Text.Json.Serialization;

namespace DeveDiskSpaceInfo.Models
{
    public class PartitionTable
    {
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;
        
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        
        [JsonPropertyName("device")]
        public string Device { get; set; } = string.Empty;
        
        [JsonPropertyName("unit")]
        public string Unit { get; set; } = string.Empty;
        
        [JsonPropertyName("firstlba")]
        public long FirstLba { get; set; }
        
        [JsonPropertyName("lastlba")]
        public long LastLba { get; set; }
        
        [JsonPropertyName("sectorsize")]
        public int SectorSize { get; set; }
        
        [JsonPropertyName("partitions")]
        public List<PartitionInfo> Partitions { get; set; } = new();
    }
}
