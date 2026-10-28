# Russian first-stage collapse: observed native producer gap

This diagnosis uses the unchanged261 blind inputs from the frozen Russian
comparison. All78 earlier artifact hashes were checked before reuse. A fresh
global capture on commit8cf6fe2 is byte-for-byte identical to the original,
SHA256 `83f9abee51b93e369768e6900c3e449499b2878354e4b80590888591236418ac`.
Inference reads coordinates and identifiers only. No expected text, score,
threshold, beam size or model parameter is changed.

## Earliest four refusals

All four whole-recognition refusals throw `The glyph has no spatial extent`
while evaluating a **first-stage candidate**, before main dictionary inference.
The raw inputs contain102,168,170 and176 points; none is a collapsed raw word.
The same actual smoothing/cut/normalization path produces these candidate ranges:

|Blind identifier|Current physical range, end exclusive|Merged normalized shorts|
|---|---|---|
|heldout-0130|0:156..158|(0,-134),(0,-134)|
|heldout-0199|0:129..131|(0,25),(0,25)|
|heldout-0207|0:109..111|(0,89),(0,89)|
|heldout-0239|0:178..180|(0,-91),(0,-91)|

The observed exception path is `LegacyFirstPreparedAcousticClassifier.Classify`
through `LegacyStrokeFeatures.Extract` to `LegacyGlyphGeometry.Create`. The
temporary E-only failure tap rethrows the same exception and copies its source
metadata; it changes no candidate, source buffer, NN result or winner. The
preceding full failure-stack capture still matches every original output field.
This diagnostic subset is selected by refusal, not expected transcription, and
is not a new accuracy benchmark.

## Exact supported model domain

Each failed candidate retains source6120..187933, SHA256
`aa2af98c8c550603e797f304db204aedd1102482cc8a6ddcc0ed8b62cf8113d2`,
with ordinary21D node13082. Its first feature profile at19121 is
flags1011hex, statistics mask3Fhex, samples30, bins16, factor8, exponent10,
extra setting0 and seven float settings0. Both active slots use the second
network, kind1F/input247 and kind2F/input229. Its serialized normalization
target is0; the existing factory's unchanged effective target is100. This
batch does not change or newly claim that default construction.

All four actual context vectors are28 values-1024 because these one-track
inputs produce no prior physical-stroke windows. Both actual clipped boundary
values are-1024. These are independently produced source channels and must
remain exactly as they are; the fix must not replace them with isolated-glyph
defaults, a zero vector or a selected expected letter.

## Native branch

Text **1B0760** constructs the configured feature object through **00AC70**.
**1B75B0** calls **00AEF0** over its actual prepared geometry, then copies
features throughV88 and appends context and boundary values throughV90.
The complete native bodies are pinned along with the current-version reader
**1C2940**. That reader has **two disjoint Ghidra regions**; both are checked.

**007470** computes the integer geometry's X and Y extents. When both are0 it
setsD0=1. **00AEF0** detects this flag and calls **00A9F0**, returning without
the ordinary resampling/division path. This is a real model feature producer,
not a recognition fallback. It applies to the all-identical normalized short
candidate even when the original raw word has a nonzero extent.

For this exact1011/3F/exponent10 profile,00A9F0 assigns:

- Trajectory217: first six values0;30 pen states1024; remaining trajectory
  values0; final seven statistics `[1024,-768,0,0,-768,1024,-1024]`.
- Raster199: those same seven statistics,64 values-1024,128 values1024.

The native branch does not depend on the collapsed point's absolute Y. It
does not ask a classifier to identify a point before assigning these values.
The actual resource has no mid-nibble baseline group in this feature object.
The English no-initial owner already models the same basic217/199 assignments,
but its source/context ownership is different and must not be reused as a fake
Russian receipt.

`audit_russian_first_degenerate.py` independently checks the exact source bytes,
profile fields, network widths and full native bodies:7 functions,8 complete
regions and3165 instruction pins. It reconstructs every assigned217/199 value.
Vendor binaries are read as data only. Directional313 zero-step/baseline behavior
is separately exported for follow-up; it is **not included in this proposed fix**.

## Proposed bounded runtime change, awaiting review

Only one old production seam is needed:
`LegacyFirstPreparedAcousticClassifier.Classify`, immediately after its existing
same-owner normalization and pen-up merge. A NEW internal kernel may recognize
the all-identical merged-short condition for the **exact trusted model source
fingerprint** above and construct the proven217/199 values. It then uses the
existing28 context channels, existing two boundary signs, same model networks,
same class mask, same integer cost conversion and same cache/source callbacks.
The kernel must not accept caller-supplied source provenance or replacement
feature settings. Generic/synthetic models retain their existing refusals.

For every noncollapsed candidate the old feature calls remain unchanged.
Do not alter `LegacyGlyphGeometry`, the general trajectory/raster APIs, main313
features, model parsing, learned weights, dictionaries, pruning or application
admission thresholds. Cancellation and whole-point/range bounds remain checked;
the new kernel owns its output arrays and mutates no source data.

Required verification before integration: independent complete vector oracle;
actual four failed candidate snapshots; cancellation, foreign/synthetic source
and returned-copy controls; full261 inference with all257 old successful rows
unchanged; actual progress or next refusal reported for the four formerly failed
inputs; unchanged first-model/cache/owned/English regressions; Managed/NativeAOT
parity. Expected labels may be opened only after inference and emission audit.
Newly completed inputs are not assumed correct, and this fix alone does not
establish usable Russian cursive recognition.
