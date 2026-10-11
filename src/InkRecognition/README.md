# InkRecognition

Portable CPU recognition of pen coordinates. The project has no UI dependencies,
NuGet packages, Python runtime or native ML libraries. It is compatible with
.NET 10 or later, including Native AOT, and can be referenced from another project or copied as a
separate directory. Source code follows the repository MIT license; model weights
are separate and retain their own terms.

```xml
<ProjectReference Include="../InkRecognition/InkRecognition.csproj" />
```

```csharp
using InkRecognition;

using var stream = File.OpenRead("en-US.ikrpack");
IStrokeTextRecognizer recognizer = StrokeTextRecognizer.Load(stream);
var strokes = new InkStroke[]
{
    new(new InkPoint[] { new(0, 0), new(8, 20), new(16, 0) })
};
var result = recognizer.Recognize(strokes, cancellationToken);
Console.WriteLine(result.Text);
```

Reuse one loaded recognizer. Weights are immutable; each recognition call owns its
scratch buffers. Calls accept cancellation. UI scheduling, row grouping, colors,
fonts and document editing belong to the caller. Model loading retains ownership
of the supplied stream with the caller.

The initial implementation supports 13-channel, unlinked-stroke text models:
float32 geometry/resampling, channel normalization, float/int16/int8 affine
kernels, peephole LSTM/BLSTM and CTC collapse. Input limits are 256 strokes and
20000 finite points within ±100000 coordinate units. Sampling is bounded to 6000
frames. The pack reader validates SHA256, lengths, dimensions, UTF-8, finite
parameters, tensor padding and full consumption of input. SHA256 detects damaged
data; it is not a publisher signature.

`IKRPACK1` models use little-endian primitives and a 48-byte header: eight magic
bytes, version, body length and SHA256 of the body. The body contains language,
labels, preprocessing parameters, architecture, tensor inventory and packed
weights. [The exporter](../../tools/export-ink-model.py) documents the exact layout.
No model weights are embedded in this library.

Current limits: whole-word geometry is heuristic; CTC has no lexical language
model; Russian and Math2 are not implemented here. Confidence is an acoustic
score. This is an experimental engine, not a reproduction of the complete
MyScript SDK.

The legacy RU investigation has a separate experimental API:
`LegacyInkNetwork.Load` reads a complete FEA0/version106 classifier. The caller
supplies weights and activation tables. `LegacyStrokeFeatures` builds trajectory
inputs from up to 15 strokes with explicit pen-up transitions;
`LegacyRasterFeatures` builds the statistics/projection/contour profile.
Caller-supplied `LegacyInkBaseline` enables the nine baseline features.
`LegacyClassScores.Convert` implements the quantized EXP normalization and
log-score channel; confidence calibration is separate. There are no NuGet or
native dependencies, and the probe passes Native AOT checks.
These APIs do not implement `IStrokeTextRecognizer`: joined Russian handwriting
still needs the remaining profiles, segmentation, ensemble and language decoding.

Run `dotnet run --project tools/LegacyInk.Probe -c Release` from the repository
root for vendor-free format, integer, geometry, raster, baseline and score tests.
The probe supports `--glyph-cases resource.res
tools/LegacyInk.Probe/isolated-ru-pi-raster.json activations.json` for eight
original synthetic **П/И** controls against locally supplied assets. Supplied
reference files can be checked with `--raw-controls` or `--score-controls`.
These controls exercise narrow raw-ink paths, not handwriting accuracy.
See [the evidence and current limits](../../docs/ink-math-engine.md#russian-text-preparation).

`LegacyPackedLexicon` restores the checked packed RU dictionary graph and
supports prefix transitions and terminal word lookup without expanding its
labels/offsets. `LegacyLexicalDecoder` filters a caller-supplied segmented glyph
lattice through that graph. Run `dotnet run --project tools/LegacyLexicon.Probe
-c Release` for vendor-free malformed-format, transition and ambiguous-candidate
tests. These APIs do not turn raw ink into Russian words; acoustic-to-lexical
mapping, cursive segmentation and language costs remain unfinished.
`LegacyInkAlphabet` reads caller-supplied acoustic and lexical alphabets so that
their distinct numeric IDs can be mapped through Unicode text.
See [lexical format evidence and checks](../../docs/legacy-russian-lexicon.md).

The alphabet loader also exposes stored virtual-label markers, per-symbol bits
and the checked three-form accessor with narrow/wide signed lookup maps and
mode fallback. Relations may include both case and accented forms. Reading
these fields does not implement the engine's word-case policy or preserve case
through dictionary decoding. `LegacyLexicon.Probe` covers absent relations,
sentinel insertion, malformed references and version103/104 payloads.

`LegacySegmentedWordRecognizer` connects the isolated raster classifier to the
dictionary through Unicode acoustic/lexical label mapping. The original
`LegacySeparatedInkSegmenter` proposes left-to-right groups of overlapping
whole physical strokes and preserves stroke acquisition order within groups.
It retains connected cursive strokes intact; these groups do not prove letter
boundaries. Vendor-free probes cover mapping, terminal words, grouping, late
dots, geometry limits and cancellation. Four locally supplied classifier checks
rank the original printed **ИП** sample first after automatic separated grouping.
This remains an experimental headless path; general Russian words and joined
handwriting are not enabled in the app. See [the checked pipeline and limits](../../docs/legacy-russian-segmented-word.md).

An opt-in `relatedFormMode` expands candidates through supplied acoustic form
relations and retains the strongest score per lexical label. The default uses
exact text mapping. Eight own separated printed МАМА/МИР controls then rank
dictionary words мама/мир first. This returns dictionary spelling, does not
retain written case and is not representative Russian handwriting accuracy.

`LegacyLexicalLatticeDecoder` performs original bounded dictionary beam search
over alternative caller-scored glyph cuts. It returns both the word labels and
the selected boundaries, so callers can compare possible segmentations without
committing to a single split first. It does not generate the cuts or score raw
ink. `LegacyLexicon.Probe` exercises ambiguity, complete coverage, duplicate-word
merging, final terminals and resource/cancellation guards without vendor assets.

`LegacyDirectionalLandmarks` restores the checked directional reversal and
physical stroke-boundary flag pass for caller-prepared samples in the 313-input
feature family. It does not create character cuts, prepare native coordinates,
mark intersections or supply the full network vector. Analytic and 641 original
reference cases cover plateaus, short-coordinate extremes, multiple strokes,
the statically confirmed first-Y carry and sample bounds. Regenerate the
vendor-free controls with `tools/LegacyDirectional.Probe/reference_controls.py`
and pass their JSON path to `LegacyDirectional.Probe`.

`LegacyDirectionalStatistics` adds the 17 checked global integer channels for
caller-prepared geometry and intersection classifications. Independent own
controls cover signed covariance, dynamic scaling and integer arc reduction;
unprepared coordinates, guessed intersections and the full 313-vector are not
supplied by this API. See [the restored passes and remaining work](../../docs/legacy-russian-directional-profile.md).

`LegacyDirectionalCoordinatePreparation` and `LegacyDirectionalResampling`
prepare power-of-two coordinates, constant-step physical strokes and native
dimensions for the directional family. `LegacyArcLengthWindows` partitions
prepared cumulative physical lengths. Independent own controls cover 655
coordinate/resampling and 640 window cases; intersections, local channels,
baseline/context assembly and joined-word decoding remain unfinished.

`LegacyDirectionalIntersections` consumes prepared samples and restores ordered
spatial-grid crossing flags and self-loop intervals; the underlying
`LegacyQuantizedSegmentIntersection` preserves the checked fixed-point
arithmetic. Together they pass 4,736 own pair/classification references plus a
complete prepared-ink smoke path. See [the intersection checks and limits](../../docs/legacy-russian-intersections.md).
