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
                    async (ShowOptions opts) =>
                    {
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
            DiscUtils.FileSystems.SetupHelper.SetupFileSystems();

            var logger = new OutputService(options);
            var results = new List<DDSIAnalysisResult>();
            var hasSuccesses = false;

            foreach (var devicePath in options.DevicePaths)
            {
                // Force kernel to flush any cached data for this device before reading
                try
                {
                    var (output, error) = await Command.ReadAsync("blockdev", $"--flushbufs {devicePath}");
                }
                catch (Exception ex)
                {
                    logger.WriteError($"Warning: Failed to flush buffers for {devicePath}. Ensure the `blockdev` tools is available: {ex.Message}");
                    throw;
                }

                var analysisResult = PartitionDetectorService2.DetectPartitionsAsync(devicePath, logger);

                if (analysisResult != null)
                {
                    results.Add(analysisResult);
                    hasSuccesses = true;
                }
            }

            OutputFormattingService.OutputResults(results, options);
            return hasSuccesses ? 0 : 1;
        }
    }
}
