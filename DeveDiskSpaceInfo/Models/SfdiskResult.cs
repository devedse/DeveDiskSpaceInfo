using System.Text.Json.Serialization;

namespace DeveDiskSpaceInfo.Models
{
    public class SfdiskResult
    {
        [JsonPropertyName("partitiontable")]
        public PartitionTable PartitionTable { get; set; } = new();
    }
}
