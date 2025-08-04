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

When using the `--json` flag, the tool outputs structured data suitable for programmatic consumption:

```json
{
  "success": true,
  "error": null,
  "device_path": "/dev/iscsi_thick_vg/iscsi_devedse",
  "partition_table": {
    "device": "/dev/iscsi_thick_vg/iscsi_devedse",
    "label": "gpt",
    "sector_size": 512,
    "first_lba": 34,
    "last_lba": 419430366,
    "total_sectors": 419430333,
    "total_size_bytes": 214748330496,
    "partitions": [
      {
        "node": "/dev/iscsi_thick_vg/iscsi_devedse1",
        "name": "Basic data partition",
        "type": "C12A7328-F81F-11D2-BA4B-00A0C93EC93B",
        "size_bytes": 209715200,
        "start": 2048,
        "start_offset_bytes": 1048576,
        "is_ntfs": false,
        "attrs": "GUID:63"
      },
      {
        "node": "/dev/iscsi_thick_vg/iscsi_devedse2",
        "name": "Microsoft reserved partition",
        "type": "E3C9E316-0B5C-4DB8-817D-F92DF00215AE",
        "size_bytes": 16777216,
        "start": 411648,
        "start_offset_bytes": 210763776,
        "is_ntfs": false,
        "attrs": "GUID:63"
      },
      {
        "node": "/dev/iscsi_thick_vg/iscsi_devedse3",
        "name": "Basic data partition",
        "type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7",
        "size_bytes": 213789966336,
        "start": 444416,
        "start_offset_bytes": 227540992,
        "is_ntfs": true,
        "attrs": null
      },
      {
        "node": "/dev/iscsi_thick_vg/iscsi_devedse4",
        "name": "",
        "type": "DE94BBA4-06D1-4D40-A16A-BFD50179D6AC",
        "size_bytes": 728760320,
        "start": 418002944,
        "start_offset_bytes": 214017507328,
        "is_ntfs": false,
        "attrs": "RequiredPartition GUID:63"
      }
    ]
  },
  "ntfs_analysis": [
    {
      "partition": {
        "node": "/dev/iscsi_thick_vg/iscsi_devedse3",
        "name": "Basic data partition",
        "type": "EBD0A0A2-B9E5-4433-87C0-68B6B72699C7",
        "size_bytes": 213789966336,
        "start": 444416,
        "start_offset_bytes": 227540992,
        "is_ntfs": true,
        "attrs": null
      },
      "analysis_success": true,
      "error": null,
      "file_system_info": {
        "total_clusters": 0,
        "free_clusters": 0,
        "cluster_size_bytes": 0,
        "total_size_bytes": 213789966336,
        "free_size_bytes": 176115056640,
        "used_size_bytes": 37674909696,
        "used_percentage": 17.622393764162325
      }
    }
  ]
}
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