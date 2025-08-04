using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;
using System.Text.Json;

namespace DeveDiskSpaceInfo.Tests.Services
{
    [TestClass]
    public sealed class SfdiskJsonContextTests
    {
        [TestMethod]
        public void SfdiskJsonContext_DeserializeSfdiskResult_WorksCorrectly()
        {
            // Arrange
            var json = """
            {
                "partitiontable": {
                    "label": "gpt",
                    "device": "/dev/sda",
                    "unit": "sectors",
                    "sectorsize": 512,
                    "partitions": [
                        {
                            "node": "/dev/sda1",
                            "start": 2048,
                            "size": 204800,
                            "type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
                        }
                    ]
                }
            }
            """;

            // Act
            var result = JsonSerializer.Deserialize(json, SfdiskJsonContext.Default.SfdiskResult);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PartitionTable);
            Assert.AreEqual("gpt", result.PartitionTable.Label);
            Assert.AreEqual("/dev/sda", result.PartitionTable.Device);
            Assert.AreEqual(1, result.PartitionTable.Partitions.Count);
            Assert.AreEqual("/dev/sda1", result.PartitionTable.Partitions[0].Node);
        }

        [TestMethod]
        public void SfdiskJsonContext_SerializeSfdiskResult_WorksCorrectly()
        {
            // Arrange
            var sfdiskResult = new SfdiskResult
            {
                PartitionTable = new PartitionTable
                {
                    Label = "gpt",
                    Device = "/dev/sda",
                    Unit = "sectors",
                    SectorSize = 512,
                    Partitions = new List<PartitionInfo>
                    {
                        new PartitionInfo
                        {
                            Node = "/dev/sda1",
                            Start = 2048,
                            Size = 204800,
                            Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
                        }
                    }
                }
            };

            // Act
            var json = JsonSerializer.Serialize(sfdiskResult, SfdiskJsonContext.Default.SfdiskResult);

            // Assert
            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("\"label\":\"gpt\""));
            Assert.IsTrue(json.Contains("\"device\":\"/dev/sda\""));
            Assert.IsTrue(json.Contains("\"node\":\"/dev/sda1\""));
        }

        [TestMethod]
        public void SfdiskJsonContext_CaseInsensitiveDeserialization_WorksCorrectly()
        {
            // Arrange - JSON with different casing
            var json = """
            {
                "PartitionTable": {
                    "Label": "GPT",
                    "Device": "/dev/sda",
                    "Partitions": [
                        {
                            "Node": "/dev/sda1",
                            "Start": 2048,
                            "Size": 204800,
                            "Type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
                        }
                    ]
                }
            }
            """;

            // Act
            var result = JsonSerializer.Deserialize(json, SfdiskJsonContext.Default.SfdiskResult);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PartitionTable);
            Assert.AreEqual("GPT", result.PartitionTable.Label);
            Assert.AreEqual("/dev/sda", result.PartitionTable.Device);
            Assert.AreEqual(1, result.PartitionTable.Partitions.Count);
        }

        [TestMethod]
        public void SfdiskJsonContext_DeserializePartitionTable_WorksCorrectly()
        {
            // Arrange
            var json = """
            {
                "label": "gpt",
                "device": "/dev/sda",
                "unit": "sectors",
                "sectorsize": 512,
                "partitions": []
            }
            """;

            // Act
            var result = JsonSerializer.Deserialize(json, SfdiskJsonContext.Default.PartitionTable);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("gpt", result.Label);
            Assert.AreEqual("/dev/sda", result.Device);
            Assert.AreEqual("sectors", result.Unit);
            Assert.AreEqual(512, result.SectorSize);
            Assert.IsNotNull(result.Partitions);
            Assert.AreEqual(0, result.Partitions.Count);
        }

        [TestMethod]
        public void SfdiskJsonContext_DeserializePartitionInfo_WorksCorrectly()
        {
            // Arrange
            var json = """
            {
                "node": "/dev/sda1",
                "start": 2048,
                "size": 204800,
                "type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7",
                "uuid": "12345678-1234-1234-1234-123456789abc",
                "name": "EFI System"
            }
            """;

            // Act
            var result = JsonSerializer.Deserialize(json, SfdiskJsonContext.Default.PartitionInfo);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sda1", result.Node);
            Assert.AreEqual(2048, result.Start);
            Assert.AreEqual(204800, result.Size);
            Assert.AreEqual("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", result.Type);
            Assert.AreEqual("12345678-1234-1234-1234-123456789abc", result.Uuid);
            Assert.AreEqual("EFI System", result.Name);
            Assert.IsTrue(result.IsNtfs);
        }
    }
}
