# iClover Tweaks

Professional Windows 10/11 x64 optimizer and maintenance utility.

## Download

The GitHub Pages download page points to the latest single-file executable:

https://github.com/Gaspo635/TurboSafeOptimizer/releases/latest/download/iCloverTweaks.exe

## Build

The repository contains a GitHub Actions workflow that:

1. Restores .NET 8.
2. Publishes a self-contained Windows x64 single-file EXE.
3. Verifies that the EXE exists.
4. Creates a GitHub Release with the EXE.
5. Uploads the EXE as a workflow artifact.

## Safety

iClover Tweaks is designed around a restore-first workflow. It does not disable Windows Defender, Firewall, UAC or other core security mechanisms.

Current maintenance actions are intentionally conservative: restore point request, temporary-file cleanup, DNS cache flush and links to native Windows tools.

## GitHub Pages

Set GitHub Pages to deploy from the `docs/` folder on the `main` branch. The page then uses the stable GitHub Releases latest-download URL, so the download button automatically follows the newest release.

## License

See the repository license before redistributing builds.
