using System.Globalization;

namespace DeveDiskSpaceInfo.Helpers
{
    public static class ByteFormatHelper
    {
        public static string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int suffixIndex = 0;
            double size = Math.Abs(bytes); // Use absolute value for calculations
            bool isNegative = bytes < 0;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            string sign = isNegative ? "-" : "";
            return $"{sign}{size.ToString("F2", CultureInfo.InvariantCulture)} {suffixes[suffixIndex]}";
        }
    }
}
