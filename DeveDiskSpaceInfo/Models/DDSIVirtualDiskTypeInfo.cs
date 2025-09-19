namespace DeveDiskSpaceInfo.Models
{
    public class DDSIVirtualDiskTypeInfo
    {
        public bool CanBeHardDisk { get; set; }
        public bool DeterministicGeometry { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool PreservesBiosGeometry { get; set; }
        public string Variant { get; set; } = string.Empty;
    }
}