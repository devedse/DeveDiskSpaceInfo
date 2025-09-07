using System.Text.Json;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Helpers;

namespace DeveDiskSpaceInfo.Services
{
    public static class OutputFormattingService
    {
        public static void OutputResults(List<AnalysisResult> results, ShowOptions options)
        {
            if (options.JsonOutput)
            {
                OutputAsJson(results, options);
            }
            else
            {
                OutputAsText(results, options);
            }
        }

        private static void OutputAsJson(List<AnalysisResult> results, ShowOptions options)
        {
            var jsonResults = new List<JsonOutputResult>();
            var devicePaths = options.DevicePaths.ToList();

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var devicePath = i < devicePaths.Count ? devicePaths[i] : null;
                
                var jsonResult = new JsonOutputResult
                {
                    Success = result.Success,
                    Error = result.Error,
                    DevicePath = devicePath,
                    PartitionTable = result.PartitionTable != null ? ConvertToJsonPartitionTable(result.PartitionTable) : null,
                    NtfsAnalysis = result.NtfsAnalysisResults?.Select(ConvertToJsonNtfsAnalysis).ToList() ?? new List<JsonNtfsAnalysis>()
                };
                
                jsonResults.Add(jsonResult);
            }

            Console.WriteLine(JsonSerializer.Serialize(jsonResults, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static void OutputAsText(List<AnalysisResult> results, ShowOptions options)
        {
            var devicePaths = options.DevicePaths.ToList();

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                var devicePath = i < devicePaths.Count ? devicePaths[i] : "Unknown Device";
                
                if (results.Count > 1)
                {
                    Console.WriteLine($"\n{'='} Analysis for {devicePath} {'='}");
                }

                if (!result.Success)
                {
                    Console.WriteLine($"❌ {result.Error}");
                    continue;
                }

                if (result.PartitionTable != null)
                {
                    PrintPartitionInfo(result.PartitionTable);
                }

                if (result.NtfsAnalysisResults != null && result.NtfsAnalysisResults.Any())
                {
                    var successfulAnalyses = result.NtfsAnalysisResults.Where(r => r.Success).ToList();
                    var failedAnalyses = result.NtfsAnalysisResults.Where(r => !r.Success).ToList();

                    Console.WriteLine($"Found {result.NtfsAnalysisResults.Count} NTFS partition(s). Analyzing free space...");

                    foreach (var analysis in result.NtfsAnalysisResults)
                    {
                        Console.WriteLine($"\n--- Analyzing {analysis.Partition.Node} ---");
                        
                        if (analysis.Success && analysis.FileSystemInfo != null)
                        {
                            Console.WriteLine($"✅ Successfully mounted NTFS partition");
                            Console.WriteLine($"=== NTFS Volume Information ===");
                            Console.WriteLine($"Total space     : {analysis.FileSystemInfo.TotalSizeBytes:N0} bytes ({ByteFormatHelper.FormatBytes(analysis.FileSystemInfo.TotalSizeBytes)})");
                            Console.WriteLine($"Used space      : {analysis.FileSystemInfo.UsedSizeBytes:N0} bytes ({ByteFormatHelper.FormatBytes(analysis.FileSystemInfo.UsedSizeBytes)})");
                            Console.WriteLine($"Available space : {analysis.FileSystemInfo.FreeSizeBytes:N0} bytes ({ByteFormatHelper.FormatBytes(analysis.FileSystemInfo.FreeSizeBytes)})");
                            Console.WriteLine($"Free percentage : {(100.0 - analysis.FileSystemInfo.UsedPercentage):F2}%");
                        }
                        else
                        {
                            Console.WriteLine($"❌ Failed to mount NTFS on {analysis.Partition.Node}: {analysis.Error}");
                        }
                    }
                }
                else if (result.PartitionTable != null)
                {
                    var ntfsPartitions = result.PartitionTable.Partitions.Where(p => p.IsNtfs).ToList();
                    if (!ntfsPartitions.Any())
                    {
                        Console.WriteLine("No NTFS partitions found.");
                    }
                }
            }
        }

        private static void PrintPartitionInfo(PartitionTable partitionTable)
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

        private static JsonPartitionTable ConvertToJsonPartitionTable(PartitionTable partitionTable)
        {
            return new JsonPartitionTable
            {
                Device = partitionTable.Device,
                Label = partitionTable.Label,
                SectorSize = partitionTable.SectorSize,
                FirstLba = partitionTable.FirstLba,
                LastLba = partitionTable.LastLba,
                TotalSectors = partitionTable.LastLba - partitionTable.FirstLba + 1,
                TotalSizeBytes = (partitionTable.LastLba - partitionTable.FirstLba + 1) * partitionTable.SectorSize,
                Partitions = partitionTable.Partitions.Select(ConvertToJsonPartition).ToList()
            };
        }

        private static JsonNtfsAnalysis ConvertToJsonNtfsAnalysis(NtfsAnalysisResult result)
        {
            return new JsonNtfsAnalysis
            {
                Partition = ConvertToJsonPartition(result.Partition),
                AnalysisSuccess = result.Success,
                Error = result.Error,
                FileSystemInfo = result.FileSystemInfo
            };
        }

        private static JsonPartitionInfo ConvertToJsonPartition(PartitionInfo partition)
        {
            return new JsonPartitionInfo
            {
                Node = partition.Node,
                Name = partition.Name,
                Type = partition.Type,
                SizeBytes = partition.SizeInBytes,
                Start = partition.Start,
                StartOffsetBytes = partition.StartOffsetBytes,
                IsNtfs = partition.IsNtfs,
                Attrs = partition.Attrs
            };
        }
    }
}
