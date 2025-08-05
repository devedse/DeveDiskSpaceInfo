namespace DeveDiskSpaceInfo.Services
{
    public static partial class NtfsAnalyzerService
    {
        // Simple SubStream implementation for partition access
        private sealed class SubStream : Stream
        {
            private readonly Stream _baseStream;
            private readonly long _startOffset;
            private readonly long _length;
            private long _position;

            public SubStream(Stream baseStream, long startOffset, long length)
            {
                _baseStream = baseStream;
                _startOffset = startOffset;
                _length = length;
                _position = 0;
            }

            public override bool CanRead => _baseStream.CanRead;
            public override bool CanSeek => _baseStream.CanSeek;
            public override bool CanWrite => false;
            public override long Length => _length;
            public override long Position
            {
                get => _position;
                set => Seek(value, SeekOrigin.Begin);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_position >= _length) return 0;

                long availableBytes = Math.Min(count, _length - _position);
                _baseStream.Seek(_startOffset + _position, SeekOrigin.Begin);
                int bytesRead = _baseStream.Read(buffer, offset, (int)availableBytes);
                _position += bytesRead;
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        _position = Math.Max(0, Math.Min(_length, offset));
                        break;
                    case SeekOrigin.Current:
                        _position = Math.Max(0, Math.Min(_length, _position + offset));
                        break;
                    case SeekOrigin.End:
                        _position = Math.Max(0, Math.Min(_length, _length + offset));
                        break;
                }
                return _position;
            }

            public override void Flush() { }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
