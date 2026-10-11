# Independent Math stroke recognizer

`src/InkRecognition.Math` is a reusable .NET 10 library. It converts ordinary
pen strokes into 26 features, runs the three Math2 graphs, and performs greedy
autoregressive decoding. It does not load MyScript DLLs, run Python, or use the
old raster recognition models.

The C# feature pipeline matches eight independent float32 fixtures exactly,
including public handwritten `1`, `y=3x+2`, and a two-row radical expression.
The complete Windows x64 Native AOT CLI recognizes all three with EOS. This is
evidence for these controls, not a claim that the vendor SDK has been recovered.

## API

```csharp
using var model = File.OpenRead(userInstalledModelPath);
using var recognizer = StrokeMathRecognizer.Load(model, nativeRuntimePath);
MathRecognitionResult result = recognizer.Recognize(strokes, cancellationToken);
```

The recognizer is disposable and serializes calls. It keeps native sessions and
tensor buffers in memory; no model files are extracted to a temporary directory.
Cancellation is checked around preprocessing and decoding and terminates a
running ORT call through native run options. Disposal waits for the active call.
`Confidence` is the geometric mean of selected token softmax probabilities,
excluding EOS; it is not a calibrated correctness probability.

`Text` preserves recognized structural tokens. `LaTex` is produced by a generic
bounded token parser for radicals, fractions, scripts, tables, and fences. It is
null for unsupported or unbalanced structures. `Completed` is false if EOS is not
reached within 128 steps; such a result has no LaTex. The decoder is greedy;
vendor beam search, grammar constraints and forced segmentation are not ported.
Input is limited to 256 strokes, 20,000 points and 6,000 processed frames.
Glyph objects, external normalization scale and custom allocation are unsupported.

`NumericExpressionEvaluator.TryEvaluate(plainOrLatex, out result)` is a separate
model-free numeric API. It accepts explicit `+ - * / ^`, Unicode operators,
parentheses, decimal comma, LaTeX fractions and roots. It returns an invariant
compact number; variables, implicit multiplication, undefined arithmetic and
unsupported commands return false. Existing numeric equalities must agree.
Limits are 2,048 characters, nesting depth 32, 256 operations, integral root
indices 2–64 and absolute exponents at most 10,000. It uses no scripting engine
or external dependency and is covered by 84 managed/Native AOT guards and goldens.
The application's AI pen calls this API only when the recognized expression ends
with an equals sign and its right-hand side is empty. Recognition without `=` and
already written answers are preserved; the reusable numeric API itself remains
independent of that UI trigger.

## User-installed model

Models are not committed or included in application releases. Export a locally
obtained Math2 `math-sr.res` of the observed v103 ordinary-pen profile:

```powershell
python tools/export-ink-math-model.py path/to/math-sr.res --output path/to/math2.ikrmath
```

The exporter uses Python's standard library only. It accepts the observed
resource pipeline, validates every graph boundary and vocabulary, and writes
`IKRMATH1`: eight-byte magic, little-endian version1/body length, SHA256 of body;
body has five uint32 profile fields (26,297,BOS0,EOS1,profile1), 297 UTF8 labels
with uint32 byte lengths, graph count3, then uint32 length and ORT bytes per graph.
The reader limits the body to 64MiB and graphs to 32MiB, verifies SHA256 and EOF.
Changing normalization/graph shapes requires a new supported profile.

Do not redistribute extracted vendor weights merely because this reader can
parse them. The runtime is separate open-source code.

## Native runtime and AOT

The module calls the append-only C API through unmanaged function pointers.
There is no Microsoft.ML.OnnxRuntime managed package, reflection-based tensor
binding, Python runtime, or GPU provider. ORT CPU 1.23.2 is pinned; the three
graphs contain more than 1,500 nodes including quantized and dynamic operators,
so a complete managed kernel port was not necessary for this integration.

```powershell
python tools/fetch-ink-math-runtime.py win-x64 --output .artifacts/ink-math-runtime
dotnet publish tools/InkRecognition.Math.Probe/InkRecognition.Math.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o .artifacts/ink-math-aot
```

The fetcher downloads official CPU archives, verifies pinned published SHA256,
and writes only the native runtime and license notices under
`runtimes/RID/native`. Supported archive mappings: win-x64, linux-x64,
osx-x64/osx-arm64 (one universal2 archive). Explicit native paths are supported;
without one, the loader checks that directory beneath the application and then
the system library search path. Release packaging should include licenses with
the runtime. Windows ORT is 14,186,016 bytes and depends on the Microsoft VC++
2015–2022 runtime (MSVCP140, MSVCP140_1, VCRUNTIME140, VCRUNTIME140_1), in addition
to OS libraries. It is not CRT self-contained. The optional fetcher argument
`--vc-redist-dir` copies those four dependencies from a supplied Visual Studio
`VC/Redist/MSVC/version/x64/Microsoft.VC*.CRT` directory beside ORT. Their
transitive imports require only those four and Windows UCRT/system libraries.
The loader explicitly searches the ORT directory for dependencies. These files
are subject to the [Visual Studio redistribution terms and REDIST list](https://learn.microsoft.com/cpp/windows/redistributing-visual-cpp-files);
the source path and individual hashes are recorded. Linux/macOS AOT execution must be
verified on those systems in CI; Windows success alone does not establish that.

References: [official ORT release](https://github.com/microsoft/onnxruntime/releases/tag/v1.23.2),
[pinned C API header](https://github.com/microsoft/onnxruntime/blob/v1.23.2/include/onnxruntime/core/session/onnxruntime_c_api.h),
[MIT license](https://github.com/microsoft/onnxruntime/blob/v1.23.2/LICENSE),
[third-party notices](https://github.com/microsoft/onnxruntime/blob/v1.23.2/ThirdPartyNotices.txt).

## Reproducible checks

```powershell
dotnet run --project tools/InkRecognition.Math.Probe -c Release
dotnet run --project tools/InkRecognition.Math.Probe -c Release -- --features fixture.bin
dotnet run --project tools/InkRecognition.Math.Probe -c Release -- --runtime-native path/to/onnxruntime.dll
dotnet run --project tools/InkRecognition.Math.Probe -c Release -- --runtime-checks model.ikrmath path/to/onnxruntime.dll
dotnet run --project tools/InkRecognition.Math.Probe -c Release -- model.ikrmath strokes.json path/to/onnxruntime.dll
```

The probe accepts arrays of raw `[x,y]` points per stroke or iinkTS recorded
stroke objects. Expected text is never supplied to the recognizer.
`--runtime-native` exercises the complete direct C API with an independently
constructed tiny ONNX Identity graph (no vendor weights), including tensor
reading, zero-length caches, inference, termination and releases. It is suitable
for platform CI even when recognition models are unavailable.
Private analysis fixtures, model provenance and Windows results are under
`.artifacts/myscript-analysis/output/MathIntegrationStage5`; they are excluded
from Git and releases.

## Russian text preparation

A legacy `ru_RU-ak-cur.lite.res` was found in an older
[public firmware tree](https://github.com/arter97/back-to-n0t3/tree/27291c8aa479889026345deb39b6ea04efde1709/system/VODB/ru_RU).
The 572,763-byte acoustic resource has SHA256
`7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978`.
Its root type247/version10d/build07000000 contains eight feedforward networks,
not the type5f/21e BLSTM adapters used by the current English engine. Both
type21d/version110 configurations are now parsed to their exact ends, including
seven submodel slots per profile and global tables. All eight networks and
sixteen matrix/row-scale pairs have exact byte coverage. The Cyrillic alphabet,
including Ё/ё, was extracted. No vendor weights or activation tables are included
in the reusable implementation or release.

`LegacyInkNetwork` loads the observed FEA0/version106, exponent-10, row-scaled
int8 MLP format. It preserves scalar int32 wrap, signed-byte weights, hidden bias,
and caller-supplied tanh/sigmoid lookup tables. The **full stored input widths**
are 30, 247, 229, 256, 238 and 313; the old claim of 246/228/etc. omitted a channel
incorrectly. These input vectors have no automatically inserted global bias.

The backward-compatible extractor was found in the existing modern Text DLL.
`LegacyStrokeFeatures` implements integer normalization into a 128×128 grid,
30-point integer resampling across physical strokes and pen-up transitions,
210 pen-state/coordinate/tangent/curvature channels, and seven shape statistics.
The physical/gap allocation and pen-up length are kept separate: gaps influence
sample allocation but do not become raster ink. Inputs are limited to 15 strokes
and 4,096 total points. Unverified degenerate interpolation branches fail closed.
The native small-distance lookup equals `round(sqrt(n))` for every one of its
16,384 entries, so the implementation derives it without distributing that table.
An isolated complete glyph with no preceding candidates uses 28 context channels
of -1024 and two boundary channels of +1024, producing the 247-input first model.

`LegacyRasterFeatures` draws the normalized/resampled physical ink into a binary
128×128 raster and derives twelve 16-bin projection/contour banks. The **seven
shape statistics precede the 192 raster channels**, yielding the 229-input model
with context/boundaries. `LegacyInkBaseline` supplies the original-coordinate
baseline and x-height; its nine features produce 256-input trajectory and
238-input raster profiles. A negative height means unavailable (nine zeroes).
Baseline inference from a word is not yet implemented; these values must be
supplied by the caller. Neither API finds glyph boundaries or decodes words.

Static evidence for that layout is Text `18000ac70` (registration), `180007470`
(normalization), `180007b20` (resampling), `180008290` (trajectory), `180008770`
(statistics), `1801b75b0` (channel assembly), `1801ca5f0` (empty prior context),
and `1800ebb20`/`1800ebb60` (int16 copy and float-to-fixed conversion). Text
`1801bcbd0` maps canonical labels below the classifier width directly to network
slots `label+1`; the extra slot is the output bias. Thus emitted rows 0..179 have
the default canonical alphabet IDs 0..179. Virtual labels 180+ and contextual
overrides still need their separate decoder rules. These are not CTC outputs.
The second configuration's 192×4×2 global float table is model-major center/slope
calibration: `18013e8e0` computes `sigmoid((score-center)*slope)`.

`LegacyClassScores.Convert` restores the full unmasked exponent-10 EXP output
path rather than treating raw dots or sigmoid scores as model probabilities.
Text `1801b86f0` selects type212 slot0x50; `1800e82c0` stabilizes the raw scores,
`180249db0` applies EXP, and `1800e69f0` performs quantized normalization.
Every one of the 352 EXP table entries equals
`floor(25*exp(-4+22*i/351))`, so no vendor table is embedded. The conversion
exposes integer probabilities scaled by 32768 and the native unnormalized
log-score channel (`1800ebd90`). Per-class confidence calibration is separate
from this normalization. Candidate masks, special mixed sigmoid/EXP classes and
complete ensemble ranking are not implemented by this API.

Headless verification passed in managed .NET and Windows Native AOT: synthetic
format/overflow/geometry/cancellation tests, 32 supplied-vector comparisons for
all eight actual networks, 60 single-stroke, 68 multiple-stroke, 70 raster and
420 baseline feature/network comparisons against an independent Python
implementation. Another 329 score vectors check EXP probabilities and log
scores, including overflow rescaling and zero-probability paths. These are
numerical conformance checks, not calls to an activated vendor recognizer.
The checked-in `tools/LegacyInk.Probe/isolated-ru-i.json` supplies original
four-point Cyrillic **И** controls: the actual RU trajectory model ranks
canonical output 70 first for original, translated and scaled variants.
`isolated-ru-pi-raster.json` independently tests **П** (output77) and **И**
(output70) in eight raster-profile variants. These are concrete raw-ink results,
**not a natural-handwriting accuracy benchmark or a recognized whole word**.
Other geometric letters still confuse Latin/Russian lookalikes or symbols when
ranked by one model alone.

Reproduce the vendor-free tests with `python tools/test-legacy-ink.py` and
`dotnet run --project tools/LegacyInk.Probe -c Release`. With locally supplied
resource and activation JSON, use `--glyph-cases resource.res
tools/LegacyInk.Probe/isolated-ru-i.json activations.json`, or `--raw-controls`
with a local reference control file. `--score-controls controls.json` checks
caller-supplied raw/probability/log-score vectors. Private static exports and
controls are
ignored under `.artifacts/myscript-analysis/output/RussianAssetsSearch` and
`RussianExtractorStage2` through `RussianExtractorStage5`.

The [legacy lexical backend](legacy-russian-lexicon.md) now reads the actual
type2b4 Russian word trie and performs bounded dictionary-constrained decoding
of caller-supplied per-glyph candidates. Ten local string/label lookups pass;
the main graph uses 2,910,612 owned packed bytes. This advances word decoding,
but does not supply automatic glyph boundaries or recognize cursive ink.

The [experimental segmented word path](legacy-russian-segmented-word.md)
now connects the checked raster model to that dictionary through distinct
acoustic/lexical alphabets. Four original printed **ИП** examples, including
translation/scale changes and reversed physical stroke acquisition order, rank
**ИП** first after original whole-stroke separated grouping. This advances a
small raw-ink-to-word path, rather than natural-handwriting accuracy. Connected
cursive strokes remain intact; their internal cuts are not implemented.

The remaining legacy 313-input family has a checked 283-channel geometric vector:
one leading bias, 17 global channels (mask 0x1e1fff), 16 windows of 16 channels
(mask 0xe1fff), and 9 baseline channels (mask 0x1ff). Context 28 and boundaries 2
complete 313. `LegacyDirectionalLandmarks` restores one prerequisite pass
(`1800b6d20`) over caller-prepared physical sample points: axis reversals and
stroke ends, preserving the assembly-confirmed first-Y carry. It does not
substitute the other profile's 30-point resampling or generate character cuts.
641 own independently generated controls plus analytic/limits/cancellation
checks pass in managed .NET and Windows Native AOT. Full native coordinate
preparation, intersections, window aggregates and complete 313 vectors remain
unfinished; the comparison did not execute an activated vendor engine.

Remaining work: the two 313-input profiles, automatic baseline estimation,
complete ensemble/candidate-mask handling, cursive segmentation, virtual-label
rules, full language-model costs and representative Russian ink validation.
The English loader remains incompatible. Modern official RU assets still require
signed URLs/cookies; no login or activation checks were bypassed. Russian word
recognition is not enabled in the application or advertised as ready.
