using System.Text.Json;
using System.Text.Json.Serialization;
using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Services
{
    // JSON Source Generation Context for trimming compatibility
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
    [JsonSerializable(typeof(SfdiskResult))]
    [JsonSerializable(typeof(PartitionTable))]
    [JsonSerializable(typeof(PartitionInfo))]
    [JsonSerializable(typeof(JsonOutputResult))]
    [JsonSerializable(typeof(JsonPartitionTable))]
    [JsonSerializable(typeof(JsonPartitionInfo))]
    [JsonSerializable(typeof(JsonNtfsAnalysis))]
    [JsonSerializable(typeof(JsonFileSystemInfo))]
    [JsonSerializable(typeof(List<JsonPartitionInfo>))]
    [JsonSerializable(typeof(List<JsonNtfsAnalysis>))]
    public partial class SfdiskJsonContext : JsonSerializerContext
    {
    }
}
