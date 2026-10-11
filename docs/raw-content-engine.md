# Local raw-content analyzer

`src/InkRecognition.RawContent` is a reusable, UI-independent Native AOT-compatible
module. `RawContentAnalyzer.Load(stream)` loads a private `.ikrraw` pack;
`Analyze(strokes, cancellationToken)` returns per-stroke classifications and
text/math regions, referencing the original stroke indices. It uses the same
native CPU ONNX Runtime as the formula decoder, with one inference thread. No
MyScript DLL, activation mechanism, Python process or network request is used.

The observed Raw Content2 4.5 `dl-raw-content.res` contains one 5,341,760-byte ORT
graph. Its metadata defines 14 categories: connector, cross-out, filling, free
drawing, grouping, math, plot axis, scratch, separator, shape, strike-through,
table grid, text and underline. It accepts `node_features[N,11]`,
`edge_index[2,E]` and `edge_features[E,16]`; outputs are category and connection
**logits**, not probabilities. Category confidence uses stable softmax. Regions
join confident matching text/math categories through mutual positive connections.
Decorations, drawings and uncertain strokes remain ink.

This follows the role described by the [official content-type documentation](https://developer.myscript.com/doc/interactive-ink/4.3/overview/content-types/):
classification/segmentation is separate from text and math recognition. Installing
this model does not supply a Russian decoder or a symbolic mathematics solver.

## Features and limits

The recovered node11bctypesetalphabet / edge16bc profile uses the INK alphabet
index111, polyline length, convex-hull area, centroid-ray angular dispersion,
endpoint ratios, bounding boxes, center distances, curvature and chronology.
Curvature is the sum of absolute approximate tangent turns divided by pi. The
scaler selects the upper median nontrivial raw bounding-box diagonal, floored at
.1; distances/areas are normalized before signed square-root features. Spatial
neighbors are bounded at20 per stroke, plus self/adjacent temporal edges.

The observed geometric distance is zero for crossing polylines and otherwise the
minimum sample-to-sample distance. A bounding-box tree preserves this metric
without allocating a quadratic distance matrix. Inputs are capped at256 strokes,
20,000 points and finite coordinates within ±1,000,000. Work is cancellable.

This is an **experimental recovered profile**, not a complete copy of MyScript's
Raw Content engine. SDK incremental graph updates, semantic grouping, recognition
feedback, decoration editing and lexical context are not implemented. Graph
neighbor ties, floating-point rounding and segmentation thresholds have not been
validated against the complete vendor pipeline. Recognition starts on pen
release and considers the bounded current ink fragment, including retained
original ink behind its unchanged live text, rather than the whole document.

## Model installation

The inspected resource is stored at
[`models/raw-content/raw-content2.res`](../models/raw-content/raw-content2.res).
The exporter accepts its 4.5 fingerprint, rejecting unverified future profiles.
Installers use a separately installed `.ikrraw` pack.

```sh
python tools/export-raw-content-model.py models/raw-content/raw-content2.res .artifacts/raw-content.ikrraw
BlurMask --install-raw-content-model .artifacts/raw-content.ikrraw
```

The same pack can be installed in Settings. Automatic AI mode then partitions
text and formulas before decoding. Explicit Text/Formula modes remain overrides.
The pack lives at `<LocalApplicationData>/BlurMask/models/raw-content.ikrraw`.
Install validates bounded size, checksum, profile and an actual CPU inference
before atomically replacing an existing model. A failed load is cached until the
file changes, avoiding repeated expensive warm-up attempts.

## Verification

`dotnet run --project tools/InkRecognition.RawContent.Probe -c Release` checks
geometry, crossing lines, curvature, normalization, region/category separation,
one-way connections, uncertainty, cancellation, bounds and memory growth.
`--runtime-native /path/to/onnxruntime` verifies the native three-input/two-output
ABI using an independently constructed **synthetic** graph, not vendor weights.
The 3,984-byte embedded fixture has fixed TEXT logits and connection logits;
it verifies transport/lifetime and cannot recognize handwriting.

Actual caller-supplied weights were verified locally on the official public
`helloOneStroke`, `helloMultipleStrokes` and equation pen fixtures, and a combined
14-stroke text/formula batch. The combined batch yields one TEXT region (5strokes)
and one MATH region (9strokes). `BLURMASK_RAW_TEST_PACK`,
`BLURMASK_RAW_TEST_INPUT`, `BLURMASK_RAW_TEST_RUNTIME` and optionally
`BLURMASK_RAW_TEST_TEXT` exercise the actual model through application dispatch,
replacement and undo without opening a GUI. This small corpus is not a universal
accuracy measurement.
