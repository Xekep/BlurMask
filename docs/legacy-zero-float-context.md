# Native zero/tiny float context windows

The public `LegacyFloatContextGeometry.Extract` now accepts valid single-point
and tiny-extent windows through a separate restored1c8400 special stage. It keeps
the previous nonempty/non-null, finite coordinate, input-count and cancellation
checks. Ordinary nondegenerate geometry/raster behavior remains unchanged.

Native1c8400 tests its float width-plus-one and height-plus-one fields against1.
Thus it takes this branch both for identical points and for tiny nonzero extents
that round away during float addition. It skips physical arc/direction/per-stroke
and raster loops, then sets meanY to the **first selected point's Y**, regardless
of the usual point-mean/arc-weighted mode. It sets both marked-run averages to1,
which normalize to−0.5. Four direction channels remain0 and arc normalizes to−1.

The last-point pointer also resets to the first selected point; endpoint deltas
therefore remain0 even for a window whose tiny points are not identical. The
global minimumY and X-boundary still use actual source points. All per-stroke
stored fields remain0: first top subtracts the supplied vertical reference,
later zero-top sentinels retain0, and dimensions/arcs normalize to−1. The extended
stroke-count indicators still reflect the selected physical stroke count.

`LegacyDegenerateFloatContextGeometry` implements this internal validated branch;
the public extractor dispatches to it before the ordinary raster stage. The
first ordinary row context consumer therefore processes point windows without
a guessed fallback or short conversion.

```powershell
python tools/LegacyZeroContext.Probe/reference_controls.py .artifacts/zero-context-controls.json
dotnet run --project tools/LegacyZeroContext.Probe -c Release -- .artifacts/zero-context-controls.json
dotnet run --project tools/LegacyZeroContext.Probe -c Release -- .artifacts/zero-context-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json
```

The independent Python oracle expresses named native fields at the special
stage exit and normalizes those fields separately.512 cases match every output
float exactly, including255 nonidentical tiny windows, both X-boundary modes,
both normal Y modes,1–5 physical strokes, positive/negative references and tiny
line heights. Both Y modes return the same special-path values. These controls
would fail if the code used ordinary raster statistics, final-point deltas or
point-mean Y for the tiny path.

The actual resource diagnostic appends a late single-point dot to own printed
M/I/R. The selected row24a/265 pipeline preserves that point; the first context
producer processes the zero window and accepts10 of12 possible windows. Its
28 fixed context channels reach a first-model180-class glyph evaluation without
rejection. This proves **context window** support, not dot glyph recognition,
diacritic ownership, native word winner or joined Russian accuracy.

Managed and Windows Native AOT pass these controls and the actual diagnostic;
the earlier512 nondegenerate float-reference cases and their first-row model
outputs also remain unchanged. No GUI or vendor engine execution is used.
