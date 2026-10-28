# Experimental segmented Russian word path

`LegacySegmentedWordRecognizer` connects restored raw-ink features and network
scores to the [packed lexical backend](legacy-russian-lexicon.md). It takes
**already separated glyphs** in reading order; each glyph contains one or more
complete physical strokes. It is not enabled in the application UI and does not
claim to recognize general Russian cursive.

The narrow pipeline uses the checked 229-input raster profile (kind 0x2f), empty
prior context, caller-supplied activation tables and EXP-normalized class
probabilities. Acoustic canonical labels are converted to Unicode text, then to
the distinct lexical label space. A bounded beam search rejects impossible
dictionary prefixes and returns terminal word paths. Result scores are summed
acoustic log probabilities, not calibrated confidence or language-model scores.

With locally supplied legacy RU assets, the own four-point printed glyphs **И**
and **П**, supplied as two separate physical strokes, produce the real dictionary
result **ИП**. The original `LegacySeparatedInkSegmenter` groups their separated
horizontal intervals into glyphs, then the recognizer evaluates them. The best
path score is −1.8585377; next paths are **ИТ** (−3.3630040) and **НП** (−4.8671150).
Translation and 2×/3× scale variants give identical rankings, including reversed
physical stroke acquisition order.
This proves a small complete raw-ink→network→alphabet mapping→dictionary path.
Grouping uses only whole strokes; this abbreviation is not a representative
handwriting benchmark or proof of slanted/joined Russian recognition.

An optional `relatedFormMode` constructor argument enables original candidate
expansion through the acoustic resource's stored three-form accessor. It maps
each supplied related form through Unicode into the lexical alphabet, retaining
the strongest source class score for duplicate lexical labels. This avoids
double-counting duplicate triplet entries. It returns dictionary spelling;
these expanded scores are not normalized word probabilities and do not restore
the SDK's native case costs, case preservation or accent selection. The default
remains exact text mapping. At most 512 distinct glyph choices are supported.

Eight own **printed МАМА/МИР** controls (original, translated, 2x and 3x) use
separate physical letters and whole-stroke grouping, including the separate
crossbar of А. The supplied raster model plus optional form expansion ranks
**мама** and **мир** first (scores -5.0654643 and -3.4854462). Exact mapping alone
ranks **МДМА** and **НИР** first on these samples. These are narrow smoke controls,
not natural-handwriting or cursive accuracy measurements. Written uppercase
case is not retained by this experimental path.

`RecognizeAlternatives` also scores caller-proposed complete glyphs spanning
different ordered cut positions, then invokes the original dictionary
[cut-lattice beam search](legacy-russian-lexicon.md). It returns the selected
cuts with each word. A request scores at most 128 glyph alternatives, four per
start cut, keeping each glyph's best 32 acoustic classes. A caller-supplied
nonpositive segmentation log penalty (bounded to −1000..0) adjusts competing
cuts. Cut generation and cost calibration are separate missing components;
the method does not establish general cursive segmentation or confidence.
Own toy controls verify an alternative cut path with reversed label spaces.
The real ИП controls verify the same acoustic scores through competing
caller-provided cuts; their sample glyphs remain physically separate letters.

The default probe uses an original zero-weight toy network, reversed acoustic
and lexical label IDs, and a tiny own dictionary. Equal acoustic probabilities
exercise label mapping and word-boundary enforcement without vendor assets.
It also checks nonterminal rejection, empty input, malformed glyphs, word-length
bounds and cancellation:

```powershell
dotnet run --project tools/LegacySegmentedWord.Probe -c Release
```

For the four genuine-classifier controls, supply locally available inspected
legacy lite RU acoustic/lexical resources and activation JSON:

```powershell
dotnet run --project tools/LegacySegmentedWord.Probe -c Release -- acoustic.res lexical.res activations.json
```

Append `--related-words` to compare exact and stored-form mapping on the eight
own printed word controls. They pass managed .NET and Windows Native AOT. The
vendor-free toy probe also verifies an uppercase-only classifier output with a
lowercase dictionary path and duplicated stored forms, in both fixed-position
and alternative-cut decoding.

Append `--benchmark` to measure 100 repeated alternative-word requests with
reused models (400 actual raster classifier evaluations). The probe reports
elapsed time, current-thread allocations per request, packed dictionary
storage and its own process private memory. This measures the small checked
path; it is not a canvas/application memory benchmark or a large-word latency
guarantee. No GUI is started.

One local Windows x64 Native AOT run measured 100 requests in 52.6 ms,
190,856 allocated bytes/request, 2,910,612 packed dictionary bytes and 31 MiB
private process memory (including the supplied resource buffers). Managed .NET
measured 226.3 ms and 41 MiB for the same diagnostic. These are observed warm
two-letter timings on one machine, not performance thresholds or full-app RAM.

The probe's explicit offsets apply only to those checked assets. No model,
dictionary or activation bytes are included in the repository. Managed .NET and
Windows Native AOT pass both modes without trim/AOT warnings. The reusable API
has no native dependencies and uses only caller-supplied immutable resources.

Required follow-up includes general automatic segmentation, calibrated ensemble
scores, remaining 313-input features, virtual-label rules and type2d1 language costs.
Joined cursive needs cut-point reasoning inside physical strokes; grouping whole
strokes alone cannot supply those boundaries. None of these missing components
is substituted with guessed letters or embedded expected recognition results.
