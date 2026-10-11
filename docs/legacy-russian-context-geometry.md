# Legacy extended context geometry

`LegacyContextGeometry` restores the checked nondegenerate extended30 type226
profile (`1801c8400`, serializer `1801c91e0`) for integer-coordinate physical
strokes. It accepts explicit positive line height and horizontal/vertical
references, plus the native maximum-X and point-mean-Y modes. These inputs remain
caller data; neither line fitting nor prior-candidate selection is supplied.

The30 float channels contain bias; normalized global position, weighted or
point-mean Y, dimensions and physical arc; average marked runs in columns and
rows of a128×128 raster; four directed path-length fractions; an X-boundary and
endpoint delta; three physical-stroke-count indicators; and position/dimensions/
arc for the first three physical strokes. Later zero-valued stroke tops retain
the checked native sentinel ambiguity. Overlapping raster marks merge while
physical arc remains additive. Pen-up gaps are not rasterized or counted as arc.

The raster preserves the inspected integer stepping rule including diagonal
steps on ties. Its independent reference enumerates each major-axis coordinate
and calculates the rounded minor-axis displacement directly. The native hypotf
helper (`18025bab0`) widens float deltas, calculates a double square root and
narrows the segment length before accumulation; the implementation preserves
those stages. `LegacyFloatInputQuantization` supplies the subsequent network
input conversion without nearest rounding.

The bounded implementation accepts1..15 strokes and at most4096 points. It
explicitly rejects the native zero-extent special branch, whose per-stroke and
raster statistics differ. Fractional source points are also outside this API's
current integer-coordinate scope. The older30-sample trajectory extractor and
the313 temporal geometry have different meanings and are not substitutes.

Managed and Windows Native AOT tests pass512 independent Python float32/raster
reference cases (absolute tolerance3e−7), plus analytic layout, translation with
matching references, raster overlap versus physical arc, later zero-top sentinel,
float-to-fixed composition, malformed/degenerate inputs and cancellation.

The optional probe mode runs caller-owned inspected RU initial context network
30→100→28 (`7001`, span189755..195746) with the restored geometry and EXP class
probabilities. On own printed И and П shapes with explicit height40, Xreference0,
Yreference40 and default modes, the largest unlabeled output is class27 at
26088/32768 and27168/32768 respectively. Those are context network classes,
not alphabet labels; no text recognition success is asserted. Assets are not
included, and SDK runtime/authorization is not bypassed.

```powershell
python tools/LegacyContextGeometry.Probe/reference_controls.py .artifacts/ru-context-geometry.json
dotnet run --project tools/LegacyContextGeometry.Probe -c Release -- .artifacts/ru-context-geometry.json
# Optional caller-owned inspected legacy assets:
dotnet run --project tools/LegacyContextGeometry.Probe -c Release -- .artifacts/ru-context-geometry.json acoustic.res activations.json
```

Automatic prior-group/line selection, contextual313 word decoding and
representative joined Russian handwriting accuracy remain unverified. This
experimental component is not enabled in AI-pen UI.
