# Ordinary sample point ownership

`LegacyInkRangeExtraction` restores the ordinary-sample point adapter at native
`1801b5ba0`, with start/end feature flags from `1801b7f70`. A sample carries
12-byte triples `(sourceStrokeIndex, startPoint, endPointExclusive)`. The adapter
opens a new stroke for every triple and copies exactly the points in `[start,end)`.
It does not sort ranges, merge adjacent ranges, insert shared endpoints,
interpolate points or join a late dot to its stem with a pen-down bridge.

The float overload retains source float32 coordinates exactly. The short
overload applies the same ownership operation to already quantized geometry.
Its boundary flags use the explicit exponent10 feature convention; the native
float flags use `+1/-1`. The first flag is positive only when the first range
starts at source point0 and logical start suppression is off. The last flag is
positive only when the final range reaches that source stroke's point count and
logical end suppression is off. These flags use first/last range order, rather
than bounding-box extrema or earliest/latest acquisition globally.

All ranges and selected points are copied. This independent implementation
rejects malformed or empty intervals, nonfinite selected float coordinates,
more than15 ranges, more than4096 selected points and unsupported source sizes.
Native empty-range behavior and special sample kinds1/2 are outside this API.
Duplicate/overlapping caller ranges are retained, matching the ordinary copying
operation; a cut producer is responsible for its ownership policy.

Interior ranges now provide the mechanics needed to evaluate partial cursive
strokes without pretending the partial endpoint was a physical pen release.
**This does not infer character boundaries.** Candidate-cut production, cut
costs, full physical-context exclusion for partial ranges and language decoding
remain separate. Splitting a stroke at an arbitrary x coordinate is not claimed
as native segmentation or Russian cursive recognition.

The probe checks an own continuous zigzag with interior cuts and a late dot,
adjacent ranges without duplicated endpoints, reversed range order, logical
suppression, exact negative-zero float bits, caller mutation isolation, malformed
input, work limits and cancellation.768 deterministic independent Python cases
cover multiple physical strokes, repeated and overlapping ranges, point-only
ranges and all four logical-suppression combinations:

```powershell
python tools/LegacyInkRanges.Probe/reference_controls.py .artifacts/legacy-ink-range-controls.json
dotnet run --project tools/LegacyInkRanges.Probe -c Release -- .artifacts/legacy-ink-range-controls.json
```

The probe starts no GUI and uses no vendor binary or model resource.
