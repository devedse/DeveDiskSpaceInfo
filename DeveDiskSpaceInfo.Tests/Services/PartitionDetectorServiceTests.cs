using DeveDiskSpaceInfo.Models;
using DeveDiskSpaceInfo.Services;
using DiscUtils;
using DiscUtils.Ntfs;
using DiscUtils.Partitions;
using DiscUtils.Streams;
using System.IO.Compression;
using System.Text.Json;

namespace DeveDiskSpaceInfo.Tests.Services
{
    [TestClass]
    public class PartitionDetectorServiceTests
    {
        private const long FirstSector = 2048;
        private const long VolumeBytes = 16L * 1024 * 1024;

        private static readonly OutputService Quiet = new(new ShowOptions { JsonOutput = true });

        [ClassInitialize]
        public static void SetupFileSystems(TestContext _) => DiscUtils.FileSystems.SetupHelper.SetupFileSystems();

        [TestMethod]
        public void NtfsShutDownCleanly_ReportsACleanJournal()
        {
            using var disk = CreateDisk(partition =>
                NtfsFileSystem.Format(partition, "Fresh", Geometry.FromCapacity(VolumeBytes), FirstSector, VolumeBytes / 512).Dispose());

            var result = PartitionDetectorService2.DetectPartitions(disk, "/dev/test", Quiet);

            var fileSystem = SingleFileSystem(result);
            Assert.AreEqual("Fresh", fileSystem.VolumeLabel);
            Assert.IsNotNull(fileSystem.Journal);
            Assert.AreEqual("Clean", fileSystem.Journal.Status);
            Assert.AreEqual(0, fileSystem.Journal.TransactionsToUndo);
            Assert.IsFalse(fileSystem.Journal.ReplayedInMemory);
        }

        [TestMethod]
        public void NtfsNotShutDownCleanly_IsReadAfterAnInMemoryRecovery()
        {
            // A synthetic crash (from DevePXEBoot's tests): an unfinished transaction whose change already reached the
            // disk, so recovery rolls it back. DiscUtils does that in memory only; the disk is never written.
            var volume = Gunzip("crashed-undo.ntfs.gz");
            using var disk = CreateDisk(partition => partition.Write(volume));
            var before = Hash(disk);

            var result = PartitionDetectorService2.DetectPartitions(disk, "/dev/test", Quiet);

            var journal = SingleFileSystem(result).Journal;
            Assert.IsNotNull(journal);
            Assert.AreEqual("NeedsReplay", journal.Status);
            Assert.IsGreaterThan(0, journal.TransactionsToUndo);
            Assert.IsGreaterThan(0, journal.PagesChanged);
            Assert.IsTrue(journal.ReplayedInMemory);
            Assert.IsFalse(journal.CanReplayPersistently);
            Assert.AreEqual(before, Hash(disk), "Analyzing must never write to the disk");
        }

        [TestMethod]
        public void Json_HasTheJournalOfEachNtfsVolume()
        {
            using var disk = CreateDisk(partition =>
                NtfsFileSystem.Format(partition, "Fresh", Geometry.FromCapacity(VolumeBytes), FirstSector, VolumeBytes / 512).Dispose());
            var result = PartitionDetectorService2.DetectPartitions(disk, "/dev/test", Quiet);

            var json = JsonSerializer.Serialize(new List<DDSIAnalysisResult> { result }, SfdiskJsonContext.Default.ListDDSIAnalysisResult);

            using var document = JsonDocument.Parse(json);
            var journal = document.RootElement[0].GetProperty("Disk").GetProperty("Partitions")[0]
                .GetProperty("FileSystems")[0].GetProperty("Journal");
            Assert.AreEqual("Clean", journal.GetProperty("Status").GetString());
            Assert.AreEqual(0, journal.GetProperty("TransactionsToUndo").GetInt32());
        }

        [TestMethod]
        public async Task ImageFile_IsOpenedReadOnlyWithItsOwnLength()
        {
            var path = Path.Combine(Path.GetTempPath(), $"ddsi-{Guid.NewGuid():N}.img");
            try
            {
                using (var disk = CreateDisk(partition =>
                    NtfsFileSystem.Format(partition, "File", Geometry.FromCapacity(VolumeBytes), FirstSector, VolumeBytes / 512).Dispose()))
                using (var file = File.Create(path))
                {
                    disk.CopyTo(file);
                }

                File.SetAttributes(path, FileAttributes.ReadOnly);
                var result = await PartitionDetectorService2.DetectPartitionsAsync(path, Quiet);

                Assert.AreEqual("File", SingleFileSystem(result).VolumeLabel);
                Assert.AreEqual(new FileInfo(path).Length, result.Disk!.Capacity);
            }
            finally
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(4)]
        public async Task SeveralDevices_AreAnalyzedInParallel_AndKeepTheirOrder(int parallelism)
        {
            var dir = Directory.CreateTempSubdirectory("ddsi-batch-");
            try
            {
                var paths = new List<string>();
                for (var i = 0; i < 6; i++)
                {
                    var path = Path.Combine(dir.FullName, $"disk{i}.img");
                    var label = $"Disk{i}";
                    using (var disk = CreateDisk(partition =>
                        NtfsFileSystem.Format(partition, label, Geometry.FromCapacity(VolumeBytes), FirstSector, VolumeBytes / 512).Dispose()))
                    using (var file = File.Create(path))
                    {
                        disk.CopyTo(file);
                    }

                    paths.Add(path);
                }

                // The fifth device can't be flushed, so it's left out (as before); the others keep their order.
                var flushFails = paths[4];
                var results = await Program.AnalyzeDevicesAsync(paths, parallelism, Quiet,
                    path => path == flushFails ? throw new InvalidOperationException("no blockdev") : Task.CompletedTask);

                CollectionAssert.AreEqual(paths.Where(p => p != flushFails).ToList(), results.Select(r => r.DevicePath).ToList());
                CollectionAssert.AreEqual(new[] { "Disk0", "Disk1", "Disk2", "Disk3", "Disk5" }, results.Select(r => SingleFileSystem(r).VolumeLabel).ToList());
            }
            finally
            {
                dir.Delete(recursive: true);
            }
        }

        [TestMethod]
        public async Task MissingDevice_IsAFailedResultWithTheReason()
        {
            var result = await PartitionDetectorService2.DetectPartitionsAsync(Path.Combine(Path.GetTempPath(), "does-not-exist.img"), Quiet);

            Assert.IsFalse(result.Success);
            Assert.Contains("does-not-exist.img", result.Error!);
        }

        [TestMethod]
        [DataRow("Clean", 0, 0, null, "Clean (shut down cleanly)")]
        [DataRow("NothingToReplay", 0, 0, null, "Not shut down cleanly, nothing lost (every change reached the disk)")]
        [DataRow("NeedsReplay", 23, 0, null, "Not shut down cleanly: 23 page(s) recovered in memory")]
        [DataRow("NeedsReplay", 5, 2, null, "Not shut down cleanly: 2 unfinished change(s) shown rolled back, 5 page(s) recovered in memory")]
        [DataRow("Unsupported", 0, 0, "the volume belongs to a hibernated Windows", "Unsupported: the volume belongs to a hibernated Windows")]
        public void TextOutput_DescribesTheJournal(string status, int pages, int undo, string? detail, string expected)
        {
            var journal = new DDSINtfsJournal { Status = status, PagesChanged = pages, TransactionsToUndo = undo, Detail = detail };

            Assert.AreEqual(expected, OutputFormattingService.DescribeJournal(journal));
        }

        private static DDSIFileSystem SingleFileSystem(DDSIAnalysisResult result)
        {
            Assert.IsTrue(result.Success, result.Error);
            Assert.IsTrue(string.IsNullOrEmpty(result.Warning), result.Warning);
            Assert.IsNotNull(result.Disk);
            Assert.HasCount(1, result.Disk.Partitions);
            Assert.HasCount(1, result.Disk.Partitions[0].FileSystems);
            return result.Disk.Partitions[0].FileSystems[0];
        }

        /// <summary>An MBR disk in memory with one NTFS partition of <see cref="VolumeBytes"/>, filled by <paramref name="fill"/>.</summary>
        private static MemoryStream CreateDisk(Action<Stream> fill)
        {
            var stream = new MemoryStream();
            using (var disk = DiscUtils.Raw.Disk.Initialize(stream, Ownership.None, (FirstSector * 512) + VolumeBytes + (1024 * 1024)))
            {
                var table = BiosPartitionTable.Initialize(disk);
                table.CreatePrimaryBySector(FirstSector, FirstSector + (VolumeBytes / 512) - 1, BiosPartitionTypes.Ntfs, false);
                using var partition = disk.Partitions!.Partitions[0].Open();
                fill(partition);
            }

            stream.Position = 0;
            return stream;
        }

        private static byte[] Gunzip(string name)
        {
            using var gzip = new GZipStream(File.OpenRead(Path.Combine(AppContext.BaseDirectory, "TestData", name)), CompressionMode.Decompress);
            using var volume = new MemoryStream();
            gzip.CopyTo(volume);
            return volume.ToArray();
        }

        private static string Hash(MemoryStream disk) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(disk.ToArray()));
    }
}
