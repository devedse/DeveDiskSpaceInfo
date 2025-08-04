using DeveDiskSpaceInfo.Helpers;

namespace DeveDiskSpaceInfo.Tests.Helpers
{
    [TestClass]
    public sealed class ByteFormatHelperTests
    {
        [TestMethod]
        public void FormatBytes_ZeroBytes_ReturnsZeroBytesString()
        {
            // Arrange
            long bytes = 0;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("0.00 B", result);
        }

        [TestMethod]
        public void FormatBytes_OneBytes_ReturnsOneBytesString()
        {
            // Arrange
            long bytes = 1;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 B", result);
        }

        [TestMethod]
        public void FormatBytes_1023Bytes_ReturnsBytesString()
        {
            // Arrange
            long bytes = 1023;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1023.00 B", result);
        }

        [TestMethod]
        public void FormatBytes_1024Bytes_ReturnsOneKB()
        {
            // Arrange
            long bytes = 1024;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 KB", result);
        }

        [TestMethod]
        public void FormatBytes_1536Bytes_ReturnsOneAndHalfKB()
        {
            // Arrange
            long bytes = 1536; // 1024 + 512

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.50 KB", result);
        }

        [TestMethod]
        public void FormatBytes_OneMB_ReturnsOneMBString()
        {
            // Arrange
            long bytes = 1024 * 1024; // 1 MB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 MB", result);
        }

        [TestMethod]
        public void FormatBytes_OneGB_ReturnsOneGBString()
        {
            // Arrange
            long bytes = 1024L * 1024 * 1024; // 1 GB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 GB", result);
        }

        [TestMethod]
        public void FormatBytes_OneTB_ReturnsOneTBString()
        {
            // Arrange
            long bytes = 1024L * 1024 * 1024 * 1024; // 1 TB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 TB", result);
        }

        [TestMethod]
        public void FormatBytes_OnePB_ReturnsOnePBString()
        {
            // Arrange
            long bytes = 1024L * 1024 * 1024 * 1024 * 1024; // 1 PB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("1.00 PB", result);
        }

        [TestMethod]
        public void FormatBytes_LargerThanPB_ReturnsPBString()
        {
            // Arrange
            long bytes = 1024L * 1024 * 1024 * 1024 * 1024 * 2; // 2 PB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("2.00 PB", result);
        }

        [TestMethod]
        public void FormatBytes_2Point5GB_ReturnsCorrectFormat()
        {
            // Arrange
            long bytes = (long)(2.5 * 1024 * 1024 * 1024); // 2.5 GB

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("2.50 GB", result);
        }

        [TestMethod]
        public void FormatBytes_NegativeBytes_ReturnsNegativeString()
        {
            // Arrange
            long bytes = -1024;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.AreEqual("-1.00 KB", result);
        }

        [TestMethod]
        public void FormatBytes_MaxLongValue_ReturnsLargePBValue()
        {
            // Arrange
            long bytes = long.MaxValue;

            // Act
            string result = ByteFormatHelper.FormatBytes(bytes);

            // Assert
            Assert.IsTrue(result.EndsWith(" PB"));
            Assert.IsTrue(result.Contains("8192.00")); // Approximate value for long.MaxValue in PB
        }
    }
}
