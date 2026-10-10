# BlurMask

A small desktop annotation toolbar and always-on-top privacy mask for screen recording and streaming.
Create a rectangle from the system tray, move/resize it over sensitive content, and
switch privacy effects without doing post-production blur work.

## Features

- system tray application;
- Epic Pen-style floating drawing toolbar, shown on startup;
- click the tray icon to show the drawing toolbar;
- tray context menu: drawing tools, create mask, close mask, exit application;
- always-on-top frameless rectangle;
- drag the center with the left mouse button to move it;
- drag edges/corners to resize it;
- middle click the mask to destroy it;
- right click the mask to cycle privacy modes;
- hover-only outline so the editing border normally stays out of recordings;
- .NET 11 + Avalonia 12;
- Native AOT deployment;
- Native AOT-safe code path; tray resources are defined in compiled Avalonia XAML.

## Desktop drawing

The narrow white toolbar follows the classic Epic Pen layout: drag handle, visibility,
cursor, drawing tools, eraser, pen size, undo, clear, board, screenshot, menu, and colors.
Icons are drawn as local vector geometry. Reference: [Epic Pen toolbar screenshot](https://software.thaiware.com/13290.html)
and [official tool guide](https://epicpen.com/userguide).

- Select the pen button to open a side menu with **pen, highlighter, text, line, arrow, rectangle, ellipse**.
- Draw directly over the desktop and applications. On Windows, cursor mode leaves the ink visible
  and passes clicks to applications underneath. Right-click the drawing surface or press Esc to return to cursor mode.
- The eraser removes entire strokes. Each eraser gesture, drawing operation, and clear can be undone.
  Right-click Undo or use the menu for Redo.
- Hold Shift while drawing shapes for a square/circle or 45° line/arrow angles.
- Click to place text; Enter commits, Shift+Enter adds a line, and Esc cancels.
- Choose five stroke widths, four quick colors, 24 palette colors, or enter a custom hex color.
- Switch between the desktop, whiteboard, and blackboard without losing annotations.
- The eye button collapses the toolbar and hides ink; click the round handle to restore it.
- Screenshots include all monitors and visible annotations, exclude the toolbar, and save PNG files to `Pictures/BlurMask`.
- The menu includes **New mask**, text size, history actions, visibility, shortcuts, and exit.
- Multiple monitors use separate transparent windows and a shared document with physical desktop coordinates.

Global Windows shortcuts: **Ctrl+Shift+1** cursor, **2** pen, **3** highlighter, **4** eraser;
**Ctrl+Shift+Z/Y** undo/redo, **F** clear, **H** hide/show ink, **S** screenshot.
If another application owns a shortcut, use the corresponding toolbar button.

Windows supports click-through ink and desktop screenshots. Other platforms currently hide the
ink overlay in cursor mode; desktop screenshots and global hotkeys are Windows-only.
Desktop annotations and toolbar settings are session-local. The separate canvas is saved locally.

## Independent canvas and shared boards

Use the board flyout or **Ctrl+Shift+5** to open a separate, zoomable 2400×1600 canvas.
**Ctrl+Shift+6** returns drawing input to the screen. Screen and canvas drawings have independent
documents and undo histories. Both use the same drawing tools, sizes and colors. The canvas
supports panning, white/black backgrounds, PNG export and automatic local persistence.

The canvas **Connect** button accepts an invitation code or link, or creates a server copy
on the built-in `https://pen.kiiko.ru` service. **Share** displays separate edit and view codes,
with buttons to copy the code or a browser link. Links use one opaque fragment (`/#<code>`)
without visible board/key parameters. The code retains the full 256-bit access key; existing
parameter-based invitations still work. Browser and desktop changes synchronize every 1.3 seconds, preserving
concurrent strokes and pending offline edits. Screen annotations remain local.

The web canvas, ASP.NET Core server, PostgreSQL storage and Docker/Caddy configuration for
`pen.kiiko.ru` are in [`server/`](server/README.md). Deployment requires a server and DNS
configuration; see that README for launch commands. Accounts are not required: shared links
carry permission to edit or view a particular board.

Epic Pen's [official guide](https://epicpen.com/userguide) describes desktop annotation and
white/black screen boards. The independent persistent canvas and browser synchronization
are BlurMask additions.

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

- Right-click the tray icon to open the application menu: drawing tools, create a mask, close the mask, or exit BlurMask.
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

### Windows installer (Inno Setup)

```powershell
.\build-installer.ps1 -Version 1.2.3
```

This publishes Windows x64 Native AOT, downloads **Inno Setup 7.1.0 x64** from the
official immutable release, verifies its pinned SHA-256, and compiles
[`installer/BlurMask.iss`](installer/BlurMask.iss). The compiler is installed for
the current user under `.artifacts/tools`; no existing Inno Setup installation is required.
Version 7.1.0 was the latest stable release on 2026-10-10; see the
[official downloads](https://jrsoftware.org/isdl.php).

Output: `.artifacts/installers/BlurMask-1.2.3-win-x64-Setup.exe`, accompanied by
SHA-256 and JSON build metadata. The installer contains the executable and all
native rendering DLLs, excludes PDBs, and does not require an installed .NET runtime.
It supports English/Russian, installs into `%LOCALAPPDATA%/Programs/BlurMask`
without elevation, creates a Start Menu shortcut, and offers optional desktop
shortcut and Windows startup tasks. Close BlurMask before updating or uninstalling.
Uninstall removes the program, shortcuts and its startup entry while preserving local canvases.

To package an existing Windows publish directory:

```powershell
.\build-installer.ps1 -Version 1.2.3 -SkipPublish -PublishDirectory bin/Release/net11.0/win-x64/publish
```

Use the version corresponding to that payload. To verify installation, reinstallation,
startup and removal on a clean Windows user/runner:

```powershell
.\tools\test-installer.ps1 -InstallerPath .artifacts/installers/BlurMask-1.2.3-win-x64-Setup.exe -PublishDirectory bin/Release/net11.0/win-x64/publish
```

The test refuses to touch an existing BlurMask installation, shortcuts or startup
entry, and refuses to close a running BlurMask. It installs into a unique test
directory, checks payload hashes, launches the installed application's smoke mode,
reinstalls to verify replacement of outdated files, then uninstalls and checks cleanup.
Logs remain in `.artifacts/installer-test`. `-SkipLaunch` skips only the app startup check.

GitHub Actions:

- **CI** calls the reusable [Windows Installer workflow](.github/workflows/installer.yml)
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
.\bin\Release\net11.0\win-x64\publish\BlurMask.exe --smoke-test
```

This is used by CI to catch startup/native-loader failures that a compile-only workflow misses.
Set `BLURMASK_SMOKE_ARTIFACTS` to a directory to also save rendered toolbar/menu images
and Windows desktop screenshots for visual verification. Desktop captures are generated only
when this environment variable is explicitly set or the user clicks the screenshot button.

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
