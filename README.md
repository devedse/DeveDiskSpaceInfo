# DeveDiskSpaceInfo

This is a project to read the actual used diskspace of disks on linux without mounting them

## Installation & Usage

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
  "device_path": "/dev/sdb",
  "partition_table": {
    "device": "/dev/sdb",
    "label": "gpt",
    "sector_size": 512,
    "first_lba": 34,
    "last_lba": 976773134,
    "total_sectors": 976773101,
    "total_size_bytes": 500107837952,
    "partitions": [
      {
        "node": "/dev/sdb1",
        "name": "partition1",
        "type": "Microsoft basic data",
        "size_bytes": 499999997952,
        "start": 2048,
        "start_offset_bytes": 1048576,
        "is_ntfs": true,
        "attrs": ""
      }
    ]
  },
  "ntfs_analysis": [
    {
      "partition": {
        "node": "/dev/sdb1",
        "name": "partition1",
        "type": "Microsoft basic data",
        "size_bytes": 499999997952,
        "start": 2048,
        "start_offset_bytes": 1048576,
        "is_ntfs": true,
        "attrs": ""
      },
      "analysis_success": true,
      "error": null,
      "file_system_info": {
        "total_size_bytes": 499999997952,
        "used_size_bytes": 250000000000,
        "free_size_bytes": 249999997952,
        "used_percentage": 50.0
      }
    }
  ]
}
```

### Requirements

- Linux operating system
- Root privileges (sudo) for device access
- .NET 9.0 runtime

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