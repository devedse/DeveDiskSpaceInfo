using DiscUtils;

namespace DeveDiskSpaceInfo.Models
{
    public class DDSIDisk
    {
        public DDSIGeometry BiosGeometry { get; set; } = new();
        public int BlockSize { get; set; }
        public bool CanWrite { get; set; }
        public long Capacity { get; set; }
        public VirtualDiskClass DiskClass { get; set; }
        public DDSIVirtualDiskTypeInfo? DiskTypeInfo { get; set; }
        public DDSIGeometry? Geometry { get; set; }
        public bool IsPartitioned { get; set; }
        public int SectorSize { get; set; }
        public int Signature { get; set; }
        public List<DDSIPartition> Partitions { get; set; } = new();
        public string DevicePath { get; set; } = string.Empty;
    }
}