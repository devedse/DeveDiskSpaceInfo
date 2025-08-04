using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public class CommandLineOptionsTests
    {
        [TestMethod]
        public void CommandLineOptions_DefaultValues_AreCorrect()
        {
            // Act
            var options = new CommandLineOptions();

            // Assert
            Assert.AreEqual("/dev/iscsi_thick_vg/iscsi_devedse", options.DevicePath);
            Assert.IsFalse(options.JsonOutput);
        }

        [TestMethod]
        public void CommandLineOptions_CanSetProperties()
        {
            // Arrange
            var options = new CommandLineOptions();

            // Act
            options.DevicePath = "/test/device";
            options.JsonOutput = true;

            // Assert
            Assert.AreEqual("/test/device", options.DevicePath);
            Assert.IsTrue(options.JsonOutput);
        }
    }
}