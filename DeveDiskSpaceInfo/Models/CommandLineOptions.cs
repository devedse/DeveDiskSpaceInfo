namespace DeveDiskSpaceInfo.Models
{
    public class CommandLineOptions
    {
        public string DevicePath { get; set; } = "/dev/iscsi_thick_vg/iscsi_devedse";
        public bool JsonOutput { get; set; }
    }
}