# Reusable prepared word-group neural/context evaluator

`LegacyPreparedWordGroupEvaluator` exposes the existing diagnostic word
recognizer's neural group stage independently of its source-order lattice.
An ordinary source overload takes `LegacyOrdinaryCursiveCutSource`, preserving
its filtered physical geometry, inserted cut ownership and original raw keys.
An explicit overload takes source geometry, prepared ink and chronology; their
provenance remains caller responsibility, and matching prepared counts are checked.

`Classify(ranges)` accepts1..10 supplied pre-merge ranges and returns
`LegacyPreparedGlyphClassification`. A nonlinear candidate graph can supply
its actual range groups without copying the word recognizer's private closure.
The evaluator prepares context windows once and retains them across group calls.
It does not impose a search/group budget, create cache flags, choose accepted
words or manufacture a native confidence threshold.

The stage preserves the prior composed behavior:

1. Sort ranges using `LegacySourcePointChronology` and native signed-key/tie
   ordering, then find the selected float minimum/meanX.
2. Select restored prior context observations using the resource selection mode.
3. Weight/quantize context in the same caller line frame and resolve prepared
   boundary signs under explicit suppression flags.
4. Normalize supplied float ranges before four-family or temporal/spatial-only
   neural inference, resource dynamic weights and native integer acoustic costs.

Whole-line context normalization, zero line/scope IDs and the pre-merge range
limit10 retain the existing **original diagnostic orchestration policy**. They
are not claimed to be the complete native source/frame/controller defaults.
Line baseline/height and other options remain explicit. Restored FF/FF float
normalization is used; source metadata discovery and automatic frame selection
remain separate. No ordinary source probability is converted into a baseline.

`LegacyRussianCursiveWordRecognizer` delegates its group stage to this evaluator;
word search, budgets, final cache and lexical scoring keep their existing behavior.
The actual28-group printed MIR source/final-cache diagnostic retains all prior
top-five words and costs (`то:50888`, `мне:52602`, `мы:54665`, `мир:55698`,
`МЮ:57917`). Its inaccurate first candidate remains documented rather than
turned into an accuracy assertion.

## Controls

`LegacyPreparedWordGroup.Probe` uses actual committed Russian models. Sixteen
own M-glyph fixtures vary scale/translation, duplicate raw ingress points,
baseline-family enablement and start/end suppression. Their normalized shorts
are specified analytically: `(0,0),(0,-100),(31,-37),(62,-100),(62,0)`.
Direct fixed-glyph classification with these independently supplied points and
neutral context must match every available family probability channel,
confidence and integer costs of the prepared pipeline.

Multi-stroke controls require actual accepted context windows, verify that
reversing caller physical-range order still yields the native chronological
geometry/costs, and classify a supplied internal candidate range. Tests also
exercise retained original raw ordinals, inactive family weights, malformed
range/count/frame rejection and cancellation at construction and classification.
These are transport/arithmetic controls with own glyphs, not Russian cursive
accuracy measurements. No vendor DLL or GUI is executed.

```powershell
dotnet run --project tools/LegacyPreparedWordGroup.Probe -c Release -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```
