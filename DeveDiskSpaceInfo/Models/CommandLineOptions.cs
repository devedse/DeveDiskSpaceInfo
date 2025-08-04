namespace DeveDiskSpaceInfo.Models
{
    public class CommandLineOptions
    {
        public string DevicePath { get; set; } = string.Empty;
        public bool JsonOutput { get; set; }
    }
}