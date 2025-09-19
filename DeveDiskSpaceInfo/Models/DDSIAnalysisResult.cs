namespace DeveDiskSpaceInfo.Models
{
    public class DDSIAnalysisResult
    {
        public required string DevicePath { get; set; }

        public DDSIDisk? Disk { get; set; }

        public required bool Success { get; set; }
        public string? Error { get; set; }
        public string? Warning { get; set; }
    }
}
