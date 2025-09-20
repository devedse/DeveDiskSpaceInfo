# DeveDiskSpaceInfo

This is a project to read the actual used diskspace of disks on linux without mounting them

## Installation

### Download Pre-built Binary (Recommended)

Download the latest release directly:

```bash
# Download the binary
wget https://github.com/devedse/DeveDiskSpaceInfo/releases/latest/download/devediskspaceinfo

# Make it executable
chmod +x devediskspaceinfo

# Move to a directory in your PATH (optional)
sudo mv devediskspaceinfo /usr/local/bin/
```

### Alternative Installation Methods

- **Manual download**: Visit the [releases page](https://github.com/devedse/DeveDiskSpaceInfo/releases) to download other versions
- **Build from source**: Clone this repository and run `dotnet publish` with your preferred configuration

## Usage

DeveDiskSpaceInfo provides a command-line interface for analyzing disk space usage on Linux devices without mounting them. The tool supports NTFS partition analysis and provides both human-readable and JSON output formats.

### Syntax

```bash
DeveDiskSpaceInfo <device> [options]
```

### Arguments

- `device` (required) - Path to the device to analyze (e.g., `/dev/sdb`, `/dev/nvme0n1p1`)

### Options

- `-j, --json` - Output results in JSON format for programmatic consumption
- `--help` - Display help screen with usage information
- `--version` - Display version information

### Examples

```bash
# Analyze a device with human-readable output
DeveDiskSpaceInfo /dev/sdb

# Get JSON output for programmatic use
DeveDiskSpaceInfo /dev/sdb --json

# Analyze a complex device path
DeveDiskSpaceInfo /dev/disk/by-uuid/12345-67890 --json

# Display help
DeveDiskSpaceInfo --help
```

### JSON Output

When using the `--json` flag, the tool outputs structured data using the new DDSI (DeveDiskSpaceInfo) model structure:

```json
[
  {
    "devicePath": "/dev/sdb",
    "success": true,
    "error": null,
    "warning": null,
    "disk": {
      "biosGeometry": {
        "cylinders": 26108,
        "headsPerCylinder": 255,
        "sectorsPerTrack": 63,
        "bytesPerSector": 512
      },
      "blockSize": 512,
      "canWrite": false,
      "capacity": 214748364800,
      "diskClass": "HardDisk",
      "diskTypeInfo": {
        "name": "Raw",
        "variant": "",
        "canBeHardDisk": true,
        "deterministicGeometry": false,
        "preservesBootSector": true,
        "supportsSparseExtents": false
      },
      "geometry": {
        "cylinders": 26108,
        "headsPerCylinder": 255,
        "sectorsPerTrack": 63,
        "bytesPerSector": 512
      },
      "isPartitioned": true,
      "sectorSize": 512,
      "signature": 0,
      "partitions": [
        {
          "biosType": 238,
          "firstSector": 2048,
          "lastSector": 411647,
          "sectorCount": 409600,
          "guidType": "c12a7328-f81f-11d2-ba4b-00a0c93ec93b",
          "typeAsString": "EFI System",
          "volumeType": "None",
          "sectorSize": 512,
          "fileSystems": [
            {
              "volumeId": 1234567890,
              "volumeLabel": "SYSTEM",
              "friendlyName": "FAT32",
              "size": 209715200,
              "usedSpace": 33554432,
              "availableSpace": 176160768,
              "canWrite": false,
              "rootPath": "\\"
            }
          ],
          "size": 209715200,
          "usedSpace": 33554432,
          "availableSpace": 176160768
        },
        {
          "biosType": 0,
          "firstSector": 411648,
          "lastSector": 444415,
          "sectorCount": 32768,
          "guidType": "e3c9e316-0b5c-4db8-817d-f92df00215ae",
          "typeAsString": "Microsoft Reserved",
          "volumeType": "None",
          "sectorSize": 512,
          "fileSystems": [],
          "size": 0,
          "usedSpace": 0,
          "availableSpace": 0
        },
        {
          "biosType": 7,
          "firstSector": 444416,
          "lastSector": 418002943,
          "sectorCount": 417558528,
          "guidType": "ebd0a0a2-b9e5-4433-87c0-68b6b72699c7",
          "typeAsString": "Microsoft Basic Data",
          "volumeType": "None",
          "sectorSize": 512,
          "fileSystems": [
            {
              "volumeId": 987654321,
              "volumeLabel": "Windows",
              "friendlyName": "NTFS",
              "size": 213789966336,
              "usedSpace": 37674909696,
              "availableSpace": 176115056640,
              "canWrite": false,
              "rootPath": "\\"
            }
          ],
          "size": 213789966336,
          "usedSpace": 37674909696,
          "availableSpace": 176115056640
        }
      ],
      "devicePath": "/dev/sdb"
    }
  }
]
```

### Requirements

- Linux operating system
- Root privileges (sudo) for device access

**Note**: The pre-built binary is self-contained and does not require .NET runtime installation.

### Permission Requirements

Most device analysis operations require root privileges. Run with `sudo` if you encounter permission denied errors:

```bash
sudo DeveDiskSpaceInfo /dev/sdb --json
```

## Build status

| GitHubActions Builds |
|:--------------------:|
| [![GitHubActions Builds](https://github.com/devedse/DeveDiskSpaceInfo/workflows/GitHubActionsBuilds/badge.svg)](https://github.com/devedse/DeveDiskSpaceInfo/actions/workflows/githubactionsbuilds.yml) |

## Code Coverage Status

| CodeCov |
|:-------:|
| [![codecov](https://codecov.io/gh/devedse/DeveDiskSpaceInfo/branch/master/graph/badge.svg)](https://codecov.io/gh/devedse/DeveDiskSpaceInfo) |

## Code Quality Status

| SonarQube |
|:---------:|
| [![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=DeveDiskSpaceInfo&metric=alert_status)](https://sonarcloud.io/dashboard?id=DeveDiskSpaceInfo) |
