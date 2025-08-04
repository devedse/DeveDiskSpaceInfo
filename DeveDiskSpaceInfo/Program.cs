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

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
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
                        options.DevicePath = args[++i];
                        break;
                    case "--help":
                    case "-h":
                        return null;
                    default:
                        Console.WriteLine($"Error: Unknown option '{args[i]}'");
                        return null;
                }
            }

            return options;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("DeveDiskSpaceInfo - Analyze disk space usage on Linux devices without mounting");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DeveDiskSpaceInfo [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --device, -d <path>    Path to the device to analyze");
            Console.WriteLine("                         (default: /dev/iscsi_thick_vg/iscsi_devedse)");
            Console.WriteLine("  --json                 Output results in JSON format");
            Console.WriteLine("  --help, -h             Show this help message");
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
