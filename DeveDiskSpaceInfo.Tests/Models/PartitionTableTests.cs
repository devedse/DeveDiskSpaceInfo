using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public sealed class PartitionTableTests
    {
        [TestMethod]
        public void PartitionTable_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var partitionTable = new PartitionTable();

            // Assert
            Assert.AreEqual(string.Empty, partitionTable.Label);
            Assert.AreEqual(string.Empty, partitionTable.Id);
            Assert.AreEqual(string.Empty, partitionTable.Device);
            Assert.AreEqual(string.Empty, partitionTable.Unit);
            Assert.AreEqual(0, partitionTable.FirstLba);
            Assert.AreEqual(0, partitionTable.LastLba);
            Assert.AreEqual(0, partitionTable.SectorSize);
            Assert.IsNotNull(partitionTable.Partitions);
            Assert.AreEqual(0, partitionTable.Partitions.Count);
        }

        [TestMethod]
        public void PartitionTable_SetProperties_WorksCorrectly()
        {
            // Arrange
            var partitionTable = new PartitionTable();

            // Act
            partitionTable.Label = "gpt";
            partitionTable.Id = "12345678-1234-1234-1234-123456789abc";
            partitionTable.Device = "/dev/sda";
            partitionTable.Unit = "sectors";
            partitionTable.FirstLba = 34;
            partitionTable.LastLba = 1953525134;
            partitionTable.SectorSize = 512;

            // Assert
            Assert.AreEqual("gpt", partitionTable.Label);
            Assert.AreEqual("12345678-1234-1234-1234-123456789abc", partitionTable.Id);
            Assert.AreEqual("/dev/sda", partitionTable.Device);
            Assert.AreEqual("sectors", partitionTable.Unit);
            Assert.AreEqual(34, partitionTable.FirstLba);
            Assert.AreEqual(1953525134, partitionTable.LastLba);
            Assert.AreEqual(512, partitionTable.SectorSize);
        }

        [TestMethod]
        public void PartitionTable_AddPartitions_WorksCorrectly()
        {
            // Arrange
            var partitionTable = new PartitionTable();
            var partition1 = new PartitionInfo
            {
                Node = "/dev/sda1",
                Start = 2048,
                Size = 204800,
                Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
            };
            var partition2 = new PartitionInfo
            {
                Node = "/dev/sda2",
                Start = 206848,
                Size = 1953318287,
                Type = "0FC63DAF-8483-4772-8E79-3D69D8477DE4"
            };

            // Act
            partitionTable.Partitions.Add(partition1);
            partitionTable.Partitions.Add(partition2);

            // Assert
            Assert.AreEqual(2, partitionTable.Partitions.Count);
            Assert.AreEqual("/dev/sda1", partitionTable.Partitions[0].Node);
            Assert.AreEqual("/dev/sda2", partitionTable.Partitions[1].Node);
        }

        [TestMethod]
        public void PartitionTable_InitializeWithPartitions_WorksCorrectly()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Node = "/dev/sda1",
                Start = 2048,
                Size = 204800
            };

            var partitions = new List<PartitionInfo> { partition };

            // Act
            var partitionTable = new PartitionTable
            {
                Label = "gpt",
                Device = "/dev/sda",
                Partitions = partitions
            };

            // Assert
            Assert.AreEqual("gpt", partitionTable.Label);
            Assert.AreEqual("/dev/sda", partitionTable.Device);
            Assert.AreEqual(1, partitionTable.Partitions.Count);
            Assert.AreEqual("/dev/sda1", partitionTable.Partitions[0].Node);
        }
    }
}
