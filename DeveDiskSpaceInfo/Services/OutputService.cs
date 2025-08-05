using DeveDiskSpaceInfo.Models;

namespace DeveDiskSpaceInfo.Services
{
    public class OutputService
    {
        private readonly bool _isJsonMode;

        public OutputService(ShowOptions options)
        {
            _isJsonMode = options.JsonOutput;
        }

        public void WriteLine(string message)
        {
            if (!_isJsonMode)
            {
                Console.WriteLine(message);
            }
        }

        public void WriteError(string error)
        {
            if (!_isJsonMode)
            {
                Console.WriteLine($"❌ {error}");
            }
        }

        // Debug method to check the internal state
        public bool IsJsonMode => _isJsonMode;
    }
}