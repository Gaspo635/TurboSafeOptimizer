# iClover Tweaks

Professional Windows 10/11 x64 maintenance and optimization utility.

## Download

The official download page points to the latest GitHub Release:

https://github.com/Gaspo635/TurboSafeOptimizer/releases/latest/download/iCloverTweaks.exe

The release is a self-contained Windows x64 single-file executable.

## What it does

- System information (CPU, GPU, RAM and Windows version)
- Windows privacy/settings shortcuts
- Temporary-file cleanup
- DNS cache flush
- Windows Game Mode settings
- Task Manager, Services, System Information and System Properties shortcuts
- Restore point request before the main optimization flow

The application does not disable Windows Defender, Firewall, UAC or other core Windows security mechanisms.

## Safety and permissions

The application starts with the normal Windows user context. Administrator elevation is requested only when Windows requires it for the restore-point operation.

No passwords, API keys, access tokens or database credentials are required by the application.

Maintenance actions are intentionally conservative. No personal documents or photos are targeted by the temporary-file cleanup.

## Build

The GitHub Actions workflow restores .NET 8, builds Windows x64, publishes a self-contained single-file EXE, verifies that only iCloverTweaks.exe is present, uploads the EXE as an artifact, and creates a GitHub Release containing only the EXE.

## Requirements

- Windows 10 or Windows 11
- 64-bit x64 processor/OS
- No separate .NET installation is required for the published EXE

## Source and license

Source code is available in this repository under the MIT License. See LICENSE.

## Project status

The project is being hardened for reliable public distribution. Builds are validated by GitHub Actions, but hardware-specific behavior should still be tested on representative Windows 10/11 machines before claiming universal compatibility.
