# BlurMask

A small cross-platform always-on-top privacy mask for screen recording and streaming.
Create a rectangle from the system tray, move/resize it over sensitive content, and
switch privacy effects without doing post-production blur work.

## Features

- system tray application;
- click the tray icon to create or activate the mask;
- tray context menu: create mask, close mask, exit application;
- always-on-top frameless rectangle;
- drag the center with the left mouse button to move it;
- drag edges/corners to resize it;
- middle click the mask to destroy it;
- right click the mask to cycle privacy modes;
- hover-only outline so the editing border normally stays out of recordings;
- .NET 11 + Avalonia 12;
- Native AOT deployment;
- Native AOT-safe code path; tray resources are defined in compiled Avalonia XAML.

## Privacy modes

Right-click the rectangle to cycle:

1. **Blur** - compositor blur-behind when available.
2. **Pixel Blur** - transparent coarse pixel tiles over compositor blur.
3. **Scramble** - a deliberately shuffled block pattern with stronger visual breakup.
4. **Glass Refraction** - on Windows, fixed 30×30 px optical cells on a 32 px pitch. The cells are assigned to four persistent Magnification planes with slightly different zoom/offset/color transforms, so lines and text genuinely break at cell boundaries. Resizing adds/removes square cells instead of stretching them. Other platforms currently use the translucent glass-block fallback.
5. **Blackout** - solid black privacy mask.

None of the modes performs an application-level desktop capture. On Windows, Glass Refraction uses the system Magnification API with exactly four child Magnifier windows for the whole mask, regardless of how many cells are visible. Each plane is clipped by a disjoint Win32 region. There is no 60 Hz polling/render timer and no child HWND per cell; BlurMask only updates the four source rectangles when the mask actually moves or resizes. Blackout and Scramble remain the strongest privacy modes; Pixel Blur and Glass Refraction intentionally preserve some visual context.
The tray tooltip shows the current mode.


### Tray and single-instance behavior

- Right-click the tray icon to open the application menu: create a mask, close the mask, or exit BlurMask.
- Only one BlurMask process is allowed per OS user/session. Launching it again exits silently with code `0`; it does not show a dialog, notification, or second tray icon.

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
dotnet build -c Release -p:PublishAot=false
```

### Windows x64 Native AOT

```powershell
.\build.ps1 win-x64
```

Output:

```text
bin\Release\net11.0\win-x64\publish\
```

**Do not copy only `BlurMask.exe`.** Avalonia Native AOT on Windows still requires
native DLLs next to the executable. The build script verifies the expected files:

- `av_libglesv2.dll`
- `libSkiaSharp.dll`
- `libHarfBuzzSharp.dll`

Copy/distribute the **whole publish directory**.

### Linux

```bash
bash ./build.sh linux-x64
```

### macOS

```bash
bash ./build.sh osx-arm64
```

Native AOT is OS-specific: publish Windows on Windows, Linux on Linux, and macOS on macOS.

## Smoke test

The app has a CI-only smoke mode that creates a mask and exits automatically:

```powershell
.\bin\Release\net11.0\win-x64\publish\BlurMask.exe --smoke-test
```

This is used by CI to catch startup/native-loader failures that a compile-only workflow misses.

## Platform notes

### Windows

Avalonia compositor blur/acrylic is used for the basic blur modes. Glass Refraction uses the Windows Magnification API through four constant optical planes clipped into 30×30 px cells on a 32 px pitch. The project targets `net11.0`. Native AOT output must be kept together with its native rendering DLLs.

### Linux

Blur-behind depends on the desktop compositor. X11/XWayland environments generally have
the most predictable behavior. If the compositor does not provide blur-behind, the app
falls back to transparency/privacy overlays.

### macOS

The application additionally installs an `NSVisualEffectView` behind the Avalonia content
using source-generated `LibraryImport` interop suitable for Native AOT.

## Troubleshooting

See [`docs/TROUBLESHOOTING.md`](docs/TROUBLESHOOTING.md), especially if a Native AOT
build starts with a Windows "stack-based buffer overrun" dialog.

## License

MIT.
