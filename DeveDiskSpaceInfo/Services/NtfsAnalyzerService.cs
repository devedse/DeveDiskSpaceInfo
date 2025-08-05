using DeveDiskSpaceInfo.Models;
using DiscUtils.Ntfs;
using SimpleExec;

namespace DeveDiskSpaceInfo.Services
{
    public static partial class NtfsAnalyzerService
    {

        public static async Task AnalyzeNtfsPartition(PartitionInfo partition, string devicePath, OutputService outputService)
        {
            outputService.ReportNtfsPartitionAnalysisStart(partition);

            try
            {
                // Force kernel to flush any cached data for this device before reading
                var (output, error) = await Command.ReadAsync("blockdev", $"--flushbufs {devicePath}");

                // Open the device and seek to the partition start
                // Use FileShare.ReadWrite to avoid potential caching issues with block devices
                // Also ensure we get fresh data by opening with different sharing semantics
                using var dev = new FileStream(devicePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                dev.Seek(partition.StartOffsetBytes, SeekOrigin.Begin);

                // Create a sub-stream for this partition
                var partitionStream = new SubStream(dev, partition.StartOffsetBytes, partition.SizeInBytes);

                using var ntfs = new NtfsFileSystem(partitionStream);
                var fileSystemInfo = GetFileSystemInfo(ntfs);
                outputService.ReportNtfsAnalysisSuccess(partition, fileSystemInfo);
            }
            catch (Exception ex)
            {
                outputService.ReportNtfsAnalysisError(partition, ex.Message);
            }
        }

        private static JsonFileSystemInfo GetFileSystemInfo(NtfsFileSystem fs)
        {
            try
            {
                // Use the built-in AvailableSpace property
                long availableSpace = fs.AvailableSpace;
                long totalSize = fs.Size;
                long usedSpace = totalSize - availableSpace;

                return new JsonFileSystemInfo
                {
                    TotalSizeBytes = totalSize,
                    FreeSizeBytes = availableSpace,
                    UsedSizeBytes = usedSpace,
                    UsedPercentage = totalSize > 0 ? (double)usedSpace / totalSize * 100 : 0,
                    // Note: We don't have direct access to cluster info in DiscUtils NTFS API
                    // These would need to be calculated or retrieved differently if needed
                    TotalClusters = 0,
                    FreeClusters = 0,
                    ClusterSizeBytes = 0
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading NTFS volume information: {ex.Message}", ex);
            }
        }
    }
}
