using System.Text.Json;
using SimpleExec;
using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Services
{
    public static class PartitionDetectorService
    {
        public static async Task<PartitionTable?> DetectPartitionsAsync(string devicePath, OutputService logger)
        {
            logger.WriteLine("Detecting partitions using sfdisk...");
            
            try
            {
                var (output, _) = await Command.ReadAsync("sfdisk", $"--json {devicePath}");
                
                // Use source generation for trimming compatibility
                var result = JsonSerializer.Deserialize<SfdiskResult>(output, SfdiskJsonContext.Default.SfdiskResult);
                return result?.PartitionTable;
            }
            catch (Exception ex)
            {
                logger.WriteError($"Failed to detect partitions using sfdisk: {ex.Message}");
                return null;
            }
        }
    }
}
