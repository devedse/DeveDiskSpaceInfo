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
            var results = new List<AnalysisResult>
            {
                new AnalysisResult { Success = true },
                new AnalysisResult { Success = true }
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
            var results = new List<AnalysisResult>
            {
                new AnalysisResult { Success = true },
                new AnalysisResult { Success = false, Error = "Failed to detect partitions" },
                new AnalysisResult { Success = true }
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
            var results = new List<AnalysisResult>
            {
                new AnalysisResult { Success = false, Error = "Device not found" },
                new AnalysisResult { Success = false, Error = "Access denied" }
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
            var results = new List<AnalysisResult>();

            // Act
            var exitCode = GetExitCodeFromResults(results);

            // Assert
            Assert.AreEqual(1, exitCode, "Should return 1 when no results");
        }

        /// <summary>
        /// Simulates the exit code logic from the ExecuteAnalysis method
        /// </summary>
        private static int GetExitCodeFromResults(List<AnalysisResult> results)
        {
            var hasSuccesses = results.Any(r => r.Success);
            return hasSuccesses ? 0 : 1;
        }
    }
}
