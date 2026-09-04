# Troubleshooting

## Windows: "stack-based buffer overrun" / immediate native crash

First make sure you did not copy only `BlurMask.exe` out of the Native AOT publish folder.
Avalonia Native AOT on Windows requires native rendering libraries alongside the executable:

- `av_libglesv2.dll`
- `libSkiaSharp.dll`
- `libHarfBuzzSharp.dll`

`build.ps1` checks for them after publishing. Distribute the complete `publish` directory.

A missing native DLL usually causes a loader/DLL error rather than a true managed stack
overflow, so if the complete publish directory is present and the program still terminates,
treat that as a separate Native AOT/native backend failure.

Useful isolation steps:

1. Build/run without AOT:

   ```powershell
   dotnet run -c Release -p:PublishAot=false
   ```

2. Publish Native AOT again from the CLI, not by manually copying Visual Studio output:

   ```powershell
   .\build.ps1 win-x64
   ```

3. Run the CI smoke mode from the publish directory:

   ```powershell
   .\bin\Release\net11.0\win-x64\publish\BlurMask.exe --smoke-test
   ```

4. If non-AOT works but Native AOT fails with all three DLLs present, capture the Windows
   Event Viewer/Application Error entry and the process exit code. That separates a native
   renderer failure from application logic.

## The mask is transparent but not blurred on Linux

Blur-behind is compositor-specific. Scramble and Blackout are compositor-independent. Pixel Blur uses compositor blur. Glass Refraction uses the Windows Magnification API on Windows and a compositor-based fallback elsewhere. The fallback privacy modes still hide the underlying content.

## Right-click seems to do nothing

The mode changes immediately. The tray tooltip shows the current mode. Pixel Blur, Scramble, Glass Refraction and Blackout are visually obvious; Blur depends on the OS compositor.


## Glass Refraction on Windows

The Windows Glass Refraction mode keeps the optical cells fixed at 30×30 px on a 32 px pitch, so resizing only changes how many cells are visible. Internally it uses exactly four persistent Magnifier child windows for the entire mask. Each one is clipped with a disjoint Win32 region and has a slightly different zoom, offset and color transform. This gives piecewise optical displacement without creating one HWND per cell. There is no 60 Hz geometry timer; BlurMask updates the four source rectangles only when the mask moves or resizes. If the native backend cannot initialize, BlurMask falls back to the translucent glass-block visual instead of crashing.
