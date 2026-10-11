# Legacy directional intersections

`LegacyQuantizedSegmentIntersection` restores the checked `0b5fa0` arithmetic
helper: exponent-10 slopes, integer truncation, dominant-axis segment inclusion
and the native parallel-overlap representative. It is not a replacement for
an exact general-purpose geometric predicate. Unsupported signed-division
overflow is rejected instead of permitting a process fault.

`LegacyDirectionalIntersections` restores the checked `0b6270` classification
pass on prepared physical sample points. It accepts either the output of
`LegacyDirectionalResampling` or explicitly supplied prepared strokes,
dimensions, original resampling stride and grid multiplier. The multiplier is
caller data; the helper does not derive a profile from a model resource.

The pass preserves the four-bin lookup order, newest-first segment chains,
previous-segment exclusion, stride-relative proximity check, fixed-point angle
filter, nearest-vertex ties and first-write flags. Class 1 marks interior points
of self-loop intervals, class 2 self-crossing vertices and class 3 crossings
between physical strokes. The aggregate counts accepted nonoverlapping
self-loop intervals rather than all geometric pair intersections. Pen-up paths
are not inserted.

Managed and Windows Native AOT checks pass against separate Python references:
4096 own segment pairs and 640 own spatial-classification cases. Analytic
crossings, collinear/vertical cases, self-loop intervals, cross-stroke flags,
invalid input, cancellation and a complete own ink → normalization → sampling
→ intersection path also pass. The Python classification reference scans prior
segments by bin rather than using the implementation's linked-head grid.
No vendor engine is executed and no resource weights or vendor sample ink
are part of the controls. This establishes consistency with the independent
arithmetic reference, not measured handwriting accuracy or full SDK parity.

```powershell
python tools/LegacyIntersection.Probe/reference_controls.py .artifacts/ru-intersections.json
dotnet run --project tools/LegacyIntersection.Probe -c Release -- .artifacts/ru-intersections.json
```

The later [temporal](legacy-russian-directional-model.md) and
[spatial](legacy-russian-spatial-model.md) components now assemble and evaluate
the 313 networks with explicit caller calibration, baseline and context.
The [word module](legacy-russian-directional-words.md) combines all four families.
Automatic line profiles, joined-character cuts and
representative handwriting controls remain unfinished. These components do not
enable Russian cursive recognition in the AI-pen interface.
