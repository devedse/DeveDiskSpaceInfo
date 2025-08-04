using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;

namespace DeveDiskSpaceInfo
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var options = ParseCommandLineArguments(args);
            
            if (options == null)
            {
                PrintUsage();
                return 1;
            }

            await ExecuteAnalysis(options);
            return 0;
        }

        private static CommandLineOptions? ParseCommandLineArguments(string[] args)
        {
            var options = new CommandLineOptions();
            bool devicePathSet = false;

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                
                switch (arg.ToLowerInvariant())
                {
                    case "--json":
                        options.JsonOutput = true;
                        break;
                    case "--device":
                    case "-d":
                        if (i + 1 >= args.Length)
                        {
                            Console.WriteLine("Error: --device option requires a value");
                            return null;
                        }
                        if (devicePathSet)
                        {
                            Console.WriteLine("Error: Device path specified multiple times");
                            return null;
                        }
                        options.DevicePath = args[++i];
                        devicePathSet = true;
                        break;
                    case "--help":
                    case "-h":
                        return null;
                    default:
                        // Check if this is a positional argument (device path)
                        if (!arg.StartsWith("-"))
                        {
                            if (devicePathSet)
                            {
                                Console.WriteLine("Error: Device path specified multiple times");
                                return null;
                            }
                            options.DevicePath = arg;
                            devicePathSet = true;
                        }
                        else
                        {
                            Console.WriteLine($"Error: Unknown option '{arg}'");
                            return null;
                        }
                        break;
                }
            }

            // Check if device path was provided
            if (string.IsNullOrEmpty(options.DevicePath))
            {
                Console.WriteLine("Error: Device path is required");
                return null;
            }

            return options;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("DeveDiskSpaceInfo - Analyze disk space usage on Linux devices without mounting");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DeveDiskSpaceInfo <device> [options]");
            Console.WriteLine();
            Console.WriteLine("Arguments:");
            Console.WriteLine("  device                 Path to the device to analyze (required)");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --device, -d <path>    Alternative way to specify device path");
            Console.WriteLine("  --json                 Output results in JSON format");
            Console.WriteLine("  --help, -h             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  DeveDiskSpaceInfo /dev/sdb                     # Analyze /dev/sdb");
            Console.WriteLine("  DeveDiskSpaceInfo /dev/sdb --json              # Get JSON output");
            Console.WriteLine("  DeveDiskSpaceInfo --device /dev/sdb --json     # Alternative syntax");
        }

        private static async Task ExecuteAnalysis(CommandLineOptions options)
        {
            var outputService = new OutputService(options);

            try
            {
                // Detect partitions using sfdisk
                var partitionTable = await PartitionDetectorService.DetectPartitionsAsync(options.DevicePath, outputService);
                
                if (partitionTable == null)
                {
                    outputService.ReportPartitionDetectionFailed();
                    outputService.OutputFinalResult();
                    return;
                }

                outputService.ReportPartitionTableDetected(partitionTable);
                
                // Find and mount NTFS partitions
                var ntfsPartitions = partitionTable.Partitions.Where(p => p.IsNtfs).ToList();
                
                if (ntfsPartitions.Any())
                {
                    outputService.ReportNtfsAnalysisStart(ntfsPartitions.Count);
                    
                    foreach (var partition in ntfsPartitions)
                    {
                        NtfsAnalyzerService.AnalyzeNtfsPartition(partition, options.DevicePath, outputService);
                    }
                }
                else
                {
                    outputService.ReportNoNtfsPartitions();
                }

                outputService.OutputFinalResult();
            }
            catch (FileNotFoundException)
            {
                outputService.ReportError($"Device not found: {options.DevicePath}");
                outputService.OutputFinalResult();
            }
            catch (UnauthorizedAccessException)
            {
                outputService.ReportError($"Access denied to device: {options.DevicePath}. Try running with sudo or as root.");
                outputService.OutputFinalResult();
            }
            catch (Exception ex)
            {
                outputService.ReportError($"Unexpected error: {ex.Message}");
                outputService.OutputFinalResult();
            }
        }
    }
}
