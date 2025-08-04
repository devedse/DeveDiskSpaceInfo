using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public sealed class SfdiskResultTests
    {
        [TestMethod]
        public void SfdiskResult_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var sfdiskResult = new SfdiskResult();

            // Assert
            Assert.IsNotNull(sfdiskResult.PartitionTable);
            Assert.AreEqual(string.Empty, sfdiskResult.PartitionTable.Label);
            Assert.AreEqual(string.Empty, sfdiskResult.PartitionTable.Device);
            Assert.AreEqual(0, sfdiskResult.PartitionTable.Partitions.Count);
        }

        [TestMethod]
        public void SfdiskResult_SetPartitionTable_WorksCorrectly()
        {
            // Arrange
            var sfdiskResult = new SfdiskResult();
            var partitionTable = new PartitionTable
            {
                Label = "gpt",
                Device = "/dev/sda",
                Unit = "sectors",
                FirstLba = 34,
                LastLba = 1953525134,
                SectorSize = 512
            };

            // Act
            sfdiskResult.PartitionTable = partitionTable;

            // Assert
            Assert.AreEqual("gpt", sfdiskResult.PartitionTable.Label);
            Assert.AreEqual("/dev/sda", sfdiskResult.PartitionTable.Device);
            Assert.AreEqual("sectors", sfdiskResult.PartitionTable.Unit);
            Assert.AreEqual(34, sfdiskResult.PartitionTable.FirstLba);
            Assert.AreEqual(1953525134, sfdiskResult.PartitionTable.LastLba);
            Assert.AreEqual(512, sfdiskResult.PartitionTable.SectorSize);
        }

        [TestMethod]
        public void SfdiskResult_WithPartitionsInPartitionTable_WorksCorrectly()
        {
            // Arrange
            var partition1 = new PartitionInfo
            {
                Node = "/dev/sda1",
                Start = 2048,
                Size = 204800,
                Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7",
                Name = "EFI System"
            };

            var partition2 = new PartitionInfo
            {
                Node = "/dev/sda2",
                Start = 206848,
                Size = 1953318287,
                Type = "0FC63DAF-8483-4772-8E79-3D69D8477DE4",
                Name = "Linux filesystem"
            };

            var partitionTable = new PartitionTable
            {
                Label = "gpt",
                Device = "/dev/sda",
                Partitions = new List<PartitionInfo> { partition1, partition2 }
            };

            // Act
            var sfdiskResult = new SfdiskResult
            {
                PartitionTable = partitionTable
            };

            // Assert
            Assert.AreEqual(2, sfdiskResult.PartitionTable.Partitions.Count);
            
            var firstPartition = sfdiskResult.PartitionTable.Partitions[0];
            Assert.AreEqual("/dev/sda1", firstPartition.Node);
            Assert.AreEqual(2048, firstPartition.Start);
            Assert.AreEqual(204800, firstPartition.Size);
            Assert.AreEqual("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", firstPartition.Type);
            Assert.AreEqual("EFI System", firstPartition.Name);
            Assert.IsTrue(firstPartition.IsNtfs);

            var secondPartition = sfdiskResult.PartitionTable.Partitions[1];
            Assert.AreEqual("/dev/sda2", secondPartition.Node);
            Assert.AreEqual(206848, secondPartition.Start);
            Assert.AreEqual(1953318287, secondPartition.Size);
            Assert.AreEqual("0FC63DAF-8483-4772-8E79-3D69D8477DE4", secondPartition.Type);
            Assert.AreEqual("Linux filesystem", secondPartition.Name);
            Assert.IsFalse(secondPartition.IsNtfs);
        }

        [TestMethod]
        public void SfdiskResult_InitializeWithValues_WorksCorrectly()
        {
            // Arrange
            var partitionTable = new PartitionTable
            {
                Label = "mbr",
                Device = "/dev/sdb",
                Id = "0x12345678"
            };

            // Act
            var sfdiskResult = new SfdiskResult
            {
                PartitionTable = partitionTable
            };

            // Assert
            Assert.AreEqual("mbr", sfdiskResult.PartitionTable.Label);
            Assert.AreEqual("/dev/sdb", sfdiskResult.PartitionTable.Device);
            Assert.AreEqual("0x12345678", sfdiskResult.PartitionTable.Id);
        }
    }
}
