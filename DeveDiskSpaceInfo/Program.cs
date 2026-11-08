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
            var results = await AnalyzeDevicesAsync(options.DevicePaths.ToList(), options.Parallelism, logger, FlushBuffersAsync);

            OutputFormattingService.OutputResults(results, options);
            return results.Count > 0 ? 0 : 1;
        }

        /// <summary>
        /// Analyzes the devices <paramref name="parallelism"/> at a time (each device is independent: its own stream and
        /// file systems) and returns the results in the order the devices were given. A device whose buffers can't be
        /// flushed is left out, as before.
        /// </summary>
        internal static async Task<List<DDSIAnalysisResult>> AnalyzeDevicesAsync(IReadOnlyList<string> devicePaths, int parallelism,
            OutputService logger, Func<string, Task> flushBuffers)
        {
            var results = new DDSIAnalysisResult?[devicePaths.Count];
            var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, parallelism) };

            await Parallel.ForEachAsync(Enumerable.Range(0, devicePaths.Count), options, async (index, _) =>
            {
                var devicePath = devicePaths[index];

                // Force the kernel to drop its cached data for this device before reading: the iSCSI target writes to
                // the volume without going through this cache, so cached pages can be out of date.
                try
                {
                    await flushBuffers(devicePath);
                }
                catch (Exception ex)
                {
                    logger.WriteError($"Warning: Failed to flush buffers for {devicePath}. Ensure the `blockdev` tools is available and also that this disk is available: {ex.Message}");
                    return;
                }

                results[index] = await PartitionDetectorService2.DetectPartitionsAsync(devicePath, logger);
            });

            return results.OfType<DDSIAnalysisResult>().ToList();
        }

        private static async Task FlushBuffersAsync(string devicePath) => await Command.ReadAsync("blockdev", $"--flushbufs {devicePath}");
    }
}
