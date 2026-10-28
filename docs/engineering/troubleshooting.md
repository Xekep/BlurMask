# Troubleshooting

## Windows: "stack-based buffer overrun" / immediate native crash

The default Windows Native AOT build links rendering libraries into `NovaPen.exe`.
Formula recognition also needs the bundled `runtimes/win-x64/native` folder.
Older builds and the diagnostic
`-DynamicNativeLibraries` build require these libraries alongside the executable:

- `av_libglesv2.dll`
- `libSkiaSharp.dll`
- `libHarfBuzzSharp.dll`

`build.ps1` verifies the selected payload type. For a dynamic build distribute its
complete publish folder. Distribute the complete static publish folder as well
to retain formula recognition and its license notices.

A missing native DLL usually causes a loader/DLL error rather than a true managed stack
overflow, so if the complete publish directory is present and the program still terminates,
treat that as a separate Native AOT/native backend failure.

Useful isolation steps:

1. Build/run without AOT:

   ```powershell
   dotnet run --project src/NovaPen -c Release -p:PublishAot=false
   ```

2. Publish Native AOT again from the CLI, not by manually copying Visual Studio output:

   ```powershell
   .\tools\build.ps1 win-x64
   ```

3. Run the CI smoke mode from the publish directory:

   ```powershell
   .\src\NovaPen\bin\Release\net11.0\win-x64\publish\NovaPen.exe --smoke-test
   ```

4. If non-AOT works but the complete Native AOT payload fails, capture the Windows
   Event Viewer/Application Error entry and the process exit code. That separates a native
   renderer failure from application logic.

## Remote Desktop / GPU device changes

Launch `NovaPen.exe --software-rendering` before connecting over RDP to keep the
renderer independent of GPU/compositor changes. Sessions already using RDP select
this profile automatically at startup. It disables native glass magnification and
uses an opaque cover for effects requiring compositor blur. Existing ink is retained
across monitor changes. Live RDP reconnect behavior depends on the Windows session
and driver and needs an actual disconnect/reconnect test.

## The mask is transparent but not blurred on Linux

Blur-behind is compositor-specific. Scramble and Blackout are compositor-independent. Pixel Blur uses compositor blur. Glass Refraction uses the Windows Magnification API on Windows and a compositor-based fallback elsewhere. The fallback privacy modes still hide the underlying content.

## Linux / Hyprland: empty or transparent window

New builds select CPU rendering automatically under Hyprland when its valid
session environment is present. This avoids the GLX alpha path associated with
black toolbars and uses less memory in isolated tests. A physical Hyprland/GPU
confirmation is still required. Desktop ink disables blur/shadow/dimming only for
its own mapped windows through bounded compositor IPC; no user rules are changed.

Linux Settings offers **Auto**, **X11 / XWayland**, and **Wayland (experimental)**,
applied after restarting. Auto retains X11 when `DISPLAY` is present, including
XWayland on Hyprland, and uses Wayland on Wayland-only sessions. Native Wayland
currently supports the separate canvas only: Avalonia 12.1.2 ignores absolute
window positioning and Topmost, and provides no layer-shell role. Desktop overlays,
privacy masks and desktop capture are disabled in that mode. Canvas pen menus use
native relative popups and toolbar dragging uses the compositor's move operation.

You can override a saved selection without editing configuration:

```sh
novapen --display-backend x11 --software-rendering
novapen --display-backend wayland
```

Close the existing process first. Explicit Wayland needs `WAYLAND_DISPLAY`; without
it the app reports a fallback to X11. `--software-rendering` is an X11 profile and
cannot be combined with explicit native Wayland. These are different choices:
window protocol (X11/Wayland) and drawing renderer (GLX/CPU).

The desktop ink layer is transparent by design; it receives input while a drawing
tool is selected. The separate **Canvas** window should have an opaque board and
a toolbar. If that window is transparent, it is a rendering problem.

Close NovaPen using **Quit** in its tray menu before trying another renderer:
the application only permits one running instance. Run:

```sh
novapen --software-rendering
```

This selects the CPU renderer on Linux as well as Windows in builds containing
the Linux renderer fix. Earlier builds ignored this option on Linux.
For those builds, `LIBGL_ALWAYS_SOFTWARE=1 novapen` can help isolate the GPU path.

On Hyprland, capture the window information while the issue is visible:

```sh
hyprctl version
hyprctl activewindow
hyprctl clients -j
pacman -Q novapen hyprland xorg-xwayland mesa
```

Window titles distinguish the desktop ink layer from Canvas. The client data
also shows whether the compositor tiled the window, its actual size, and whether
it uses XWayland. Missing cursor files during theme fallback in `strace` are
normal if a later cursor lookup succeeds; `killed by SIGINT` indicates an
interrupt signal and does not establish that the application crashed.

The Linux ink layer excludes the drawing panel and its menus from both its
visible shape and its pointer input region. This keeps the controls usable even
when a compositor ignores requests to raise a window. This requires the X11
Shape library (`libxcb` on Arch, `libxcb-shape0` on Debian/Ubuntu), included in
the installer dependencies.

### Drawing blurs the whole desktop

The ink layer requests plain transparency, not blur. Hyprland can nevertheless
apply its global blur, shadow and dimming effects to transparent normal windows.
The app now uses Hyprland's session IPC to disable those effects only on mapped
desktop-ink windows owned by its own PID. It does not edit configuration files
or global window rules. Requests are asynchronous, cancellable and bounded;
missing or unavailable IPC does not stop drawing. Canvas and privacy masks keep
their separate effects. See the official [window effects](https://wiki.hypr.land/0.54.0/Configuring/Window-Rules/)
and [IPC](https://wiki.hypr.land/IPC/) documentation.

Other compositors need their own per-window configuration. For example, picom
with global background blur needs an exclusion for the desktop ink window's
title; inspect its title with `xprop _NET_WM_NAME`. Do not exclude all `NovaPen`
windows, because that would also disable blur privacy masks. The regression
checks compare a detailed real desktop before and after mapping/drawing, and
include a blur-enabled negative control. Tests under Xvfb/Openbox/picom do not
establish behavior on a physical Hyprland/GPU session.

## Right-click seems to do nothing

The mode changes immediately. The tray tooltip shows the current mode. Pixel Blur, Scramble, Glass Refraction and Blackout are visually obvious; Blur depends on the OS compositor.


## Glass Refraction on Windows

The Windows Glass Refraction mode keeps the optical cells fixed at 30×30 px on a 32 px pitch, so resizing only changes how many cells are visible. Internally it uses exactly four persistent Magnifier child windows for the entire mask. Each one is clipped with a disjoint Win32 region and has a slightly different zoom, offset and color transform. This gives piecewise optical displacement without creating one HWND per cell. There is no 60 Hz geometry timer; NovaPen updates the four source rectangles only when the mask moves or resizes. If the native backend cannot initialize, NovaPen falls back to the translucent glass-block visual instead of crashing.
