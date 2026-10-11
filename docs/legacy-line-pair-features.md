# Prepared stroke pairs for the embedded line classifiers

`LegacyLineStrokeStatistics` and `LegacyLinePairFeatures` supply the checked
input profile for the [embedded line classifiers](legacy-line-neural-classifier.md).
They do not assign strokes to lines, choose a group scale or interpret the
classifier's two labels as a baseline or font size.

The 17-field stroke record comes from `180077140`: min/max X/Y, point mean X/Y,
arc length, eight directional arc-length bins, endpoint distance divided by
bounding-box diagonal, and diagonal divided by arc length. A zero-length stroke
uses zero endpoint ratio and one diagonal/arc ratio. Input preparation removes
points nearly equal to the last accepted point using the same float32 tolerance
as [line scale samples](legacy-line-scale-samples.md). Direction bins use
`floor(float32(atan2(dy, dx)) / .7853982) + 4`, capped at seven. The source uses
double atan2 followed by float32 conversion for this operation.

The 42-field producer `1800759c0` requires an explicit sequence of original
stroke IDs, group width, group scale and a sorted list of skipped stroke IDs.
It compares each stroke with the preceding stroke in the sequence; the first
row compares the first stroke with itself. Source instruction sequences
`180075c45`, `180075c59`, `180075c6f` recover three angles that the decompiler
does not show correctly: inverse mean-Y difference, inverse top-Y difference
and inverse bottom-Y difference, each relative to inverse mean-X difference.
The subsequent helpers are cosine and sine.

| Channels | Prepared quantities |
| --- | --- |
| 0–7 | Vertical gap and mean-Y difference divided by previous/current/union heights and scale |
| 8–13 | Cosine and sine of mean, top and bottom pair angles |
| 14–22 | Square-root areas of previous/current/union bounds divided by three heights |
| 23–24 | Current and previous arc lengths divided by union height |
| 25–28 | Current and previous endpoint/diagonal and diagonal/arc ratios |
| 29–30 | Current and previous dominant direction bins, first bin winning ties |
| 31–40 | Horizontal gap and mean-X difference divided by three heights, group width and scale |
| 41 | Native signed stroke-index relation with explicit skipped-ID count gating |

Height denominators use `max(height, max(.05 * scale, 3))`; area roots use an
area floor of one. Channel 41 preserves the native strict count guard: skipped
IDs are counted between the pair only when index distance is smaller than the
complete skipped-ID list's length. It is not replaced with a different adjacency
policy. Record fields are copied, each call owns output storage, bounded inputs
and nonfinite/overflow geometry are rejected, and cancellation is accepted.

Managed and Windows Native AOT runs pass 788 independent prepared-stroke
records and 33,096 pair-feature channels. The generated controls include flat,
single-point and repeated-point strokes, reversed index orders, height floors,
skipped IDs and varied scales. Forty own-stroke feature matrices additionally
run through the two real embedded neural models and match an independent NumPy
reference at output tolerance `2e-4`. Host transcendental functions and floating
quantization boundaries are not claimed to match all native instruction paths
bit for bit. These are numerical controls with generated ink, rather than a
handwriting or automatic-line accuracy benchmark.

```text
python tools/LegacyLinePairFeatures.Probe/reference_controls.py .artifacts/line-pair-controls.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
dotnet run --project tools/LegacyLinePairFeatures.Probe -c Release -- .artifacts/line-pair-controls.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
```
