namespace DeveDiskSpaceInfo.Helpers
{
    /// <summary>
    /// A read-only view of a stream with a known length. On Linux a block device (/dev/sdb, an LVM volume) opened as a
    /// FileStream reports a length of 0, so the device size is passed in instead (see <see cref="DeviceSizeHelper"/>).
    /// </summary>
    public sealed class FixedLengthReadStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _length;
        private long _position;

        public FixedLengthReadStream(Stream inner, long length)
        {
            ArgumentNullException.ThrowIfNull(inner);
            ArgumentOutOfRangeException.ThrowIfNegative(length);
            if (!inner.CanRead || !inner.CanSeek)
            {
                throw new ArgumentException("The stream must be readable and seekable.", nameof(inner));
            }

            _inner = inner;
            _length = length;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            var remaining = _length - _position;
            if (remaining <= 0 || buffer.IsEmpty)
            {
                return 0;
            }

            if (buffer.Length > remaining)
            {
                buffer = buffer[..(int)remaining];
            }

            _inner.Position = _position;
            var read = _inner.Read(buffer);
            _position += read;
            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var target = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin)),
            };

            if (target < 0)
            {
                throw new IOException("Attempt to seek before the start of the stream.");
            }

            _position = target;
            return _position;
        }

        public override void Flush()
        {
        }

        public override void SetLength(long value) => throw new NotSupportedException("The stream is read-only.");

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException("The stream is read-only.");
    }
}
