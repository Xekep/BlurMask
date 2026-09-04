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

Blur-behind is compositor-specific. Big Pixels, Scramble, Glass Blocks and Blackout are compositor-independent
privacy modes and will still hide the underlying content.

## Right-click seems to do nothing

The mode changes immediately. The tray tooltip shows the current mode. Big Pixels, Scramble, Glass Blocks and Blackout are visually obvious; Blur depends on the OS compositor.
