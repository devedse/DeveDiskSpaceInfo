using System.Text.Json;
using System.Text.Json.Serialization;
using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Services
{
    // JSON Source Generation Context for trimming compatibility
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
    [JsonSerializable(typeof(SfdiskResult))]
    [JsonSerializable(typeof(PartitionTable))]
    [JsonSerializable(typeof(PartitionInfo))]
    internal partial class SfdiskJsonContext : JsonSerializerContext
    {
    }
}
