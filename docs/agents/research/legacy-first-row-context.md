# First ordinary row float context

`LegacyFirstRowContext` connects the owned current row's prepared **float**
source265 coordinates to the first ordinary model. It uses the fitted row frame,
stored initial context network/configuration, native context30 geometry and
probability/selection/weighting operations, then the prepared first247/229 glyph
classifier. It does not round a whole line into shorts before context extraction.

The old `LegacyPreparedWordGroupEvaluator` whole-line short context remains an
explicit original diagnostic policy; this component does not silently change it.
The current component's scope is a fresh, already-selected single ordinary row:
no source mutation cache, opaque/boundary group kinds, custom input scopes,
automatic global row discovery, final class cache or word winner.

## Resource and binding evidence

The typed current source247@6120 owns first ordinary21d@13082 and its initial
network13103–19094 (30 /100 /28). Reader1c2940 reads the following configuration.
Initializer1b0760 packs it into the40-byte descriptor passed to227. Constructor
1c95d0 copies that descriptor into227 runtime, and builds226 with128×128 raster
dimensions plus its geometry flags/window count.

| Serialized21d field | First value | Runtime227 | Meaning |
| --- | --- | --- | --- |
|48|0|10|Left weighting offset|
|4c|1|14|Left weighting variance|
|50|1|18|Right weighting variance|
|54|0|1c /226+c|Maximum-X mode off|
|55|3|1d|Maximum context window strokes|
|56|28|1e|Native one-based blank class (API27)|
|58|0.1|20|Strict nonblank mass cutoff|
|5c|1|24|Retain blank output in context vector|
|60|2|28|Same-line all accepted-window selection|
|64|0|2c /226+d|Arc-weighted Y, not point-mean Y|
|65|1|2d /226+e|Extended30 geometry|

The extended flag is verified in serializer1c91e0 assembly: instruction1c92c0
checks226+e, omitting the simple stroke-count fraction and later adding three
count indicators/per-stroke channels when it is nonzero. Its zero branch is
outside this30-input model. This was not inferred from the network's width alone.

Native1c9870 receives the physical **float** source geometry and resolved line
baseline/height. It generates starts after the first physical stroke and windows
up to the stored count3. It maintains a cumulative earlier-X reference, feeds
the226 geometry through fixed-input conversion to the initial network, converts
full EXP probabilities, and accepts a window only when nonblank mass is strictly
greater than0.1. It retains all28 probabilities because raw5c is1.

The accepted window center is its point-count meanX. Nativeca5f0 obtains target
meanX from the supplied graph group ranges, selects according to mode2 and applies
the resource0/1/1 weights using the source line height. No context frame scale or
offset is fabricated. This bounded endpoint requires physically ordered,
nonoverlapping ordinary graph ranges; chronological sorting is a separate
normalization operation. Fresh no-custom-scope windows retain scope−1.

## Float geometry and checks

`LegacyFloatContextGeometry` restores the same checked nondegenerate extended30
226 feature profile as the earlier integer-scope component while retaining float
coordinates. It preserves float deltas, double hypot with float narrowing,
float32 accumulations and exact integer raster steps. The native zero-extent
special branch also includes extents whose float `extent+1` rounds to1. Its
restored skipped-statistics path keeps first-pointY, zero endpoint deltas and
the native run/per-stroke constants. It does not round those valid point windows
into ordinary strokes. See the [zero/tiny window evidence](legacy-zero-float-context.md).

```powershell
python tools/LegacyFirstRowContext.Probe/reference_controls.py .artifacts/float-context-controls.json
dotnet run --project tools/LegacyFirstRowContext.Probe -c Release -- .artifacts/float-context-controls.json
dotnet run --project tools/LegacyFirstRowContext.Probe -c Release -- .artifacts/float-context-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json
```

The independent Python reference enumerates raster coverage by major-axis integer
coordinates rather than copying the C# stepping loop.512 cases cover fractional
points, both maximum-X modes and both Y-statistic modes (absolute tolerance1.5e−6).
Of these,384 produce different output from premature short truncation. The own
zero-weight network fixture verifies9 accepted windows and equal context-channel
weighting. Typed field rejection, tiny/NaN geometry, foreign cut ownership and
cancellation are checked. Windows Native AOT reproduces Managed outputs.

For own printedM/I/R on the actual first model,7 of9 possible windows are accepted.
Context vectors are nonneutral. The lowest raw acoustic outputs areM label22:6172,
I label70:16233 andR label51:14405. These are diagnostic raw output classes, not
final cached labels or a recognized Russian word. Better or worse glyph ranks
are not used to alter the supplied native configuration.

Still missing are first-profile final cache/controller/search composition,
complete native winner/confidence and representative connected Russian words.
Nothing in this component enables Russian AI-pen UI or establishes cursive
recognition accuracy.
