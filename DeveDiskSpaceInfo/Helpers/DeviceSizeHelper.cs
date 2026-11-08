using SimpleExec;
using System.Globalization;

namespace DeveDiskSpaceInfo.Helpers
{
    public static class DeviceSizeHelper
    {
        /// <summary>
        /// The size of a device or image file in bytes. A regular file reports its own length; a block device reports 0,
        /// so its size comes from <c>blockdev --getsize64</c> (the tool already needs blockdev to flush the device's buffers).
        /// Symlinks are followed first: LVM volumes are links (/dev/vg/lv -> ../dm-5), and a link's own length is the
        /// length of its target's name.
        /// </summary>
        public static async Task<long> GetSizeAsync(string devicePath)
        {
            var fileLength = GetFileLength(devicePath);
            if (fileLength > 0)
            {
                return fileLength;
            }

            var (output, _) = await Command.ReadAsync("blockdev", $"--getsize64 {devicePath}");
            return ParseBlockdevSize(output);
        }

        /// <summary>The length of the file the path finally points to (0 for a block device).</summary>
        internal static long GetFileLength(string path)
        {
            var info = new FileInfo(path);
            if (info.LinkTarget != null && info.ResolveLinkTarget(returnFinalTarget: true) is FileInfo target)
            {
                info = target;
            }

            return info.Length;
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
