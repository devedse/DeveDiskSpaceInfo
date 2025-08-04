using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests.Models
{
    [TestClass]
    public class ShowOptionsTests
    {
        [TestMethod]
        public void ShowOptions_DefaultValues_AreCorrect()
        {
            // Act
            var options = new ShowOptions();

            // Assert
            Assert.AreEqual(string.Empty, options.DevicePath);
            Assert.IsFalse(options.JsonOutput);
        }

        [TestMethod]
        public void ShowOptions_CanSetProperties()
        {
            // Arrange
            var options = new ShowOptions();

            // Act
            options.DevicePath = "/test/device";
            options.JsonOutput = true;

            // Assert
            Assert.AreEqual("/test/device", options.DevicePath);
            Assert.IsTrue(options.JsonOutput);
        }
    }
}