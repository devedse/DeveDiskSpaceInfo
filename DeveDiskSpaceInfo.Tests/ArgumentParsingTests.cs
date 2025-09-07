using CommandLine;
using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests
{
    [TestClass]
    public class ArgumentParsingTests
    {
        private static ShowOptions? ParseArguments(string[] args)
        {
            ShowOptions? result = null;
            Parser.Default.ParseArguments<ShowOptions>(args)
                .WithParsed(opts => result = opts);
            return result;
        }

        [TestMethod]
        public void ParseArguments_NoArguments_ReturnsNull()
        {
            // Act
            var result = ParseArguments(new string[0]);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseArguments_SingleDevicePath_SetsDevicePaths()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.IsFalse(result.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_MultipleDevicePaths_SetsAllDevicePaths()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb", "/dev/sdc" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.AreEqual("/dev/sdc", result.DevicePaths.Last());
            Assert.IsFalse(result.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_DevicePathAndJson_SetsBothOptions()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb", "--json" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.IsTrue(result.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_JsonShortFlag_SetsJsonOutput()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb", "-j" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.IsTrue(result.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_JsonAndDevicePath_SetsBothOptions()
        {
            // Act
            var result = ParseArguments(new[] { "--json", "/dev/sdb" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.IsTrue(result.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_ValidComplexDevicePath_Works()
        {
            // Act
            var result1 = ParseArguments(new[] { "/dev/disk/by-uuid/12345", "--json" });
            var result2 = ParseArguments(new[] { "/dev/nvme0n1p1" });

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreEqual("/dev/disk/by-uuid/12345", result1.DevicePaths.First());
            Assert.IsTrue(result1.JsonOutput);

            Assert.IsNotNull(result2);
            Assert.AreEqual("/dev/nvme0n1p1", result2.DevicePaths.First());
            Assert.IsFalse(result2.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_EmptyDevicePath_ReturnsValid()
        {
            // Act
            var result = ParseArguments(new[] { "" });

            // Assert
            // CommandLineParser accepts empty strings, but our validation will catch it
            Assert.IsNotNull(result);
            Assert.AreEqual("", result.DevicePaths.First());
        }

        [TestMethod]
        public void ParseArguments_HelpFlag_ReturnsNull()
        {
            // Act
            var result1 = ParseArguments(new[] { "--help" });
            var result2 = ParseArguments(new[] { "-h" });

            // Assert
            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void ParseArguments_InvalidFlag_ReturnsNull()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb", "--invalid-flag" });

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseArguments_JsonFlagVariations_Work()
        {
            // Act
            var result1 = ParseArguments(new[] { "/dev/sdb", "--json" });
            var result2 = ParseArguments(new[] { "/dev/sdb", "-j" });

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsTrue(result1.JsonOutput);

            Assert.IsNotNull(result2);
            Assert.IsTrue(result2.JsonOutput);
        }

        [TestMethod]
        public void ParseArguments_MultipleDevicePathsWithJson_WorksCorrectly()
        {
            // Act
            var result = ParseArguments(new[] { "/dev/sdb", "/dev/sdc", "--json" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.DevicePaths.Count());
            Assert.AreEqual("/dev/sdb", result.DevicePaths.First());
            Assert.AreEqual("/dev/sdc", result.DevicePaths.Last());
            Assert.IsTrue(result.JsonOutput);
        }
    }
}