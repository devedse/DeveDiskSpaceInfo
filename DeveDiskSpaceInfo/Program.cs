using CommandLine;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;
using SimpleExec;
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
            var logger = new OutputService(options);
            var result = new AnalysisResult { Success = true };

            try
            {
                // Force kernel to flush any cached data for this device before reading
                var (output, error) = await Command.ReadAsync("blockdev", $"--flushbufs {options.DevicePath}");

                // Detect partitions using sfdisk
                var partitionTable = await PartitionDetectorService.DetectPartitionsAsync(options.DevicePath, logger);
                
                if (partitionTable == null)
                {
                    result.Success = false;
                    result.Error = "Failed to detect partitions. Make sure the device exists and you have proper permissions.";
                    OutputFormattingService.OutputResults(result, options);
                    return 1;
                }

                result.PartitionTable = partitionTable;
                
                // Find and mount NTFS partitions
                var ntfsPartitions = partitionTable.Partitions.Where(p => p.IsNtfs).ToList();
                
                if (ntfsPartitions.Any())
                {
                    result.NtfsAnalysisResults = new List<NtfsAnalysisResult>();
                    
                    foreach (var partition in ntfsPartitions)
                    {
                        var analysisResult =  NtfsAnalyzerService.AnalyzeNtfsPartition(partition, options.DevicePath, logger);
                        result.NtfsAnalysisResults.Add(analysisResult);
                    }
                }

                OutputFormattingService.OutputResults(result, options);
                return 0;
            }
            catch (FileNotFoundException)
            {
                result.Success = false;
                result.Error = $"Device not found: {options.DevicePath}";
                OutputFormattingService.OutputResults(result, options);
                return 1;
            }
            catch (UnauthorizedAccessException)
            {
                result.Success = false;
                result.Error = $"Access denied to device: {options.DevicePath}. Try running with sudo or as root.";
                OutputFormattingService.OutputResults(result, options);
                return 1;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = $"Unexpected error: {ex.Message}";
                OutputFormattingService.OutputResults(result, options);
                return 1;
            }
        }
    }
}
