using DiscUtils.Ntfs;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Helpers;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace DeveDiskSpaceInfo.Services
{
    public static class NtfsAnalyzerService
    {
        private static void FlushDeviceCache(string devicePath)
        {
            try
            {
                // On Linux, we can try to force a flush of the device cache
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    // First, try to use blockdev --flushbufs to flush the buffer cache for this device
                    RunCommand("blockdev", $"--flushbufs {devicePath}");
                    
                    // Also try to invalidate the cache entirely for this specific device
                    RunCommand("blockdev", $"--rereadpt {devicePath}");
                    
                    // As a last resort, try a general sync to ensure all pending writes are flushed
                    RunCommand("sync", "");
                }
            }
            catch
            {
                // Ignore any errors from cache flushing - it's best effort
                // The analysis should still work even if cache flushing fails
            }
        }

        private static void RunCommand(string command, string arguments)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
                
                process.Start();
                process.WaitForExit(5000); // 5 second timeout
            }
            catch
            {
                // Ignore command execution errors
            }
        }

        public static void AnalyzeNtfsPartition(PartitionInfo partition, string devicePath, OutputService outputService)
        {
            outputService.ReportNtfsPartitionAnalysisStart(partition);
            
            try
            {
                // Force kernel to flush any cached data for this device before reading
                // FlushDeviceCache(devicePath);
                
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

        // Simple SubStream implementation for partition access
        private sealed class SubStream : Stream
        {
            private readonly Stream _baseStream;
            private readonly long _startOffset;
            private readonly long _length;
            private long _position;

            public SubStream(Stream baseStream, long startOffset, long length)
            {
                _baseStream = baseStream;
                _startOffset = startOffset;
                _length = length;
                _position = 0;
            }

            public override bool CanRead => _baseStream.CanRead;
            public override bool CanSeek => _baseStream.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _length;
            public override long Position 
            { 
                get => _position; 
                set => Seek(value, SeekOrigin.Begin); 
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position >= _length) return 0;
                
                long availableBytes = Math.Min(count, _length - _position);
                _baseStream.Seek(_startOffset + _position, SeekOrigin.Begin);
                int bytesRead = _baseStream.Read(buffer, offset, (int)availableBytes);
                _position += bytesRead;
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _position = Math.Max(0, Math.Min(_length, offset));
                        break;
                    case SeekOrigin.Current:
                        _position = Math.Max(0, Math.Min(_length, _position + offset));
                        break;
                    case SeekOrigin.End:
                        _position = Math.Max(0, Math.Min(_length, _length + offset));
                        break;
                }
                return _position;
            }

            public override void Flush() { }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
