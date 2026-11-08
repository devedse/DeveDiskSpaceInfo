using DeveDiskSpaceInfo.Helpers;

namespace DeveDiskSpaceInfo.Tests.Helpers
{
    [TestClass]
    public class FixedLengthReadStreamTests
    {
        [TestMethod]
        public void ReportsTheGivenLength_AndStopsReadingThere()
        {
            // A block device FileStream reports length 0; the wrapper gives it its real size.
            using var inner = new MemoryStream(Enumerable.Range(0, 100).Select(i => (byte)i).ToArray());
            using var stream = new FixedLengthReadStream(inner, 60);

            Assert.AreEqual(60, stream.Length);
            stream.Seek(-10, SeekOrigin.End);
            var buffer = new byte[30];
            Assert.AreEqual(10, stream.Read(buffer, 0, buffer.Length));
            Assert.AreEqual(50, buffer[0]);
            Assert.AreEqual(0, stream.Read(buffer, 0, buffer.Length));
        }

        [TestMethod]
        public void ReadsFromItsOwnPosition_EvenIfTheInnerStreamMoved()
        {
            using var inner = new MemoryStream(Enumerable.Range(0, 100).Select(i => (byte)i).ToArray());
            using var stream = new FixedLengthReadStream(inner, 100);

            stream.Position = 40;
            inner.Position = 0;
            Assert.AreEqual(40, stream.ReadByte());
            Assert.AreEqual(41, stream.Position);
        }

        [TestMethod]
        public void IsReadOnly()
        {
            using var stream = new FixedLengthReadStream(new MemoryStream(new byte[10]), 10);

            Assert.IsFalse(stream.CanWrite);
            Assert.ThrowsExactly<NotSupportedException>(() => stream.Write(new byte[1], 0, 1));
            Assert.ThrowsExactly<NotSupportedException>(() => stream.SetLength(5));
            Assert.ThrowsExactly<IOException>(() => stream.Seek(-1, SeekOrigin.Begin));
        }

        [TestMethod]
        [DataRow("1073741824\n", 1073741824L)]
        [DataRow(" 512 ", 512L)]
        public void BlockdevSize_IsParsed(string output, long expected)
        {
            Assert.AreEqual(expected, DeviceSizeHelper.ParseBlockdevSize(output));
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("0")]
        [DataRow("blockdev: cannot open /dev/nope")]
        public void BlockdevWithoutASize_IsAnError(string output)
        {
            Assert.ThrowsExactly<IOException>(() => DeviceSizeHelper.ParseBlockdevSize(output));
        }
    }
}
