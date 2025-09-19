namespace DeveDiskSpaceInfo.Models
{
    public class DDSIGeometry
    {
        public int BytesPerSector { get; set; }
        public long Capacity { get; set; }
        public int Cylinders { get; set; }
        public int HeadsPerCylinder { get; set; }
        public bool IsBiosAndIdeSafe { get; set; }
        public bool IsBiosSafe { get; set; }
        public bool IsIdeSafe { get; set; }
        public int SectorsPerTrack { get; set; }
        public long TotalSectorsLong { get; set; }
    }
}