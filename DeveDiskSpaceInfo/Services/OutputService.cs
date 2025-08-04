using System.Text.Json;
using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Services
{
    public class OutputService
    {
        private readonly CommandLineOptions _options;
        private JsonOutputResult? _jsonResult;

        public bool IsJsonMode => _options.JsonOutput;

        public OutputService(CommandLineOptions options)
        {
            _options = options;
            if (_options.JsonOutput)
            {
                _jsonResult = new JsonOutputResult
                {
                    DevicePath = _options.DevicePath,
                    NtfsAnalysis = new List<JsonNtfsAnalysis>()
                };
            }
        }

        public void ReportError(string error)
        {
            if (_options.JsonOutput)
            {
                _jsonResult!.Success = false;
                _jsonResult.Error = error;
            }
            else
            {
                Console.WriteLine($"❌ {error}");
            }
        }

        public void ReportPartitionTableDetected(PartitionTable partitionTable)
        {
            if (_options.JsonOutput)
            {
                _jsonResult!.PartitionTable = new JsonPartitionTable
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
            else
            {
                PartitionDetectorService.PrintPartitionInfo(partitionTable);
            }
        }

        public void ReportNtfsAnalysisStart(int ntfsPartitionCount)
        {
            if (!_options.JsonOutput)
            {
                Console.WriteLine($"Found {ntfsPartitionCount} NTFS partition(s). Analyzing free space...");
            }
        }

        public void ReportNoNtfsPartitions()
        {
            if (!_options.JsonOutput)
            {
                Console.WriteLine("No NTFS partitions found.");
            }
        }

        public void ReportNtfsPartitionAnalysisStart(PartitionInfo partition)
        {
            if (!_options.JsonOutput)
            {
                Console.WriteLine($"\n--- Analyzing {partition.Node} ---");
            }
        }

        public void ReportNtfsAnalysisSuccess(PartitionInfo partition, JsonFileSystemInfo fileSystemInfo)
        {
            if (_options.JsonOutput)
            {
                _jsonResult!.NtfsAnalysis!.Add(new JsonNtfsAnalysis
                {
                    Partition = ConvertToJsonPartition(partition),
                    AnalysisSuccess = true,
                    FileSystemInfo = fileSystemInfo
                });
            }
            else
            {
                Console.WriteLine($"✅ Successfully mounted NTFS partition");
                Console.WriteLine($"=== NTFS Volume Information ===");
                Console.WriteLine($"Total space     : {fileSystemInfo.TotalSizeBytes:N0} bytes ({Helpers.ByteFormatHelper.FormatBytes(fileSystemInfo.TotalSizeBytes)})");
                Console.WriteLine($"Used space      : {fileSystemInfo.UsedSizeBytes:N0} bytes ({Helpers.ByteFormatHelper.FormatBytes(fileSystemInfo.UsedSizeBytes)})");
                Console.WriteLine($"Available space : {fileSystemInfo.FreeSizeBytes:N0} bytes ({Helpers.ByteFormatHelper.FormatBytes(fileSystemInfo.FreeSizeBytes)})");
                Console.WriteLine($"Free percentage : {(100.0 - fileSystemInfo.UsedPercentage):F2}%");
            }
        }

        public void ReportNtfsAnalysisError(PartitionInfo partition, string error)
        {
            if (_options.JsonOutput)
            {
                _jsonResult!.NtfsAnalysis!.Add(new JsonNtfsAnalysis
                {
                    Partition = ConvertToJsonPartition(partition),
                    AnalysisSuccess = false,
                    Error = error
                });
            }
            else
            {
                Console.WriteLine($"❌ Failed to mount NTFS on {partition.Node}: {error}");
            }
        }

        public void ReportPartitionDetectionStart()
        {
            if (!_options.JsonOutput)
            {
                Console.WriteLine("Detecting partitions using sfdisk...");
            }
        }

        public void ReportPartitionDetectionFailed()
        {
            ReportError("Failed to detect partitions. Make sure the device exists and you have proper permissions.");
        }

        public void OutputFinalResult()
        {
            if (_options.JsonOutput)
            {
                _jsonResult!.Success = _jsonResult.Error == null;
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                Console.WriteLine(JsonSerializer.Serialize(_jsonResult, jsonOptions));
            }
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