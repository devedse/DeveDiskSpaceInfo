using System.Text.Json;
using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public class JsonOutputModelsTests
    {
        [TestMethod]
        public void JsonOutputResult_SerializesToExpectedFormat()
        {
            // Arrange
            var result = new JsonOutputResult
            {
                Success = true,
                DevicePath = "/test/device",
                PartitionTable = new JsonPartitionTable
                {
                    Device = "/test/device",
                    Label = "gpt",
                    SectorSize = 512,
                    FirstLba = 34,
                    LastLba = 1000000,
                    TotalSectors = 999967,
                    TotalSizeBytes = 511983104,
                    Partitions = new List<JsonPartitionInfo>
                    {
                        new JsonPartitionInfo
                        {
                            Node = "/test/device1",
                            Name = "Test Partition",
                            Type = "ntfs",
                            SizeBytes = 100000000,
                            Start = 2048,
                            StartOffsetBytes = 1048576,
                            IsNtfs = true
                        }
                    }
                },
                NtfsAnalysis = new List<JsonNtfsAnalysis>
                {
                    new JsonNtfsAnalysis
                    {
                        Partition = new JsonPartitionInfo
                        {
                            Node = "/test/device1",
                            Name = "Test Partition",
                            Type = "ntfs",
                            SizeBytes = 100000000,
                            Start = 2048,
                            StartOffsetBytes = 1048576,
                            IsNtfs = true
                        },
                        AnalysisSuccess = true,
                        FileSystemInfo = new JsonFileSystemInfo
                        {
                            TotalSizeBytes = 100000000,
                            FreeSizeBytes = 50000000,
                            UsedSizeBytes = 50000000,
                            UsedPercentage = 50.0
                        }
                    }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(result, SfdiskJsonContext.Default.JsonOutputResult);

            // Assert
            Assert.IsTrue(json.Contains("\"success\": true"));
            Assert.IsTrue(json.Contains("\"device_path\""));
            Assert.IsTrue(json.Contains("\"partition_table\""));
            Assert.IsTrue(json.Contains("\"ntfs_analysis\""));
            Assert.IsTrue(json.Contains("\"is_ntfs\": true"));
            Assert.IsTrue(json.Contains("\"used_percentage\": 50"));
        }

        [TestMethod]
        public void JsonPartitionInfo_ConvertsFromPartitionInfo()
        {
            // Arrange
            var partitionInfo = new PartitionInfo
            {
                Node = "/dev/test1",
                Name = "Test",
                Type = "ntfs",
                Start = 2048,
                Size = 200000
            };

            // Act
            var jsonPartition = new JsonPartitionInfo
            {
                Node = partitionInfo.Node,
                Name = partitionInfo.Name,
                Type = partitionInfo.Type,
                SizeBytes = partitionInfo.SizeInBytes,
                Start = partitionInfo.Start,
                StartOffsetBytes = partitionInfo.StartOffsetBytes,
                IsNtfs = partitionInfo.IsNtfs
            };

            // Assert
            Assert.AreEqual(partitionInfo.Node, jsonPartition.Node);
            Assert.AreEqual(partitionInfo.Name, jsonPartition.Name);
            Assert.AreEqual(partitionInfo.Type, jsonPartition.Type);
            Assert.AreEqual(partitionInfo.SizeInBytes, jsonPartition.SizeBytes);
            Assert.AreEqual(partitionInfo.IsNtfs, jsonPartition.IsNtfs);
        }

        [TestMethod]
        public void JsonFileSystemInfo_HasExpectedProperties()
        {
            // Arrange & Act
            var fsInfo = new JsonFileSystemInfo
            {
                TotalSizeBytes = 1000000,
                FreeSizeBytes = 400000,
                UsedSizeBytes = 600000,
                UsedPercentage = 60.0,
                TotalClusters = 1000,
                FreeClusters = 400,
                ClusterSizeBytes = 1000
            };

            // Assert
            Assert.AreEqual(1000000, fsInfo.TotalSizeBytes);
            Assert.AreEqual(400000, fsInfo.FreeSizeBytes);
            Assert.AreEqual(600000, fsInfo.UsedSizeBytes);
            Assert.AreEqual(60.0, fsInfo.UsedPercentage);
            Assert.AreEqual(1000, fsInfo.TotalClusters);
            Assert.AreEqual(400, fsInfo.FreeClusters);
            Assert.AreEqual(1000, fsInfo.ClusterSizeBytes);
        }
    }
}