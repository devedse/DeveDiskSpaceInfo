using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Tests
{
    [TestClass]
    public class ProgramExitCodeTests
    {
        [TestMethod]
        public void ExecuteAnalysis_AllDevicesSucceed_ReturnsZero()
        {
            // Create mock results - all successful
            var results = new List<DDSIAnalysisResult>
            {
                new DDSIAnalysisResult { DevicePath = "/dev/sda", Success = true },
                new DDSIAnalysisResult { DevicePath = "/dev/sdb", Success = true }
            };

            // Act
            var exitCode = GetExitCodeFromResults(results);

            // Assert
            Assert.AreEqual(0, exitCode, "Should return 0 when all devices succeed");
        }

        [TestMethod]
        public void ExecuteAnalysis_SomeDevicesSucceed_ReturnsZero()
        {
            // Create mock results - some successful, some failed
            var results = new List<DDSIAnalysisResult>
            {
                new DDSIAnalysisResult { DevicePath = "/dev/sda", Success = true },
                new DDSIAnalysisResult { DevicePath = "/dev/sdb", Success = false, Error = "Failed to detect partitions" },
                new DDSIAnalysisResult { DevicePath = "/dev/sdc", Success = true }
            };

            // Act
            var exitCode = GetExitCodeFromResults(results);

            // Assert
            Assert.AreEqual(0, exitCode, "Should return 0 when at least one device succeeds");
        }

        [TestMethod]
        public void ExecuteAnalysis_NoDevicesSucceed_ReturnsOne()
        {
            // Create mock results - all failed
            var results = new List<DDSIAnalysisResult>
            {
                new DDSIAnalysisResult { DevicePath = "/dev/sda", Success = false, Error = "Device not found" },
                new DDSIAnalysisResult { DevicePath = "/dev/sdb", Success = false, Error = "Access denied" }
            };

            // Act
            var exitCode = GetExitCodeFromResults(results);

            // Assert
            Assert.AreEqual(1, exitCode, "Should return 1 when no devices succeed");
        }

        [TestMethod]
        public void ExecuteAnalysis_EmptyResults_ReturnsOne()
        {
            // Arrange
            var results = new List<DDSIAnalysisResult>();

            // Act
            var exitCode = GetExitCodeFromResults(results);

            // Assert
            Assert.AreEqual(1, exitCode, "Should return 1 when no results");
        }

        /// <summary>
        /// Simulates the exit code logic from the ExecuteAnalysis method
        /// </summary>
        private static int GetExitCodeFromResults(List<DDSIAnalysisResult> results)
        {
            var hasSuccesses = results.Any(r => r.Success);
            return hasSuccesses ? 0 : 1;
        }
    }
}
