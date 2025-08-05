using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;

namespace DeveDiskSpaceInfo.Tests.Services
{
    [TestClass]
    public class OutputServiceTests
    {
        [TestMethod]
        public void OutputService_Constructor_AcceptsShowOptions()
        {
            // Arrange
            var jsonOptions = new ShowOptions { JsonOutput = true };
            var textOptions = new ShowOptions { JsonOutput = false };

            // Act & Assert - should not throw
            var jsonService = new OutputService(jsonOptions);
            var textService = new OutputService(textOptions);

            Assert.IsNotNull(jsonService);
            Assert.IsNotNull(textService);
        }
    }
}