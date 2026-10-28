# Build and packaging

## Build requirements

- .NET SDK `11.0.100-preview.7.26381.103` or a compatible .NET 11 SDK;
- Windows Native AOT: Visual Studio Build Tools with **Desktop development with C++**;
- Linux Native AOT: `clang` and `zlib1g-dev`;
- macOS Native AOT: Xcode command-line toolchain.

The repository contains `global.json` pinned to the .NET 11 preview used during development.

## Build

Normal verification build:

```powershell
dotnet restore
dotnet build -c Release -p:PublishAot=false -p:SelfContained=false
```

### Windows x64 Native AOT

```powershell
.\tools\build.ps1 win-x64
```

Output:

```text
src\BlurMask\bin\Release\net11.0\win-x64\publish\
```

The default Windows build is one native `BlurMask.exe`: Skia, HarfBuzz and ANGLE
are statically linked using StaticLink.Avalonia. No external rendering DLLs or
installed .NET runtime are required. Windows system libraries are still used.
Keep the complete publish folder: formulas additionally use the CPU ONNX Runtime
and four app-local VC CRT dependencies under `runtimes/win-x64/native/`.
For renderer diagnostics, `./tools/build.ps1 win-x64 -DynamicNativeLibraries` produces
the traditional build with three adjacent native DLLs; distribute that entire folder.

### Linux

```bash
bash ./tools/build.sh linux-x64
bash ./tools/build-deb.sh linux-x64 1.2.3
sudo apt install ./.artifacts/installers/blurmask_1.2.3_amd64.deb
```

### macOS

```bash
bash ./tools/build.sh osx-arm64
```

Native AOT is OS-specific: publish Windows on Windows, Linux on Linux, and macOS on macOS.
Linux payloads include their native `.so` libraries. Debian packaging requires `dpkg-dev`;
the package installs into `/opt/blurmask`, adds an application launcher, and enables
login startup with `--minimized` through `/etc/xdg/autostart/blurmask.desktop`. The application launcher still opens the toolbar normally. Disable startup in the
desktop's Startup Applications settings. Builds use Ubuntu 22.04 for the glibc baseline.
The reusable/manual **Debian Installer** workflow verifies installation, startup,
reinstallation and removal. CI uploads `.deb` and SHA-256 as **BlurMask-Debian-Installer**;
the Release workflow attaches them alongside Linux tar archives and Windows installers.

### Arch Linux x86_64

Download `blurmask-1.2.3-1-x86_64.pkg.tar.zst` from the GitHub Release, then install:

```bash
sudo pacman -U ./blurmask-1.2.3-1-x86_64.pkg.tar.zst
# Remove the application while preserving user canvases/settings:
sudo pacman -R blurmask
```

The package uses the same Ubuntu 22.04 Native AOT payload as the Linux archive;
no installed .NET runtime is needed. Pacman installs its system library dependencies
(glibc 2.35+, GCC runtime, X11, fontconfig, ICU, OpenSSL, zlib and D-Bus).
Use an X11 session, or install `xorg-xwayland` on Wayland. It adds the launcher,
icon, MIT license and minimized login startup in `/etc/xdg/autostart/blurmask.desktop`.
Disable startup through the desktop's Startup Applications settings. User changes
to the startup file are preserved during package upgrades by pacman's backup handling.

To package an existing Linux x64 publish directory on Arch with `base-devel`, run
as a normal user:

```bash
bash ./tools/build-arch.sh 1.2.3
```

The local-input [`PKGBUILD`](../../packaging/arch/PKGBUILD) gets SHA-256 checksums from
the packaging script and keeps native binary bytes unchanged. The reusable/manual
**Arch Installer** workflow packages in one Arch container, then installs, checks
native dependencies, launches under Xvfb, reinstalls and removes in a fresh container.
CI reuses its Linux Native AOT artifact and uploads the package/checksum as
**BlurMask-Arch-Installer**; Release attaches both after testing. Manual runs can
set `release-tag` to an existing release (for example `v1.2.3`), verifying the
published Linux archive's GitHub SHA-256 digest before packaging it without rebuilding.
Arch is rolling release; the container validates current repositories, not every
desktop/compositor or derivative distribution. This is a downloadable package,
not an Arch repository/AUR registration.

### Void Linux x86_64 (glibc)

Download the `.x86_64.xbps` package and its SHA-256 file from the GitHub Release.
XBPS installs packages from a repository directory, so index the downloaded file first:

```bash
sha256sum --check blurmask-1.2.5_1.x86_64.xbps.sha256
mkdir -p blurmask-repo
cp blurmask-1.2.5_1.x86_64.xbps blurmask-repo/
xbps-rindex -a blurmask-repo/*.xbps
sudo xbps-install -S --repository="$PWD/blurmask-repo" blurmask
# Remove the application while preserving user canvases/settings:
sudo xbps-remove blurmask
```

The package supports **glibc**, not Void musl. It reuses the tested Ubuntu 22.04
Native AOT payload, includes the formula runtime and licenses, and installs system
dependencies through XBPS. Desktop drawing uses X11/XWayland; the experimental
native Wayland backend supports the separate canvas. The desktop environment
must provide its graphics driver and XWayland when needed. Login startup is
minimized and can be disabled in the desktop's Startup Applications settings.

[`Void Installer`](../../.github/workflows/xbps.yml) packages as an unprivileged user
in an official Void glibc container. A separate fresh container checks installation,
native dependencies, ONNX Runtime C API, startup, real pointer/pixel drawing in
both renderer modes, reinstallation and removal. CI and Release reuse the existing
Linux artifact; the packager never rebuilds the application. Manual runs accept
`source-run-id` or a checksum-verified existing `release-tag`. This is a downloadable
package, not a registration in Void's official repository.

### Windows installer (Inno Setup)

```powershell
.\tools\build-installer.ps1 -Version 1.2.3
```

This publishes Windows x64 Native AOT, downloads **Inno Setup 7.1.0 x64** from the
official immutable release, verifies its pinned SHA-256, and compiles
[`packaging/BlurMask.iss`](../../packaging/BlurMask.iss). The compiler is installed for
the current user under `.artifacts/tools`; no existing Inno Setup installation is required.
Version 7.1.0 was the latest stable release on 2026-10-10; see the
[official downloads](https://jrsoftware.org/isdl.php).

Output: `.artifacts/installers/BlurMask-1.2.3-win-x64-Setup.exe`, accompanied by
SHA-256 and JSON build metadata. The installer contains the native executable and formula runtime,
excludes debug symbols, and does not require an installed .NET runtime.
It supports English/French/German/Russian, installs into `%LOCALAPPDATA%/Programs/BlurMask`
without elevation, creates a Start Menu shortcut, and offers optional desktop
shortcut and Windows startup tasks. Startup is selected by default on first install;
the user's choice is retained during upgrades. The startup entry uses `--minimized` to keep the app in the tray; manual launch opens the toolbar. Close BlurMask before updating or uninstalling.
Uninstall removes the program, shortcuts and its startup entry while preserving local canvases.

To package an existing Windows publish directory:

```powershell
.\tools\build-installer.ps1 -Version 1.2.3 -SkipPublish -PublishDirectory src/BlurMask/bin/Release/net11.0/win-x64/publish
```

Use the version corresponding to that payload. To verify installation, reinstallation,
startup and removal on a clean Windows user/runner:

```powershell
.\tools\test-installer.ps1 -InstallerPath .artifacts/installers/BlurMask-1.2.3-win-x64-Setup.exe -PublishDirectory src/BlurMask/bin/Release/net11.0/win-x64/publish
```

The test refuses to touch an existing BlurMask installation, shortcuts or startup
entry, and refuses to close a running BlurMask. It installs into a unique test
directory, checks payload hashes, launches the installed application's smoke mode,
reinstalls to verify replacement of outdated files, then uninstalls and checks cleanup.
Logs remain in `.artifacts/installer-test`. `-SkipLaunch` skips only the app startup check.

GitHub Actions:

- **CI** calls the reusable [Windows Installer workflow](../../.github/workflows/installer.yml)
  after the Windows Native AOT job, reusing its artifact. The setup executable and
  checksums appear as **BlurMask-Windows-Installer**.
- **Windows Installer → Run workflow** builds manually; an optional version accepts
  values such as `1.2.3` or `1.2.3-rc.1`. CI/manual builds without a version use
  `1.0.0-dev.<run number>`.
- A `v1.2.3` tag triggers **Release**, builds the application and installer with
  that version, tests the installer, and attaches the setup executable/checksums
  to the GitHub Release alongside the portable archives.

The stable installer AppId is shared by all versions so upgrades use the same
installation. When updating the Inno Setup dependency, change the version and hash
in `tools/install-inno-setup.ps1` together with workflow labels/build metadata.

## Smoke test

The app has a CI-only smoke mode that creates a mask, exercises the drawing document,
toolbar, board modes, Windows mouse input/eraser, and click-through styles, then exits automatically:

```powershell
.\src\BlurMask\bin\Release\net11.0\win-x64\publish\BlurMask.exe --smoke-test
```

This is used by CI to catch startup/native-loader failures that a compile-only workflow misses.
Set `BLURMASK_SMOKE_ARTIFACTS` to a directory to also save rendered toolbar/menu images
and Windows desktop screenshots for visual verification. Desktop captures are generated only
when this environment variable is explicitly set or the user clicks the screenshot button.

## Platform notes

### Windows

Avalonia compositor blur/acrylic is used for the basic blur modes. Glass Refraction uses the Windows Magnification API through four constant optical planes clipped into 30×30 px cells on a 32 px pitch. The project targets `net11.0`; the default Windows output statically links its rendering libraries.

RDP is detected at startup and selects software rendering without GPU composition or
native magnifier planes. Blur/glass modes use an opaque privacy cover in this profile.
For an app started locally before connecting through RDP, launch with
`BlurMask.exe --software-rendering`. Screen topology changes are debounced and preserve
ink when the OS temporarily reports no monitors. CI exercises the software profile;
a real RDP disconnect/reconnect is still required to validate a specific driver/session.

### Linux

Blur-behind depends on the desktop compositor. X11/XWayland environments generally have
the most predictable behavior. If the compositor does not provide blur-behind, the app
falls back to transparency/privacy overlays.

### macOS

The application additionally installs an `NSVisualEffectView` behind the Avalonia content
using source-generated `LibraryImport` interop suitable for Native AOT.

## Troubleshooting

See [troubleshooting](troubleshooting.md), especially if a Native AOT
build starts with a Windows "stack-based buffer overrun" dialog.

## License

MIT.
