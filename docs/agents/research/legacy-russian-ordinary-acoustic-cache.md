# Ordinary acoustic class cache

`LegacyOrdinaryAcousticCacheProfile` reads the observed type21d version110
single-profile tail. `LegacyOrdinaryAcousticClassCache` composes the ordinary
post-ensemble cache transformations. These functions do not load a vendor DLL.
The cache is consumed by the separate candidate scorer; raw neural class costs
are not an interchangeable substitute for its entries.

The observed Russian resource has 180 raw neural class outputs, 192 acoustic
alphabet labels, and 195 cache entries. Reader `1b0760` receives the acoustic
alphabet at runtime160 (`param_2[0x2c]`, where `param_2` is eight-byte words),
not an unrelated knowledge object. Its eight stored alphabet header fields
are `[179,140,65535,178,182,180,181,192]`.

## Restored scope

- Ensemble tail `1b9e20` disables header field0 (Russian raw class179).
- `1bb020` optionally adds an explicit 8192 scope penalty, converts the
  horizontal-line prototype into `-`, `_`, weighted U+30FC/U+4E00 and
  directional U+F0002/U+F0003 primitives, then disables the prototype.
- Direction is weighted by segment **length**, with its horizontal sign
  selecting a bucket. At `1bb3da` XMM0 receives dx and XMM1 receives dy;
  call `25bab0` is `hypotf(dx,dy)`, not absolute dy. Vertical segments split
  length equally. The signed rounding and direction divisors can produce
  negative costs: a pure positive-horizontal segment with divisor0.9 adds
  −863 to its prototype cost.
- Whole-source U+F0001 uses an opaque explicit source-stroke probability from
  interface218 virtual60. The producer of that probability is still unresolved.
  A required missing value rejects the request. No baseline/circle confidence
  or constant substitutes for it. A stored-prototype variant sums explicit
  prototype/o/O/0 probabilities instead.
- U+FFFD is disabled. Header field3 moves into fallback slot192 with raw7c
  penalty (actual1 → 8192); its old slot becomes impossible.
- The optional whole-stroke/endpoints-zero rule gives **`.`**, not `-`, the
  minimum class cost minus1. Endpoint metadata and the native scope switch
  remain explicit caller input.
- Extra slots193/194 copy **apostrophe/quotation mark**, not dash/underscore.
  Static initializer `1b0760` strings1802e9b70/1802e9b68 establish their IDs.
- `1bbf80` retains a bounded native maxheap, preserves fallback when clearing
  pruned entries, and retries a relaxed cutoff if fewer than rawEC candidates
  survive. Ordinary ascending-symbol insertion and equal-key identity are
  retained. This is not a replacement sorted-all-candidates policy.
- An explicit caller-provided derived-alphabet pair list performs the missing
  sibling fill `otherCost+1` after pruning. It does not add new shortlist records.
  The native C0/C8 pair-list producer is not yet recovered; an empty list is
  an explicit caller choice, not a proved Russian default.
- Actual Russian raw100 is0, so native `1bc900` power scaling is inactive.
  `1bc680` ordinary shortlist costs use signed native middle-pivot quicksort.
  The final tail copies `-` into **U+00AD soft hyphen**, not U+2010: static
  initializer1802e9bc0 contains UTF-8 C2 AD.

`enabledSymbols` is an explicit 192-entry mask. Family/class language-scope
selection is not guessed. The builder copies all retained memory and bounds
glyphs, point count, cache width, explicit scope labels and pair lists.
It preserves the native fallback sentinel-plus-penalty value where applicable;
values at or above the sentinel are not viable candidate classes.

## Unsupported branches

This component covers ordinary candidate kind0, without an additional
recognizer/external hook, candidate-prior correction, or enabled confidence
pruning. The source/knowledge-related minimum rule is rejected when requested:
its native derived relationship policy is not yet established. Kind2's special
three-symbol shortlist replacement is outside the ordinary branch. Runtime350
confidence pruning (`1bcbd0`→`1be1a0`) and candidate posterior normalization are
not implemented here; a caller cannot treat this component as that policy.

The profile reader rejects compressed nodes, size-penalty rawB8, grouped
smoothing rawC8, obsolete nested conversion, and nonzero raw100 power scaling.
The actual checked Russian resource has all of these branches disabled. The
reader does not extrapolate that profile to arbitrary resources or SDK settings.

## Verification

`tools/LegacyAcousticCache.Probe/reference_controls.py` creates vendor-free
vectors. A simple independent sorted-set oracle checks 768 unique-cost pruning
cases; 512 independent scalar hypot/float/log controls check direction costs;
32 synthetic serialized profiles exercise the reader. Hand-written regressions
check special string identities, negative costs, fallback, native tie identity,
post-prune pair fill, dot and quote-buffer behavior, soft-hyphen copy, missing
probability rejection, unsupported profiles, malformed inputs and cancellation.
Managed and Windows Native AOT runs pass these controls.

Optional actual-resource diagnostics normalize three **own synthetic printed**
strokes for М/И/Р, baseline90, height80, target100, four resource-weighted
families, explicit neutral context and caller +1024/+1024 boundary flags.
The opaque-probability primitive is explicitly disabled for this diagnostic.
Top raw/cached entries include `m:8692, м:13057`, `n:13761, и:14401`, and
`p:13377, …, р:24299`. A doubled/translated source gives identical scores.
Boundary −1024 changes those rankings. These are transport/frame diagnostics,
not representative handwriting accuracy, a proved language mask, or evidence
of completed Russian cursive recognition. Confusable Latin/Cyrillic classes
and the missing pair/scope producer remain important integration gaps.

```powershell
python tools/LegacyAcousticCache.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacyAcousticCache.Probe -c Release -- controls.json
# Optional model diagnostic:
dotnet run --project tools/LegacyAcousticCache.Probe -c Release -- controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```
