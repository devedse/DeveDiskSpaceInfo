using DiscUtils;

namespace DeveDiskSpaceInfo.Models
{
    public class DDSIDisk
    {
        public Geometry BiosGeometry { get; set; }
        public int BlockSize { get; set; }
        public bool CanWrite { get; set; }
        public long Capacity { get; set; }
        public VirtualDiskClass DiskClass { get; set; }
        public VirtualDiskTypeInfo? DiskTypeInfo { get; set; }
        public Geometry? Geometry { get; set; }
        public bool IsPartitioned { get; set; }
        public int SectorSize { get; set; }
        public int Signature { get; set; }
        public List<DDSIPartition> Partitions { get; set; } = new();
        public string DevicePath { get; set; } = string.Empty;
    }
}