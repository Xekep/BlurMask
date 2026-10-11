# BlurMask

A small desktop annotation toolbar and always-on-top privacy mask for screen recording and streaming.
Create a rectangle from the toolbar, move/resize it over sensitive content, and
switch privacy effects without doing post-production blur work.

## Features

- system tray application;
- Epic Pen-style floating drawing toolbar, shown on manual launch; login startup stays in the tray without creating windows;
- left-click the tray icon to show/hide the toolbar and restore any open canvas;
- right-click tray menu contains only **Show/Hide window** and **Exit**;
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
- The eye button collapses the toolbar with an animation and hides ink; the chalk logo remains above the closed eye. Click the eye to restore the toolbar. **Minimize to tray** hides it completely; click the tray icon to restore it.
- The cursor button opens two modes: normal cursor and **Select and move**. Click a drawn object and drag it on the screen or canvas; Delete removes it. Moves support undo/redo.
- Shapes have a separate button. The mask button opens a horizontal strip: each icon creates a new independent mask, and the last button removes all masks.
- **Follow window** below the mask icons automatically detects the window under each mask's centre. Masks move with the window and scale their position and dimensions as it resizes, hide while it is minimized, and close with it. Moving/resizing a mask by hand updates its attachment. Windows uses out-of-context geometry events; macOS and X11 check geometry every 16 ms. Wayland disables the switch because it does not expose foreign window geometry. Masks remain independent overlays; unrelated application rendering is not modified.
- The analog timer opens a horizontal strip: off, quick (2 s), long (10 s), very long (30 s), with progressively filled timer icons. Temporary ink fades locally and is excluded from saved/shared boards.
- The canvas uses a compact floating menu; **Tab** hides/restores it. **Ctrl+V** pastes a screenshot from the clipboard as a movable image that is saved, shared and included in PNG exports.
- Pasted images are normalized to PNG (up to 2048 px on the longest side); shared documents are limited to 4 MiB and 32 megapixels of raster content.
- Interface language follows the system UI language: English, French, German or Russian; other languages fall back to English.
- Screenshots include all monitors and visible annotations, exclude the toolbar, and save PNG files to `Pictures/BlurMask`.
- The menu includes **New mask**, text size, history actions, visibility, shortcuts, and exit.
- Multiple monitors use separate transparent windows and a shared document with physical desktop coordinates.

Shortcuts follow the installed Epic Pen layout used as the reference: **Ctrl+Shift+1** ink visibility,
**2** cursor, **3** pen, **4** highlighter, **5** eraser, **6** undo, **7** clear,
**PrintScreen** screenshot, **[ / ]** decrease/increase width, **F** fading ink, **0** toolbar visibility,
**L/E/R/A/T** line/ellipse/rectangle/arrow/text, **W/B** white/black screen board, **9** previous color.
**Alt+Shift+1–6** select quick colors. BlurMask additions: **Ctrl+Shift+Y** redo, **C** independent canvas, **D** screen input.
Open **Menu → Settings** to record, disable or reset shortcuts and select the interface language.
Shortcut changes apply immediately; language changes apply after restarting. Settings are saved in the user's
local application-data directory as `BlurMask/settings.json`. The settings window suspends shortcuts while
recording combinations and rejects duplicates. Windows shortcuts are global; on Linux/macOS they work while
a BlurMask window is focused. Conflicts with another app appear in Settings; the toolbar remains available.

Windows supports click-through ink and desktop screenshots. Other platforms currently hide the
ink overlay in cursor mode; desktop screenshots and global hotkeys are Windows-only.
Desktop annotations and toolbar settings are session-local. The separate canvas is saved locally.

## Independent canvas and shared boards

Use the board flyout or **Ctrl+Shift+C** to open a separate canvas that starts at 2400×1600 and grows in all directions as ink approaches its edges.
**Ctrl+Shift+D** returns drawing input to the screen. Screen and canvas drawings have independent
documents and undo histories. Both use the same drawing tools, sizes and colors. The canvas
supports panning, white/black backgrounds, PNG export and automatic local persistence.
The wheel zooms around the pointer; bottom-right controls and **Ctrl +/-** change zoom,
**Ctrl+0** fits the board. Middle-button dragging or cursor mode pans. Growing never shifts
object coordinates, including negative coordinates and shared peer drawings. PNG export
includes extended content and scales large boards to at most 4096 pixels on either side.

The canvas menu's **Connect** button accepts an invitation code or link.
**Collaborate** creates a shared copy on the built-in `https://pen.kiiko.ru` service on first click and displays separate **Draw together** and view codes,
with buttons to copy the code or invitation link. Links use one opaque fragment (`/#<code>`)
without visible board/key parameters. The code retains the full 256-bit access key.
Only this invitation format is supported. Pens, highlighters and shapes appear for other participants
while they are being drawn: an authenticated WebSocket sends new points about every 50 ms.
Finished strokes save immediately through the PostgreSQL API; a 1.3-second polling fallback preserves
concurrent strokes and pending offline edits. Unfinished previews stay outside saved documents,
exports and undo history; cancellation or disconnection clears them. View-only participants also
receive live previews. Screen annotations and fading ink remain local.

The minimalist software download page, ASP.NET Core API, PostgreSQL storage and Docker/Caddy configuration for
`pen.kiiko.ru` are in [`server/`](server/README.md). Deployment requires a server and DNS
configuration; see that README for launch commands. Accounts are not required: shared links
carry permission to edit or view a particular board. The website has no drawing editor;
opening an invitation there offers software downloads and a button to copy the invitation into BlurMask.

Epic Pen's [official guide](https://epicpen.com/userguide) describes desktop annotation and
white/black screen boards. The independent persistent canvas and shared native drawing
are BlurMask additions.

## Privacy modes

Right-click the rectangle to cycle:

1. **Blur** - compositor blur-behind when available.
2. **Pixel Blur** - transparent coarse pixel tiles over compositor blur.
3. **Scramble** - on Windows, live 8×8 pixel source tiles swap diagonally in adjacent groups using four Magnification planes. Unsupported platforms use an opaque mosaic.
4. **Glass Refraction** - on Windows, fixed 30×30 px optical cells on a 32 px pitch. The cells are assigned to four persistent Magnification planes with slightly different zoom/offset/color transforms, so lines and text genuinely break at cell boundaries. Resizing adds/removes square cells instead of stretching them. Other platforms currently use the translucent glass-block fallback.
5. **Blackout** - solid black privacy mask.

No mask writes desktop captures to disk. On Windows, Glass Refraction and Scramble use the system Magnification API with exactly four child Magnifier windows per mask, regardless of cell count. Each plane is clipped by a disjoint Win32 region; sources refresh at 10 Hz while visible and immediately on move/resize. Blackout provides a solid cover; other effects preserve some visual context. Native optical effects are disabled in RDP/software mode, with opaque fallback. The minimum mask size is 60×40.
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

The default Windows build is one native `BlurMask.exe`: Skia, HarfBuzz and ANGLE
are statically linked using StaticLink.Avalonia. No external rendering DLLs or
installed .NET runtime are required. Windows system libraries are still used.
For renderer diagnostics, `./build.ps1 win-x64 -DynamicNativeLibraries` produces
the traditional build with three adjacent native DLLs; distribute that entire folder.

### Linux

```bash
bash ./build.sh linux-x64
bash ./build-deb.sh linux-x64 1.2.3
sudo apt install ./.artifacts/installers/blurmask_1.2.3_amd64.deb
```

### macOS

```bash
bash ./build.sh osx-arm64
```

Native AOT is OS-specific: publish Windows on Windows, Linux on Linux, and macOS on macOS.
Linux payloads include their native `.so` libraries. Debian packaging requires `dpkg-dev`;
the package installs into `/opt/blurmask`, adds an application launcher, and enables
login startup with `--minimized` through `/etc/xdg/autostart/blurmask.desktop`. The application launcher still opens the toolbar normally. Disable startup in the
desktop's Startup Applications settings. Builds use Ubuntu 22.04 for the glibc baseline.
The reusable/manual **Debian Installer** workflow verifies installation, startup,
reinstallation and removal. CI uploads `.deb` and SHA-256 as **BlurMask-Debian-Installer**;
the Release workflow attaches them alongside Linux tar archives and Windows installers.

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
SHA-256 and JSON build metadata. The installer contains the single native executable,
excludes debug symbols, and does not require an installed .NET runtime.
It supports English/French/German/Russian, installs into `%LOCALAPPDATA%/Programs/BlurMask`
without elevation, creates a Start Menu shortcut, and offers optional desktop
shortcut and Windows startup tasks. Startup is selected by default on first install;
the user's choice is retained during upgrades. The startup entry uses `--minimized` to keep the app in the tray; manual launch opens the toolbar. Close BlurMask before updating or uninstalling.
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

See [`docs/TROUBLESHOOTING.md`](docs/TROUBLESHOOTING.md), especially if a Native AOT
build starts with a Windows "stack-based buffer overrun" dialog.

## License

MIT.
