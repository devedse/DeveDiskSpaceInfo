using DeveDiskSpaceInfo.Helpers;
using DeveDiskSpaceInfo.Models;
using DiscUtils;
using DiscUtils.Ntfs;
using DiscUtils.Streams;
using System.Text;

namespace DeveDiskSpaceInfo.Services
{
    public static class PartitionDetectorService2
    {
        /// <summary>Opens the device (or image file) read-only and analyzes it. Nothing is ever written to it.</summary>
        public static async Task<DDSIAnalysisResult> DetectPartitionsAsync(string devicePath, OutputService logger)
        {
            try
            {
                var size = await DeviceSizeHelper.GetSizeAsync(devicePath);
                using var device = new FileStream(devicePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var deviceStream = new FixedLengthReadStream(device, size);
                return DetectPartitions(deviceStream, devicePath, logger);
            }
            catch (Exception ex)
            {
                logger.WriteError($"Failed to open device {devicePath}: {ex.Message}");
                return new DDSIAnalysisResult
                {
                    Success = false,
                    DevicePath = devicePath,
                    Error = $"Failed to open device {devicePath}: {ex.Message}"
                };
            }
        }

        public static DDSIAnalysisResult DetectPartitions(Stream deviceStream, string devicePath, OutputService logger)
        {
            var errorStringBuilder = new StringBuilder();
            try
            {
                // Always read-only, whatever stream is passed: the tool never writes, and DiscUtils opens an NTFS volume
                // on a read-only stream with an in-memory replay of its journal (a writable open would refuse it).
                var readOnlyStream = deviceStream.CanWrite ? new FixedLengthReadStream(deviceStream, deviceStream.Length) : deviceStream;
                var rawDisk = new DiscUtils.Raw.Disk(readOnlyStream, Ownership.None);

                var ddsiDisk = new DDSIDisk
                {
                    DevicePath = devicePath,
                    BiosGeometry = ConvertGeometry(rawDisk.BiosGeometry),
                    BlockSize = rawDisk.BlockSize,
                    CanWrite = rawDisk.CanWrite,
                    Capacity = rawDisk.Capacity,
                    DiskClass = rawDisk.DiskClass,
                    DiskTypeInfo = rawDisk.DiskTypeInfo != null ? ConvertVirtualDiskTypeInfo(rawDisk.DiskTypeInfo) : null,
                    Geometry = rawDisk.Geometry.HasValue ? ConvertGeometry(rawDisk.Geometry.Value) : null,
                    IsPartitioned = rawDisk.IsPartitioned,
                    SectorSize = rawDisk.SectorSize,
                    Signature = rawDisk.Signature
                };

                if (rawDisk.IsPartitioned && rawDisk.Partitions != null)
                {
                    foreach (var partition in rawDisk.Partitions.Partitions)
                    {
                        var ddsiPartition = new DDSIPartition
                        {
                            BiosType = partition.BiosType,
                            FirstSector = partition.FirstSector,
                            LastSector = partition.LastSector,
                            SectorCount = partition.SectorCount,
                            GuidType = partition.GuidType,
                            TypeAsString = partition.TypeAsString,
                            VolumeType = partition.VolumeType,
                            SectorSize = rawDisk.SectorSize
                        };

                        try
                        {
                            using var partitionStream = partition.Open();
                            if (partitionStream != null)
                            {
                                var fileSystemInfo = FileSystemManager.DetectFileSystems(partitionStream);

                                foreach (var fs in fileSystemInfo)
                                {
                                    // Read-only: an NTFS volume Windows didn't shut down cleanly is shown after an in-memory
                                    // replay of its journal, so the sizes match what Windows shows after its own recovery.
                                    using var fileSystem = fs.Open(partitionStream);

                                    var ddsiFileSystem = new DDSIFileSystem
                                    {
                                        VolumeId = fileSystem.VolumeId,
                                        VolumeLabel = fileSystem.VolumeLabel ?? string.Empty,
                                        FriendlyName = fileSystem.FriendlyName,
                                        Size = fileSystem.Size,
                                        UsedSpace = fileSystem.UsedSpace,
                                        AvailableSpace = fileSystem.AvailableSpace,
                                        CanWrite = fileSystem.CanWrite,
                                        RootPath = fileSystem.Root.FullName,
                                        Journal = fileSystem is NtfsFileSystem { JournalInfo: { } journal } ? DDSINtfsJournal.From(journal) : null
                                    };

                                    ddsiPartition.FileSystems.Add(ddsiFileSystem);
                                }
                            }
                            else
                            {
                                logger.WriteError($"Partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector} has no accessible stream.");
                                errorStringBuilder.AppendLine($"Partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector} has no accessible stream.");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.WriteError($"Failed to open partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector}: {ex.Message}");
                            errorStringBuilder.AppendLine($"Failed to open partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector}: {ex.Message}");
                        }

                        ddsiDisk.Partitions.Add(ddsiPartition);
                    }
                }

                return new DDSIAnalysisResult
                {
                    Success = true,
                    DevicePath = devicePath,
                    Warning = errorStringBuilder.ToString(),
                    Disk = ddsiDisk
                };
            }
            catch (Exception ex)
            {
                logger.WriteError($"Failed to detect partitions on device {devicePath}: {ex.Message}");
                errorStringBuilder.AppendLine($"Exception occurred while detecting partitions on device {devicePath}: {ex.Message}");
                return new DDSIAnalysisResult
                {
                    Success = false,
                    DevicePath = devicePath,
                    Error = errorStringBuilder.ToString()
                };
            }
        }

        private static DDSIGeometry ConvertGeometry(DiscUtils.Geometry geometry)
        {
            return new DDSIGeometry
            {
                BytesPerSector = geometry.BytesPerSector,
                Capacity = geometry.Capacity,
                Cylinders = geometry.Cylinders,
                HeadsPerCylinder = geometry.HeadsPerCylinder,
                IsBiosAndIdeSafe = geometry.IsBiosAndIdeSafe,
                IsBiosSafe = geometry.IsBiosSafe,
                IsIdeSafe = geometry.IsIdeSafe,
                SectorsPerTrack = geometry.SectorsPerTrack,
                TotalSectorsLong = geometry.TotalSectorsLong
            };
        }

        private static DDSIVirtualDiskTypeInfo ConvertVirtualDiskTypeInfo(DiscUtils.VirtualDiskTypeInfo typeInfo)
        {
            return new DDSIVirtualDiskTypeInfo
            {
                CanBeHardDisk = typeInfo.CanBeHardDisk,
                DeterministicGeometry = typeInfo.DeterministicGeometry,
                Name = typeInfo.Name ?? string.Empty,
                PreservesBiosGeometry = typeInfo.PreservesBiosGeometry,
                Variant = typeInfo.Variant ?? string.Empty
            };
        }
    }
}
