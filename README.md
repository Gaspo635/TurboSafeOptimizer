# TurboSafe Optimizer

Professional Windows optimization and diagnostics utility focused on gaming, maintenance and reversible workflows.

> **NO MEJORÓ = NO SE QUEDA.**

## What it does

- Detects CPU, GPU, RAM and Windows information.
- Creates local diagnostic JSON reports.
- Cleans Windows temporary files without targeting personal documents.
- Flushes DNS and resets Winsock with confirmation.
- Opens official Windows Game Mode settings.
- Opens Windows System Information.
- Can request a Windows restore point before major changes.
- Does not disable Defender, Firewall, UAC or other Windows security mechanisms.

## Download

The latest self-contained Windows x64 executable is published automatically:

https://github.com/Gaspo635/TurboSafeOptimizer/releases/latest/download/TurboSafeOptimizer.exe

It is a single .exe and includes the .NET runtime.

## Build

Requires .NET 8 SDK and Windows:

```powershell
dotnet publish TurboSafeOptimizer/TurboSafeOptimizer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

## Architecture

Scan → Measure → Backup → Change → Measure → Keep/Revert.

The current release is deliberately conservative: diagnostics and maintenance first, with explicit confirmations for privileged operations.