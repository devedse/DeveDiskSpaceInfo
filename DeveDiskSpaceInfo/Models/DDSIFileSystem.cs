namespace DeveDiskSpaceInfo.Models
{
    public class DDSIFileSystem
    {
        public uint VolumeId { get; set; }
        public string VolumeLabel { get; set; } = string.Empty;
        public string FriendlyName { get; set; } = string.Empty;
        public long Size { get; set; }
        public long UsedSpace { get; set; }
        public long AvailableSpace { get; set; }
        public bool CanWrite { get; set; }
        public string RootPath { get; set; } = string.Empty;
    }
}