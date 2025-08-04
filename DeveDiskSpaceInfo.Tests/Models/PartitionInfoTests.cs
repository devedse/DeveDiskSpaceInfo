using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public sealed class PartitionInfoTests
    {
        [TestMethod]
        public void IsNtfs_WithNtfsTypeGuid_ReturnsTrue()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7"
            };

            // Act
            bool result = partition.IsNtfs;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNtfs_WithNtfsTypeGuidLowerCase_ReturnsTrue()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Type = "ebd0a0a2-b9e5-4433-87c0-68b6b72699c7"
            };

            // Act
            bool result = partition.IsNtfs;

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void IsNtfs_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Type = "0FC63DAF-8483-4772-8E79-3D69D8477DE4" // Linux filesystem
            };

            // Act
            bool result = partition.IsNtfs;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsNtfs_WithEmptyType_ReturnsFalse()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Type = string.Empty
            };

            // Act
            bool result = partition.IsNtfs;

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SizeInBytes_WithSectorSize_ReturnsCorrectByteSize()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Size = 2048 // 2048 sectors
            };

            // Act
            long result = partition.SizeInBytes;

            // Assert
            Assert.AreEqual(1048576, result); // 2048 * 512 = 1,048,576 bytes (1MB)
        }

        [TestMethod]
        public void SizeInBytes_WithZeroSize_ReturnsZero()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Size = 0
            };

            // Act
            long result = partition.SizeInBytes;

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void StartOffsetBytes_WithStartSector_ReturnsCorrectOffset()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Start = 2048 // Start at sector 2048
            };

            // Act
            long result = partition.StartOffsetBytes;

            // Assert
            Assert.AreEqual(1048576, result); // 2048 * 512 = 1,048,576 bytes
        }

        [TestMethod]
        public void StartOffsetBytes_WithZeroStart_ReturnsZero()
        {
            // Arrange
            var partition = new PartitionInfo
            {
                Start = 0
            };

            // Act
            long result = partition.StartOffsetBytes;

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void PartitionInfo_DefaultValues_AreSetCorrectly()
        {
            // Arrange & Act
            var partition = new PartitionInfo();

            // Assert
            Assert.AreEqual(string.Empty, partition.Node);
            Assert.AreEqual(0, partition.Start);
            Assert.AreEqual(0, partition.Size);
            Assert.AreEqual(string.Empty, partition.Type);
            Assert.AreEqual(string.Empty, partition.Uuid);
            Assert.AreEqual(string.Empty, partition.Name);
            Assert.IsNull(partition.Attrs);
        }

        [TestMethod]
        public void PartitionInfo_SetProperties_WorksCorrectly()
        {
            // Arrange
            var partition = new PartitionInfo();

            // Act
            partition.Node = "/dev/sda1";
            partition.Start = 2048;
            partition.Size = 204800;
            partition.Type = "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7";
            partition.Uuid = "12345678-1234-1234-1234-123456789abc";
            partition.Name = "System";
            partition.Attrs = "LegacyBIOSBootable";

            // Assert
            Assert.AreEqual("/dev/sda1", partition.Node);
            Assert.AreEqual(2048, partition.Start);
            Assert.AreEqual(204800, partition.Size);
            Assert.AreEqual("EBD0A0A2-B9E5-4433-87C0-68B6B72699C7", partition.Type);
            Assert.AreEqual("12345678-1234-1234-1234-123456789abc", partition.Uuid);
            Assert.AreEqual("System", partition.Name);
            Assert.AreEqual("LegacyBIOSBootable", partition.Attrs);
        }
    }
}
