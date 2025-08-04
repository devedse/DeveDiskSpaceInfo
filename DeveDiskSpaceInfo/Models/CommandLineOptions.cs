using CommandLine;

namespace DeveDiskSpaceInfo.Models
{
    public class ShowOptions
    {
        [Value(0, MetaName = "device", Required = true, HelpText = "Path to the device to analyze (e.g., /dev/sdb).")]
        public string DevicePath { get; set; } = string.Empty;

        [Option('j', "json", Required = false, HelpText = "Output results in JSON format for programmatic consumption.")]
        public bool JsonOutput { get; set; }
    }
}