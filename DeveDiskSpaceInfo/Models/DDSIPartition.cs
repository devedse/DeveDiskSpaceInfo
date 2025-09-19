using DiscUtils;

namespace DeveDiskSpaceInfo.Models
{
    public class DDSIPartition
    {
        public byte BiosType { get; set; }
        public long FirstSector { get; set; }
        public long LastSector { get; set; }
        public long SectorCount { get; set; }
        public Guid GuidType { get; set; }
        public string TypeAsString { get; set; } = string.Empty;
        public PhysicalVolumeType VolumeType { get; set; }
        public int SectorSize { get; internal set; }
        public List<DDSIFileSystem> FileSystems { get; set; } = new();

        public long Size => FileSystems.Sum(fs => fs.Size);
        public long UsedSpace => FileSystems.Sum(fs => fs.UsedSpace);
        public long AvailableSpace => FileSystems.Sum(fs => fs.AvailableSpace);
    }
}