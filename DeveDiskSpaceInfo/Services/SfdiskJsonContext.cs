using DeveDiskSpaceInfo.Models;
using DiscUtils;
using System.Text.Json.Serialization;

namespace DeveDiskSpaceInfo.Services
{
    [JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true, WriteIndented = true)]
    [JsonSerializable(typeof(DDSIAnalysisResult))]
    [JsonSerializable(typeof(List<DDSIAnalysisResult>))]
    [JsonSerializable(typeof(DDSIDisk))]
    [JsonSerializable(typeof(DDSIPartition))]
    [JsonSerializable(typeof(DDSIFileSystem))]
    [JsonSerializable(typeof(List<DDSIPartition>))]
    [JsonSerializable(typeof(List<DDSIFileSystem>))]
    [JsonSerializable(typeof(DDSIGeometry))]
    [JsonSerializable(typeof(VirtualDiskClass))]
    [JsonSerializable(typeof(DDSIVirtualDiskTypeInfo))]
    [JsonSerializable(typeof(PhysicalVolumeType))]
    public partial class SfdiskJsonContext : JsonSerializerContext
    {
    }
}
