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
                        // Additional validation for empty device paths
                        if (!opts.DevicePaths.Any() || opts.DevicePaths.Any(string.IsNullOrWhiteSpace))
                        {
                            Console.WriteLine("Error: At least one valid device path must be provided");
                            return 1;
                        }
                        return await ExecuteAnalysis(opts);
                    },
                    _ => Task.FromResult(1));
        }

        private static async Task<int> ExecuteAnalysis(ShowOptions options)
        {
            var logger = new OutputService(options);
            var results = new List<AnalysisResult>();
            var hasSuccesses = false;

            foreach (var devicePath in options.DevicePaths)
            {
                var result = new AnalysisResult { Success = true };

                try
                {
                    // Force kernel to flush any cached data for this device before reading
                    var (output, error) = await Command.ReadAsync("blockdev", $"--flushbufs {devicePath}");

                    // Detect partitions using sfdisk
                    var partitionTable = await PartitionDetectorService.DetectPartitionsAsync(devicePath, logger);
                    
                    if (partitionTable == null)
                    {
                        result.Success = false;
                        result.Error = "Failed to detect partitions. Make sure the device exists and you have proper permissions.";
                        results.Add(result);
                        continue;
                    }

                    result.PartitionTable = partitionTable;
                    
                    // Find and mount NTFS partitions
                    var ntfsPartitions = partitionTable.Partitions.Where(p => p.IsNtfs).ToList();
                    
                    if (ntfsPartitions.Any())
                    {
                        result.NtfsAnalysisResults = new List<NtfsAnalysisResult>();
                        
                        foreach (var partition in ntfsPartitions)
                        {
                            var analysisResult = NtfsAnalyzerService.AnalyzeNtfsPartition(partition, devicePath, logger);
                            result.NtfsAnalysisResults.Add(analysisResult);
                        }
                    }

                    results.Add(result);
                    hasSuccesses = true;
                }
                catch (FileNotFoundException)
                {
                    result.Success = false;
                    result.Error = $"Device not found: {devicePath}";
                    results.Add(result);
                }
                catch (UnauthorizedAccessException)
                {
                    result.Success = false;
                    result.Error = $"Access denied to device: {devicePath}. Try running with sudo or as root.";
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.Error = $"Unexpected error: {ex.Message}";
                    results.Add(result);
                }
            }

            OutputFormattingService.OutputResults(results, options);
            return hasSuccesses ? 0 : 1;
        }
    }
}
