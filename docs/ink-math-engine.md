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
not the type5f/21e BLSTM adapters used by the current English engine. All eight
networks and sixteen matrix/row-scale pairs were read with exact byte coverage;
an independent integer/LUT prototype passed 32 supplied-vector controls.
The Cyrillic alphabet, including Ё/ё, was extracted. This is model preparation,
not working recognition from raw Russian handwriting.

The old 246/228/255/237/312-dimensional feature extractors, model selector,
classifier-to-alphabet mapping, and segmentation/dictionary decoder remain
unknown. Its 180 classifier outputs must not be assumed to be alphabet IDs or
CTC outputs. The English loader is incompatible. Modern official RU assets still
require signed URLs/cookies; no login or activation checks were bypassed.
No Russian model is distributed or advertised as ready.
