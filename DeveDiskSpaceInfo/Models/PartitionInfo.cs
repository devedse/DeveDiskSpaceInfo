using System.Text.Json.Serialization;

namespace DeveDiskSpaceInfo.Models
{
    public class PartitionInfo
    {
        [JsonPropertyName("node")]
        public string Node { get; set; } = string.Empty;
        
        [JsonPropertyName("start")]
        public long Start { get; set; }
        
        [JsonPropertyName("size")]
        public long Size { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; } = string.Empty;
        
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("attrs")]
        public string? Attrs { get; set; }
        
        public bool IsNtfs => Type.Equals("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", StringComparison.OrdinalIgnoreCase);
        public long SizeInBytes => Size * 512; // sfdisk returns size in sectors
        public long StartOffsetBytes => Start * 512; // sfdisk returns start in sectors
    }
}
