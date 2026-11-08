using SimpleExec;
using System.Globalization;

namespace DeveDiskSpaceInfo.Helpers
{
    public static class DeviceSizeHelper
    {
        /// <summary>
        /// The size of a device or image file in bytes. A regular file reports its own length; a block device reports 0,
        /// so its size comes from <c>blockdev --getsize64</c> (the tool already needs blockdev to flush the device's buffers).
        /// </summary>
        public static async Task<long> GetSizeAsync(string devicePath)
        {
            var fileLength = new FileInfo(devicePath).Length;
            if (fileLength > 0)
            {
                return fileLength;
            }

            var (output, _) = await Command.ReadAsync("blockdev", $"--getsize64 {devicePath}");
            return ParseBlockdevSize(output);
        }

        internal static long ParseBlockdevSize(string output)
        {
            if (long.TryParse(output.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var size) && size > 0)
            {
                return size;
            }

            throw new IOException($"Could not read the device size from blockdev: '{output.Trim()}'");
        }
    }
}
