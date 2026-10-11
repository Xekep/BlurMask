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

`LegacyAcousticLexicalMapping` shares that bounded Unicode mapping with other
classifier families. Stored related forms keep the strongest source class for
each dictionary label; duplicate aliases do not add probability. The segmented
word probe checks different label spaces, overlapping aliases, absent symbols,
score bounds and cancellation. This remains an original mapping policy, not the
native ensemble or word-case algorithm.

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
coordinate/resampling and 640 window cases; automatic baseline/context estimation
and joined-word decoding remain unfinished.

`LegacyDirectionalIntersections` consumes prepared samples and restores ordered
spatial-grid crossing flags and self-loop intervals; the underlying
`LegacyQuantizedSegmentIntersection` preserves the checked fixed-point
arithmetic. Together they pass 4,736 own pair/classification references plus a
complete prepared-ink smoke path. See [the intersection checks and limits](../../docs/legacy-russian-intersections.md).

`LegacyDirectionalWindowStatistics` and `LegacyDirectionalFeatureAssembly`
restore the checked 3f temporal local channels and calibrated 313-column input
layout. Caller-supplied geometry calibration, raw baseline and context are
required. They pass 1,152 own arithmetic references; this does not supply the
4f alternative layout or enable Russian cursive recognition in the app.

`LegacyDirectionalBaselineStatistics` restores nine raw baseline channels from
an explicit caller baseline, with 640 own numerical controls. The composed
`LegacyTemporalDirectionalFeatures` connects these passes to the temporal313
input. Its optional real-model diagnostic currently has weak class accuracy;
automatic baseline/context inference and the full recognition profile remain
unfinished. See [baseline mapping](../../docs/legacy-russian-directional-baseline.md)
and [model diagnostic limits](../../docs/legacy-russian-directional-model.md).

The checked prior-context path now has separate components for
[`LegacyContextGeometry`](../../docs/legacy-russian-context-geometry.md) (30 raster/geometry channels),
[`LegacyPriorContextSelection`](../../docs/legacy-russian-prior-context.md) (line/scope and target-stroke exclusion),
[`LegacyContextWeighting`](../../docs/legacy-russian-context-weighting.md) (28 weighted channels) and
[`LegacyFloatInputQuantization`](../../docs/legacy-russian-float-input.md) (model-scaled float input).
They pass 2,304 own arithmetic/range references, including Windows Native AOT.
[`LegacyContextWindowScorer`](../../docs/legacy-russian-context-windows.md)
composes fresh candidate windows, the initial context network, selection,
weighting and fixed conversion for one explicit caller-supplied line. The real
RU diagnostic now sends changing prior context into the directional classifier.
Automatic line geometry, the full model ensemble and joined cursive cuts are
still required; these components do not enable full Russian handwriting in
the app.

The alternative spatial family now has checked
[`LegacySpatialWindows`](../../docs/legacy-russian-spatial-windows.md) membership,
`LegacySpatialWindowStatistics`, `LegacySpatialFeatureAssembly` and
[`LegacySpatialDirectionalFeatures`](../../docs/legacy-russian-spatial-model.md).
It composes a complete 313-column input using caller-supplied spatial calibration,
baseline and prior context. The spatial probes pass 1,792 independent arithmetic
controls in managed code and Windows Native AOT. Real model diagnostics on own
printed ink show different strengths from the temporal classifier, requiring
the remaining ensemble, lexical/case handling and joined-word segmentation.
This remains a headless experimental path, without Russian AI-pen integration.

[`LegacyAcousticEnsemble`](../../docs/legacy-russian-acoustic-ensemble.md) restores
the inspected mode1 per-family/per-class logistic calibration and native-order
mean for explicitly active families. Caller-owned coefficients are required;
raw probabilities are not averaged. Managed and Windows Native AOT pass 640
independent arithmetic references. Native case policy and representative
joined-word recognition remain unfinished.

The [word diagnostic](../../docs/legacy-russian-directional-words.md) now runs
all four inspected families with explicit caller line/baseline and context,
using the shared baseline trajectory/raster APIs as well as the temporal/spatial
composers. Own scaled/translated printed МАМА ranks мама first in all four
variants; МИР still ranks миф first and мир second. These narrow diagnostics
expose the remaining lexical/case and joined-stroke dependencies rather than
claiming full Russian recognition. CI also runs its resource-free pipeline as
a standalone Native AOT executable on Windows, Linux and macOS.

`LegacyDirectionalWordRecognizer` exposes that experimental word path as a
reusable model/context/dictionary module with immutable coefficient snapshots,
bounded ink, explicit source baseline and cancellable work. It returns word
candidates and diagnostic counts. The original confidence-to-cost adapter
remains separate from the native word-cost channel.

[`LegacyWordLanguageModel`](../../docs/legacy-russian-word-language.md) restores
the stored Russian word classes and integer word/unigram/bigram lookup tables.
The raw tables pass own format/boundary controls and 512 independent actual
resource lookups. Native weighting and history policy still need restoration
before using these values to rank recognized text.

[`LegacyLexiconWordIndex`](../../docs/legacy-russian-word-index.md) now aligns
dictionary terminals with the language model's stored word IDs using checked
terminal preorder, preserving terminal prefixes before descendants. The real
dictionary has 439,826 terminals; independent word-ID/cost controls distinguish
this order from BFS node IDs.

[`LegacyWordLanguageCostProfile`](../../docs/legacy-russian-word-cost-profile.md)
restores the stored four-profile scalar word and class calibration with an
explicit runtime scale. [`LegacyAcousticFixedLog`](../../docs/legacy-russian-acoustic-fixed-log.md)
restores the separate integer acoustic log-cost conversion across all 32,769
probability values without embedding a vendor table.
[`LegacyAcousticCostEnsemble`](../../docs/legacy-russian-acoustic-cost-ensemble.md)
combines seven explicit weighted family slots with checked native scalar
normalization; its integer costs remain separate from confidence probabilities.
Automatic native profile selection,
history generation and full acoustic/lexical search are still unfinished; these
components do not enable Russian cursive recognition in the GUI.

[`LegacyRussianModelBundle`](../../docs/legacy-russian-model-bundle.md) now loads
the complete checked resource/profile data from `models/ru-RU` with no vendor
DLL or private activation/calibration files. It exposes the experimental word
recognizer and stored language components with copied caller buffers and bounded
profile validation. [`LegacyLineScale` and `LegacyLineGeometry`](../../docs/legacy-line-geometry.md)
restore scale estimation from prepared height records and prepared-cluster line
materialization; producing those records/clusters from raw ink is still separate.

[`LegacyInkRangeExtraction`](../../docs/legacy-russian-point-ranges.md) restores
point ownership and clipped glyph boundary flags for supplied interior stroke
ranges. [`LegacyWordAuxiliaryProfile`](../../docs/legacy-russian-word-auxiliary.md)
restores explicit boundary/category history costs and their checked resource
factory. The bundle exposes all four auxiliary profiles. Automatic cut generation
and total native search still need restoration.

[`LegacyLineScaleSamples`](../../docs/legacy-line-scale-samples.md) extracts the
checked extrema height samples from float32 ink, including plateau/endpoints and
the explicit half-X profile. This supplies another upstream geometry component;
automatic line clustering remains unfinished.

`LegacyDirectionalGlyphClassifier` joins supplied interior point ranges and
clipped boundary flags with all four directional neural families. It preserves
calibrated confidence and raw family probabilities separately; integer acoustic
costs use the raw channel with explicit weights/multiplier. The existing word
path now calls this same classifier. Sixteen model diagnostic results remain
identical after the refactor. `LegacyRussianModelBundle.CreateGlyphClassifier`
exposes it independently of word cuts or dictionary decoding.

[`LegacyWordLanguageScorer`](../../docs/legacy-russian-word-scorer.md) combines
the checked known-word lexical terms, including category/boundary flags and
zero/one class history, with an explicit profile and runtime scale. The bundle
exposes it through `CreateLanguageScorer`. [`LegacyCutBoundaryPolicy`](../../docs/legacy-russian-cut-policy.md)
restores one sampler's threshold/geometry promotion and cut/join cost decisions;
its prepared-ink positions are not automatically original-stroke point indices.

[`LegacyCursiveInkAtoms`](../../docs/legacy-russian-cursive-atoms.md) restores the
actual Russian sampler297 ownership/composition rules. Interior adjacent atoms
share one endpoint; recombining them preserves a continuous physical stroke.
Producing cut markers from raw ink remains a separate geometry pass.

[`PreparedLineBoundaryClassifier`](../../docs/legacy-line-neural-classifier.md)
loads the two embedded42-input affine/BLSTM networks from `models/line-geometry`
directly, sharing the generic CPU executor with the ink recognizer. It consumes
prepared pair features and does not infer font size or baseline. Existing
float/int16/int8 golden controls still pass; independent NumPy controls verify
both real and synthetic networks, repeated/concurrent runs and malformed data.

[`LegacyLineStrokeStatistics` and `LegacyLinePairFeatures`](../../docs/legacy-line-pair-features.md)
now connect raw float32 strokes to the checked 17 statistics and 42 neural inputs.
Full group construction and automatic baseline selection remain separate.
[`LegacyCursiveMarkerGeometry`](../../docs/legacy-russian-cursive-marker-geometry.md)
and [`LegacyCursiveCategoryProfile`](../../docs/legacy-russian-cursive-category-profile.md)
restore selected prepared-marker passes and the stored category graph. Cut indices
address prepared ink, which may include interpolated points.
[`LegacyCursivePreparedInk`](../../docs/legacy-russian-cursive-prepared-ink.md)
provides that owned buffer, exact forward deltas/lengths, explicit point insertion
and source-index mapping. Candidate insertion locations remain caller supplied.

[`LegacyWordCandidateScorer`](../../docs/legacy-russian-word-candidate.md) restores
the ordinary known-word path's lexical budget and total-cost acceptance, including
negative offsets and strict full-heap ties. `LegacyIntegerLexicalDecoder` provides
an original bounded dictionary beam with a terminal scoring callback before final
pruning. `MapIntegerCosts` preserves integer acoustic costs; no confidence-to-cost
conversion is substituted. This decoder is not the complete native SDK search.

Releases also attach three independent `.nupkg` libraries and the separate
`InkRecognition-models.zip` catalog. Add the packages to a local NuGet source;
the model archive retains the same `models/` paths used by the repository.
See [the package readme](PACKAGE.md) for loading the Russian model bundle.

The checked [ordinary source-profile forwarding](../../docs/legacy-russian-word-source-profile.md)
reads source kind `0x135` from the acoustic resource and selects stored language
profile2. `CreateResourceLanguageScorer` reproduces that branch while keeping
runtime scale explicit. It does not infer global recognition modes or history.

[`LegacyLineArcMoments`, `LegacyPreparedLineGroupCost` and `LegacyFreeLineGroupSearch`](../../docs/legacy-line-group-search.md)
add arc-weighted raw stroke moments, projected native group costs and free
row-count dynamic programming over explicit prepared candidates. Independent
exhaustive partition checks verify the search minima. Candidate merging,
the supplied row-lattice branch and full cluster fitting remain unfinished.
