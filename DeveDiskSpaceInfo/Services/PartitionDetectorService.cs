using System.Text.Json;
using SimpleExec;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Helpers;

namespace DeveDiskSpaceInfo.Services
{
    public static class PartitionDetectorService
    {
        public static async Task<PartitionTable?> DetectPartitionsAsync(string devicePath, OutputService outputService)
        {
            outputService.ReportPartitionDetectionStart();
            
            try
            {
                var (output, _) = await Command.ReadAsync("sfdisk", $"--json {devicePath}");
                
                // Use source generation for trimming compatibility
                var result = JsonSerializer.Deserialize<SfdiskResult>(output, SfdiskJsonContext.Default.SfdiskResult);
                return result?.PartitionTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to detect partitions using sfdisk: {ex.Message}");
                return null;
            }
        }

        public static void PrintPartitionInfo(PartitionTable partitionTable)
        {
            Console.WriteLine($"=== Partition Table Information ===");
            Console.WriteLine($"Device: {partitionTable.Device}");
            Console.WriteLine($"Type: {partitionTable.Label.ToUpper()}");
            Console.WriteLine($"Sector Size: {partitionTable.SectorSize} bytes");
            Console.WriteLine($"Total Sectors: {partitionTable.LastLba - partitionTable.FirstLba + 1:N0}");
            Console.WriteLine($"Total Size: {ByteFormatHelper.FormatBytes((partitionTable.LastLba - partitionTable.FirstLba + 1) * partitionTable.SectorSize)}");
            Console.WriteLine($"Partitions: {partitionTable.Partitions.Count}");
            Console.WriteLine();

            for (int i = 0; i < partitionTable.Partitions.Count; i++)
            {
                var partition = partitionTable.Partitions[i];
                Console.WriteLine($"Partition {i + 1}:");
                Console.WriteLine($"  Node: {partition.Node}");
                Console.WriteLine($"  Name: {partition.Name}");
                Console.WriteLine($"  Type: {partition.Type}");
                Console.WriteLine($"  Size: {ByteFormatHelper.FormatBytes(partition.SizeInBytes)}");
                Console.WriteLine($"  Start: Sector {partition.Start:N0} (Offset: {ByteFormatHelper.FormatBytes(partition.StartOffsetBytes)})");
                Console.WriteLine($"  NTFS: {(partition.IsNtfs ? "✅ Yes" : "❌ No")}");
                if (!string.IsNullOrEmpty(partition.Attrs))
                {
                    Console.WriteLine($"  Attributes: {partition.Attrs}");
                }
                Console.WriteLine();
            }
        }
    }
}
