# Offscreen render/import probe

This standalone project links the production ink document and uses Avalonia Headless
with CPU Skia. It calls `SetupWithoutStarting`, creates no windows, and does not load
the production application or any desktop backend. It needs the repository's .NET SDK.

Run in the repository root:

```powershell
dotnet run --project tools/BlurMask.RenderProbe -c Release -- --output .artifacts/render-current --label current --frames 200
```

An optional `--board .artifacts/board-review/board.json` adds private-board timing and
allocation counts. The probe never saves pixels or contents from that board. Keep
the results local; do not upload private input or private reports as CI artifacts.

To compare a preserved older `InkDocument.cs`, build with
`-p:InkDocumentSource=<absolute-path-to-old-file>` and run the resulting DLL. Keep the
same linked shared contracts, fonts, runtime, synthetic generator and frame count
in both runs. Rebuild without that property to measure the current implementation.
The draw delegate automatically passes zoom to implementations with a `viewScale`
parameter; older implementations receive `Draw(context)`.

```powershell
python tools/BlurMask.RenderProbe/compare.py .artifacts/render-baseline .artifacts/render-current
```

The comparison requires Pillow and only reads generated synthetic images. It checks
full-resolution image and pen pixels, reduced-image detail against an independent
pixel-area reference, and a deliberately blurred negative control. A failed quality
gate must be investigated rather than replaced with a timing-only comparison.

CI uses `python tools/BlurMask.RenderProbe/quality.py <output>` to check reduced
detail against an independent area reference with fixed error limits, rejected
blurred controls, and nonzero intermediate coverage of thin strokes at each zoom.
It needs no historical source or private board. Pointer/compositor behavior is
checked separately by the installed-app Arch/Void tests.

Each zoom starts with a fresh document so the first image decode/LOD request occurs
at that zoom. The benchmark reports cold draw, repeated draws, unchanged import,
changed metadata import, and import plus draw. Synthetic images contain fine detail
at 4K and are drawn at 100%, 25%, and 10%. Rendering includes canvas scale, clip and
mark culling, but excludes canvas chrome, live-layer caching, collaboration timers,
GPU presentation and pointer latency. Managed allocation counts exclude native Skia
bitmap storage; process memory includes allocator retention. Treat these as offscreen
CPU costs, not desktop FPS or thermal measurements.
