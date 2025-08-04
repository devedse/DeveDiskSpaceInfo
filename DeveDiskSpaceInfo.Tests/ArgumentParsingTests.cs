using DeveDiskSpaceInfo.Models;
using System.Reflection;

namespace DeveDiskSpaceInfo.Tests
{
    [TestClass]
    public class ArgumentParsingTests
    {
        private static CommandLineOptions? ParseCommandLineArguments(string[] args)
        {
            // We need to access the private method via reflection
            var programType = typeof(Program);
            var method = programType.GetMethod("ParseCommandLineArguments", BindingFlags.NonPublic | BindingFlags.Static);
            if (method == null)
                throw new InvalidOperationException("ParseCommandLineArguments method not found");
            
            return (CommandLineOptions?)method.Invoke(null, new object[] { args });
        }

        [TestMethod]
        public void ParseCommandLineArguments_NoArguments_ReturnsNull()
        {
            // Act
            var result = ParseCommandLineArguments(new string[0]);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseCommandLineArguments_PositionalDeviceArgument_SetsDevicePath()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "/dev/sdb" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sdb", result.DevicePath);
            Assert.IsFalse(result.JsonOutput);
        }

        [TestMethod]
        public void ParseCommandLineArguments_PositionalDeviceAndJson_SetsBothOptions()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "/dev/sdb", "--json" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sdb", result.DevicePath);
            Assert.IsTrue(result.JsonOutput);
        }

        [TestMethod]
        public void ParseCommandLineArguments_JsonAndPositionalDevice_SetsBothOptions()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "--json", "/dev/sdb" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sdb", result.DevicePath);
            Assert.IsTrue(result.JsonOutput);
        }

        [TestMethod]
        public void ParseCommandLineArguments_NamedDeviceOption_SetsDevicePath()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "--device", "/dev/sdc" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sdc", result.DevicePath);
            Assert.IsFalse(result.JsonOutput);
        }

        [TestMethod]
        public void ParseCommandLineArguments_ShortDeviceOption_SetsDevicePath()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "-d", "/dev/sdd" });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("/dev/sdd", result.DevicePath);
            Assert.IsFalse(result.JsonOutput);
        }

        [TestMethod]
        public void ParseCommandLineArguments_MultipleDeviceSpecs_ReturnsNull()
        {
            // Act
            var result1 = ParseCommandLineArguments(new[] { "/dev/sdb", "--device", "/dev/sdc" });
            var result2 = ParseCommandLineArguments(new[] { "--device", "/dev/sdb", "/dev/sdc" });
            var result3 = ParseCommandLineArguments(new[] { "/dev/sdb", "/dev/sdc" });

            // Assert
            Assert.IsNull(result1);
            Assert.IsNull(result2);
            Assert.IsNull(result3);
        }

        [TestMethod]
        public void ParseCommandLineArguments_DeviceWithoutValue_ReturnsNull()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "--device" });

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseCommandLineArguments_HelpFlag_ReturnsNull()
        {
            // Act
            var result1 = ParseCommandLineArguments(new[] { "--help" });
            var result2 = ParseCommandLineArguments(new[] { "-h" });

            // Assert
            Assert.IsNull(result1);
            Assert.IsNull(result2);
        }

        [TestMethod]
        public void ParseCommandLineArguments_UnknownOption_ReturnsNull()
        {
            // Act
            var result = ParseCommandLineArguments(new[] { "--unknown" });

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void ParseCommandLineArguments_ComplexScenarios_WorkCorrectly()
        {
            // Test various valid combinations
            var result1 = ParseCommandLineArguments(new[] { "/dev/sdb", "--json" });
            Assert.IsNotNull(result1);
            Assert.AreEqual("/dev/sdb", result1.DevicePath);
            Assert.IsTrue(result1.JsonOutput);

            var result2 = ParseCommandLineArguments(new[] { "--json", "--device", "/dev/sdc" });
            Assert.IsNotNull(result2);
            Assert.AreEqual("/dev/sdc", result2.DevicePath);
            Assert.IsTrue(result2.JsonOutput);
        }
    }
}