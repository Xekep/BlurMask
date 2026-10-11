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

The probe's explicit offsets apply only to those checked assets. No model,
dictionary or activation bytes are included in the repository. Managed .NET and
Windows Native AOT pass both modes without trim/AOT warnings. The reusable API
has no native dependencies and uses only caller-supplied immutable resources.

Required follow-up includes general automatic segmentation, calibrated ensemble
scores, remaining 313-input features, virtual-label rules and type2d1 language costs.
Joined cursive needs cut-point reasoning inside physical strokes; grouping whole
strokes alone cannot supply those boundaries. None of these missing components
is substituted with guessed letters or embedded expected recognition results.
