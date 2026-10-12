# Orientation child265 stroke smoothing

`LegacyOrientationStrokePreparation` reconstructs the ordinary
173b90→1728c0 path of orientation2d0's optional child265. It is a real spline
producer, rather than a point-removal heuristic. The complete source219 metric
forwarding and primary249 scoring still require their own verified binding;
the actual-resource probe labels supplied metrics explicitly.

The pinned Text binary is the same SHA256 as in
[orientation evidence](legacy-line-orientation.md). No vendor DLL executes.
Type265 descriptor2dfa70's interface at2dfb00 exposes173b90. Its assembly
passes the source physical associated-record table asNULL and the key
multiplier as1 to1728c0. Orientation081e90 requests the prepared stroke buffer
but passesNULL for the optional point-key output. Original full coordinates
are rotated only after selection: this smoothing buffer is for candidate
evaluation and does not replace raw source chronology.

## Resource and numerical producer

Reader174080 for v102 consumes eight floats at offsets8..24. The actual
Russian node6021 is48 bytes and contains
`(.05,.075,.075,.15,.35,.10,.15,.0001)`. These control minimum/maximum
smoothing weight, additional corner-dependent weight, two ratio intervals,
and the squared flatness factor. The old-version conversion of the final
factor is deliberately outside the v102 reader.

1728c0 takes source-state metricsa0/a4. If either is negative,172680 recomputes
both. The first ratio counts nearly horizontal or vertical physical segments;
the second counts transitions between those directions, divided by total
segment count minus physical stroke count. This unusual denominator matters
for short strokes: it is not silently replaced with total interior points.
Both comparisons use float relative`2^-22`/absolute`2^-23` tolerance.
Supplying nonnegative metrics bypasses this recomputation, exactly as in the
native prepared path. Values0 are not inferred as a universal source default.

For each unassociated physical stroke:

* Fewer than three original points are copied unchanged. Otherwise remove
  nearly duplicate points against the **last accepted** coordinate. The final
  duplicate updates optional point metadata in native code but does not change
  the retained coordinate. That metadata is not requested by orientation.
* Fewer than two accepted points copy the original stroke. Two or three accepted
  points copy the deduplicated stroke. Four or more enter the spline stage.
* Segment lengths use double Euclidean norms of float coordinate differences.
  The penalty is float`n*weight^4*scale^4` divided by the double total arc.
* 171700 constructs a symmetric five-diagonal system of size`n−2`. The
  independent oracle expresses it as`R+6λQQᵀ`, where Q is the weighted second
  difference operator and R has diagonal`2(h_previous+h_next)` with adjacent
  entry`h_next`. RHS coordinate differences are subtracted in float before
  multiplication by inverse lengths in double.
* `LegacyCubicStrokeSmoothing` ports1712c0's no-pivot band factorization and
  171700's endpoint/interior corrections. It returns per-segment value, first,
  second and third polynomial coefficients with zero endpoint curvature.
* Convert each segment polynomial to its four Bezier control coordinates.
  171e00 measures each interior control point's squared distance to the closed
  endpoint segment. 1722d0/171f00 recursively bisect at half using de Casteljau.
  Both squared distances must be **strictly** below
  float`scale*flatnessFactor*scale`; depth11 forces an endpoint.

Native left-subdivision point-key output uses rounded fractional positions;
the right-terminal output uses the whole-segment endpoint. Those keys are
outside this no-key-output orientation scope. No timestamp, category or
source physical point identity is invented here.

## Verification

```powershell
python tools/LegacyOrientationSmoothing.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyOrientationSmoothing.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

256 spline controls solve the independent dense system with NumPy, rather than
copying the managed band solver. Another192 complete preparation controls
check metrics, piecewise weights, last-accepted duplicate handling, short
strokes, spline geometry and13113 flattened coordinates. Invalid inputs and
cancellation are rejected. Managed and Native AOT runs must both pass before
the phase is considered verified.

Actual resource mode parses265@6021, then runs owned printed MIR strokes
through ordinary raw ingress, orientation axis scale, spline preparation and
the real2d0 projection policy. With explicitly supplied metrics(0,0) and with
the negative-metric recomputation branch, it produces54 points and projection
angles`0,−15,+15,90,30` first. Before265 the ranking was
`0,−90,+90,15,−30`. This demonstrates that the previously missing producer
changes the evaluated data. It does **not** establish full orientation
recognition accuracy, Russian cursive acceptance or primary249 cost parity.

The bounded port accepts256 physical strokes and4096 total prepared input
points, at most4096 output points, finite scales/coordinates and a nonsingular
finite spline system. Exceeding the output budget rejects the request instead
of silently truncating curves. Associated/custom records, source remapping,
optional point keys and persistent buffer reuse are separate native paths.
