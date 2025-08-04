using CommandLine;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;
using System.Diagnostics.CodeAnalysis;

namespace DeveDiskSpaceInfo
{
    public static class Program
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ShowOptions))]
        public static async Task<int> Main(string[] args)
        {
            return await Parser.Default.ParseArguments<ShowOptions>(args)
                .MapResult(
                    async (ShowOptions opts) => {
                        // Additional validation for empty device path
                        if (string.IsNullOrWhiteSpace(opts.DevicePath))
                        {
                            Console.WriteLine("Error: Device path cannot be empty");
                            return 1;
                        }
                        return await ExecuteAnalysis(opts);
                    },
                    _ => Task.FromResult(1));
        }

        private static async Task<int> ExecuteAnalysis(ShowOptions options)
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
                    return 1;
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
                return 0;
            }
            catch (FileNotFoundException)
            {
                outputService.ReportError($"Device not found: {options.DevicePath}");
                outputService.OutputFinalResult();
                return 1;
            }
            catch (UnauthorizedAccessException)
            {
                outputService.ReportError($"Access denied to device: {options.DevicePath}. Try running with sudo or as root.");
                outputService.OutputFinalResult();
                return 1;
            }
            catch (Exception ex)
            {
                outputService.ReportError($"Unexpected error: {ex.Message}");
                outputService.OutputFinalResult();
                return 1;
            }
        }
    }
}
