using DeveDiskSpaceInfo.Services;

namespace DeveDiskSpaceInfo
{
    public static class Program
    {
        // Hard-coded device (LVM logical volume backed by iSCSI)
        private const string DevicePath = "/dev/iscsi_thick_vg/iscsi_devedse";

        public static async Task Main(string[] args)
        {
            try
            {
                // Detect partitions using sfdisk
                Console.WriteLine("Detecting partitions using sfdisk...");
                var partitionTable = await PartitionDetectorService.DetectPartitionsAsync(DevicePath);
                
                if (partitionTable == null)
                {
                    Console.WriteLine("❌ Failed to detect partitions. Make sure the device exists and you have proper permissions.");
                    return;
                }

                PartitionDetectorService.PrintPartitionInfo(partitionTable);
                
                // Find and mount NTFS partitions
                var ntfsPartitions = partitionTable.Partitions.Where(p => p.IsNtfs).ToList();
                
                if (ntfsPartitions.Any())
                {
                    Console.WriteLine($"Found {ntfsPartitions.Count} NTFS partition(s). Analyzing free space...");
                    
                    foreach (var partition in ntfsPartitions)
                    {
                        NtfsAnalyzerService.AnalyzeNtfsPartition(partition, DevicePath);
                    }
                }
                else
                {
                    Console.WriteLine("No NTFS partitions found.");
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"❌ Device not found: {DevicePath}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"❌ Access denied to device: {DevicePath}");
                Console.WriteLine("Try running with sudo or as root.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Unexpected error: {ex.Message}");
            }
        }
    }
}
