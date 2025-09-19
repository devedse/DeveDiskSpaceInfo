using DeveDiskSpaceInfo.Helpers;
using DeveDiskSpaceInfo.Models;
using System.Text.Json;

namespace DeveDiskSpaceInfo.Services
{
    public static class OutputFormattingService
    {
        public static void OutputResults(List<DDSIAnalysisResult> results, ShowOptions options)
        {
            if (options.JsonOutput)
            {
                OutputAsJson(results);
            }
            else
            {
                OutputAsText(results, options);
            }
        }

        private static void OutputAsJson(List<DDSIAnalysisResult> results)
        {
            Console.WriteLine(JsonSerializer.Serialize(results, SfdiskJsonContext.Default.Options));
        }

        private static void OutputAsText(List<DDSIAnalysisResult> results, ShowOptions options)
        {
            var devicePaths = options.DevicePaths.ToList();

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var devicePath = i < devicePaths.Count ? devicePaths[i] : result.DevicePath;

                if (results.Count > 1)
                {
                    Console.WriteLine($"\n{'='} Analysis for {devicePath} {'='}");
                }

                if (!result.Success)
                {
                    Console.WriteLine($"❌ {result.Error}");
                    continue;
                }

                if (!string.IsNullOrEmpty(result.Warning))
                {
                    Console.WriteLine($"⚠️ Warning: {result.Warning}");
                }

                if (result.Disk != null)
                {
                    PrintDiskInfo(result.Disk);
                }
            }
        }

        private static void PrintDiskInfo(DDSIDisk disk)
        {
            Console.WriteLine($"=== Disk Information ===");
            Console.WriteLine($"Device: {disk.DevicePath}");
            Console.WriteLine($"Capacity: {ByteFormatHelper.FormatBytes(disk.Capacity)}");
            Console.WriteLine($"Sector Size: {disk.SectorSize} bytes");
            Console.WriteLine($"Is Partitioned: {(disk.IsPartitioned ? "Yes" : "No")}");
            Console.WriteLine($"Disk Class: {disk.DiskClass}");
            
            if (disk.Geometry != null)
            {
                Console.WriteLine($"Geometry: {disk.Geometry}");
            }
            
            Console.WriteLine($"Partitions: {disk.Partitions.Count}");
            Console.WriteLine();

            for (int i = 0; i < disk.Partitions.Count; i++)
            {
                var partition = disk.Partitions[i];
                Console.WriteLine($"Partition {i + 1}:");
                Console.WriteLine($"  Type: {partition.TypeAsString}");
                Console.WriteLine($"  Volume Type: {partition.VolumeType}");
                Console.WriteLine($"  Sectors: {partition.FirstSector:N0} - {partition.LastSector:N0} ({partition.SectorCount:N0} sectors)");
                Console.WriteLine($"  Raw Size: {ByteFormatHelper.FormatBytes(partition.SectorCount * partition.SectorSize)}");
                
                // Show aggregated file system information if available
                if (partition.Size > 0)
                {
                    Console.WriteLine($"  Total FS Size: {ByteFormatHelper.FormatBytes(partition.Size)}");
                    Console.WriteLine($"  Total Used: {ByteFormatHelper.FormatBytes(partition.UsedSpace)} ({(partition.Size > 0 ? (double)partition.UsedSpace / partition.Size * 100 : 0):F1}%)");
                    Console.WriteLine($"  Total Available: {ByteFormatHelper.FormatBytes(partition.AvailableSpace)}");
                }
                
                if (partition.FileSystems.Any())
                {
                    Console.WriteLine($"  File Systems: {partition.FileSystems.Count}");
                    
                    foreach (var fs in partition.FileSystems)
                    {
                        Console.WriteLine($"    - {fs.FriendlyName}");
                        Console.WriteLine($"      Label: {fs.VolumeLabel}");
                        Console.WriteLine($"      Size: {ByteFormatHelper.FormatBytes(fs.Size)}");
                        Console.WriteLine($"      Used: {ByteFormatHelper.FormatBytes(fs.UsedSpace)} ({(fs.Size > 0 ? (double)fs.UsedSpace / fs.Size * 100 : 0):F1}%)");
                        Console.WriteLine($"      Available: {ByteFormatHelper.FormatBytes(fs.AvailableSpace)}");
                        Console.WriteLine($"      Can Write: {(fs.CanWrite ? "Yes" : "No")}");
                    }
                }
                else
                {
                    Console.WriteLine($"  File Systems: None detected");
                }
                
                Console.WriteLine();
            }
        }
    }
}
