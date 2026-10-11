# Sampler297 cursive point ownership

The checked Russian lite acoustic resource contains sampler **type297 v106**
at offsets11506 and188157. Its native type-method table is`1802e1490`;
metadata descriptor is`1802e15e0`. Unlike the type252 whole-stroke sampler,
this path stores cuts inside physical strokes.

`LegacyCursiveInkAtoms.Build` restores its source ownership adapter at
`1801935e0`. Each prepared marker contains `(kind, sourceStrokeIndex, pointIndex)`.
The resulting atom starts at that point. If the next marker belongs to the same
physical stroke, its exclusive end is **nextPoint+1**; otherwise its end is that
source stroke's point count. Adjacent atoms on a continuous stroke therefore
share exactly one endpoint. Kind is preserved as opaque metadata.

The source here is the **prepared ink buffer**. Native `191950` can insert
interpolated points through `18dde0`; its indices and sampler `193380` refer to
that prepared buffer. Do not apply prepared markers directly to raw strokes.
An index mapping back to raw input is separate metadata, not replacement geometry.

`Compose` restores the checked single-range atom path of`180193690/180193720`.
Consecutive atom ranges merge only when they refer to the same physical stroke
and`previous.end == next.start+1`. Their common point appears once in the final
range. Disjoint ranges, wider overlaps and different source strokes retain
separate pen-up boundaries. The combined range feeds
[ordinary sample extraction](legacy-russian-point-ranges.md), preserving the
original continuous stroke rather than inventing pen lifts at each cut.

The native count/read routines also accept arrays of multi-range groups. Their
overlap branch updates only the prior group's last endpoint; the checked API
deliberately covers single-range atoms and does not infer missing generic
multi-range behavior. Machine-code checks of`180193720` confirm that branch.

This resolves how partial cursive samples retain point ownership, **not where
the cuts should be**. Marker production at`180191950` applies several geometry
passes, typed point tags and line metrics (`18018e1f0`, `18018e960`, `180190350`,
`1801908c0`, `1801911d0`, `180191430`). Those passes remain separate; the caller
must supply markers. Arbitrary x splits or caller-provided expected characters
are not substituted for native candidate production.

The probe builds own zigzag atoms, composes them back to one physical source
stroke with a separate late dot, and checks endpoint ownership, copied marker
identity, source order, false stitching, input bounds and cancellation.1024
independent Python cases vary point counts, within-stroke cuts, marker kinds and
selected subpaths:

```powershell
python tools/LegacyCursiveAtoms.Probe/reference_controls.py .artifacts/legacy-cursive-atom-controls.json
dotnet run --project tools/LegacyCursiveAtoms.Probe -c Release -- .artifacts/legacy-cursive-atom-controls.json
```

These are model-free ownership controls and do not measure recognition quality.
