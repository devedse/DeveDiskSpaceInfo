using DeveDiskSpaceInfo.Helpers;
using DeveDiskSpaceInfo.Models;
using System.Text.Json;

namespace DeveDiskSpaceInfo.Tests
{
    [TestClass]
    public sealed class ModelIntegrationTests
    {
        [TestMethod]
        public void ByteFormatHelper_FormatsVariousSizes_Correctly()
        {
            // Test that ByteFormatHelper works with realistic disk sizes
            Assert.AreEqual("100.00 MB", ByteFormatHelper.FormatBytes(100 * 1024 * 1024));
            Assert.AreEqual("1.00 GB", ByteFormatHelper.FormatBytes(1024 * 1024 * 1024));
            Assert.AreEqual("500.00 GB", ByteFormatHelper.FormatBytes(500L * 1024 * 1024 * 1024));
        }

        [TestMethod]
        public void SfdiskResult_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var sampleJson = """
            {
                "partitiontable": {
                    "label": "gpt",
                    "id": "12345678-1234-1234-1234-123456789abc",
                    "device": "/dev/sda",
                    "unit": "sectors",
                    "firstlba": 34,
                    "lastlba": 1953525134,
                    "sectorsize": 512,
                    "partitions": [
                        {
                            "node": "/dev/sda1",
                            "start": 2048,
                            "size": 204800,
                            "type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7",
                            "uuid": "abcd1234-5678-9012-3456-789012345678",
                            "name": "EFI System"
                        },
                        {
                            "node": "/dev/sda2",
                            "start": 206848,
                            "size": 1953318287,
                            "type": "0FC63DAF-8483-4772-8E79-3D69D8477DE4",
                            "uuid": "efgh5678-9012-3456-7890-123456789012",
                            "name": "Linux filesystem"
                        }
                    ]
                }
            }
            """;

            // Act
            var sfdiskResult = JsonSerializer.Deserialize<SfdiskResult>(sampleJson);

            // Assert
            Assert.IsNotNull(sfdiskResult);
            Assert.IsNotNull(sfdiskResult.PartitionTable);
            Assert.AreEqual("gpt", sfdiskResult.PartitionTable.Label);
            Assert.AreEqual("/dev/sda", sfdiskResult.PartitionTable.Device);
            Assert.AreEqual(2, sfdiskResult.PartitionTable.Partitions.Count);

            var efiPartition = sfdiskResult.PartitionTable.Partitions[0];
            Assert.AreEqual("/dev/sda1", efiPartition.Node);
            Assert.IsTrue(efiPartition.IsNtfs);
            Assert.AreEqual(104857600, efiPartition.SizeInBytes); // 204800 * 512

            var linuxPartition = sfdiskResult.PartitionTable.Partitions[1];
            Assert.AreEqual("/dev/sda2", linuxPartition.Node);
            Assert.IsFalse(linuxPartition.IsNtfs);
        }

        [TestMethod]
        public void PartitionInfo_CalculatesSizesCorrectly()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Start = 2048,    // 1MB offset
                Size = 2097152   // 1GB in 512-byte sectors
            };

            // Act & Assert
            Assert.AreEqual(1048576, partition.StartOffsetBytes);      // 1MB in bytes
            Assert.AreEqual(1073741824, partition.SizeInBytes);       // 1GB in bytes
            
            // Test that the ByteFormatHelper would format these correctly
            Assert.AreEqual("1.00 MB", ByteFormatHelper.FormatBytes(partition.StartOffsetBytes));
            Assert.AreEqual("1.00 GB", ByteFormatHelper.FormatBytes(partition.SizeInBytes));
        }

        [TestMethod]
        public void PartitionInfo_IdentifiesNtfsPartitionsCorrectly()
        {
            // Test various NTFS scenarios
            var ntfsPartition = new PartitionInfo { Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7" };
            var ntfsPartitionLower = new PartitionInfo { Type = "ebd0a0a2-b9e5-4433-87c0-68b6b72699c7" };
            var ext4Partition = new PartitionInfo { Type = "0FC63DAF-8483-4772-8E79-3D69D8477DE4" };
            var fatPartition = new PartitionInfo { Type = "C12A7328-F81F-11D2-BA4B-00A0C93EC93B" };

            Assert.IsTrue(ntfsPartition.IsNtfs);
            Assert.IsTrue(ntfsPartitionLower.IsNtfs);
            Assert.IsFalse(ext4Partition.IsNtfs);
            Assert.IsFalse(fatPartition.IsNtfs);
        }
    }
}
