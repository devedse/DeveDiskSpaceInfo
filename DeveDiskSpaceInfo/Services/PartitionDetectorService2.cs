using DeveDiskSpaceInfo.Models;
using DiscUtils;
using DiscUtils.Streams;
using System.Text;

namespace DeveDiskSpaceInfo.Services
{
    public static class PartitionDetectorService2
    {
        public static DDSIAnalysisResult DetectPartitionsAsync(string devicePath, OutputService logger)
        {
            var errorStringBuilder = new StringBuilder();
            try
            {
                var diskDevice = new Arsenal.ImageMounter.IO.Devices.DiskDevice(devicePath, FileAccess.Read);
                using var diskDeviceStream = diskDevice.GetRawDiskStream();

                var rawDisk = new DiscUtils.Raw.Disk(diskDeviceStream, Ownership.None);

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
                            if (partition.Open() != null)
                            {
                                using var partitionStream = partition.Open();
                                var fileSystemInfo = FileSystemManager.DetectFileSystems(partitionStream);

                                foreach (var fs in fileSystemInfo)
                                {
                                    var fileSystem = fs.Open(partitionStream);

                                    var ddsiFileSystem = new DDSIFileSystem
                                    {
                                        VolumeId = fileSystem.VolumeId,
                                        VolumeLabel = fileSystem.VolumeLabel,
                                        FriendlyName = fileSystem.FriendlyName,
                                        Size = fileSystem.Size,
                                        UsedSpace = fileSystem.UsedSpace,
                                        AvailableSpace = fileSystem.AvailableSpace,
                                        CanWrite = fileSystem.CanWrite,
                                        RootPath = fileSystem.Root.FullName
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
                        catch (Exception)
                        {
                            logger.WriteError($"Failed to open partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector}");
                            errorStringBuilder.AppendLine($"Failed to open partition {partition.TypeAsString} at sectors {partition.FirstSector}-{partition.LastSector}");
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
            catch (Exception)
            {
                logger.WriteError($"Failed to detect partitions on device {devicePath}");
                errorStringBuilder.AppendLine($"Exception occurred while detecting partitions on device {devicePath}");
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
