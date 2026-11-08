using CommandLine;
using System.Collections.Generic;
using System.Linq;

namespace DeveDiskSpaceInfo.Models
{
    public class ShowOptions
    {
        [Value(0, MetaName = "devices", Required = true, HelpText = "Paths to the devices to analyze (e.g., /dev/sdb /dev/sdc).")]
        public IEnumerable<string> DevicePaths { get; set; } = Enumerable.Empty<string>();

        [Option('j', "json", Required = false, HelpText = "Output results in JSON format for programmatic consumption.")]
        public bool JsonOutput { get; set; }

        [Option('p', "parallel", Required = false, Default = 8, HelpText = "How many devices to analyze at the same time (results keep the order of the devices).")]
        public int Parallelism { get; set; } = 8;
    }
}