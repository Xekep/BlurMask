# Full Russian MAIN acoustic arithmetic diagnosis

This is a development snapshot of `b32ff5e`, before the directional producer
correction. Its independently checked stages explicitly exclude the313-channel
feature producer. Later static review found a native2B1 selector and a local
extent policy that the current calibrated2B0 path does not implement. The
downstream arithmetic agreement below does not validate those input features.
See the [current restoration status](restored-text-engine-status.md).

No numerical mismatch was found in the tested MAIN acoustic output/cost stages.
The fixed261 inputs produce27,866 actually evaluated D0 groups. Independent
arithmetic matches all20,063,520 original family probabilities,5,015,880 raw
integer costs, and5,433,870 final195-entry class-cache values. Dynamic geometry
weights are independently bit-exact for all27,866 groups (maximum ULP difference
zero), including when used to recompute the original raw costs. This is negative
evidence about these stages, not improved handwriting accuracy or a completed
recognizer. Word output remains exactly the existing137/261 diagnostic baseline.
No labels, target-driven thresholds or alternative coordinate flips are used.

## Original owner and recorded data

The NEW-only probe calls the actual reusable context's `PrepareGlobal` and
normal production `Run`. It retains `result.Selection.Publication.ConfidenceOwner`
and its identical source. `Confidence.ReadGroup` reads the existing immutable
`LegacyGlobalMainAcousticSnapshot` produced by the original D0 `Compute` call:
`CopyFamilyProbabilities`, `CopyRawIntegerCosts`, and `CopyFinalClassCosts` return
copies of that evaluation. The probe never invokes `Acoustic.Score` or a second
classifier. Public source/group identity is retained. Group count must equal the
original `AcousticEvaluations`, which must remain unchanged after observation.
All27,866 graph groups were evaluated on this corpus; the unevaluated count is0.
Only the precise documented not-yet-evaluated-group exception permits skipping;
other failures abort rather than produce a partial success capture. No production
hook, reflection, vendor execution or application change was needed.

Normalization, context, boundaries, four feature input vectors and dynamic
weights are recomputed through pure helpers on the SAME retained source. They
are explicitly not original private feature-array captures. These recomputed
inputs drive independent integer/int8/EXP evaluation against the original
probability arrays. That verifies numerical compatibility for these inputs;
it does not independently establish every native upstream feature producer.
The final class cache is original stored data, not a replacement cache from a
second NN evaluation. All observed word fields/UTF8/glyph IDs are compared to
the already verified full261 baseline before any target access.

The supplied recording has one point list per sample. The recorder's
`mouse_release` does not retain pen-up boundaries: this is NOT proof of one
continuous physical stroke. Ynegation follows its pyglet upwards-coordinate
provenance and the library's downwards UI coordinates. The audit proves the
supplied one-list representation, not the missing physical pen-up history.
It invents no release events. Data has already been evaluated and must not be
called an unseen test set or used to tune models while retaining that claim.

## Independent oracles

`audit_output.py` streams one sample at a time, bounds every network's raw
matrix shape through the pinned acoustic file, and independently performs four
int8 networks at195895/283697/367899/467101, signed32 arithmetic, activation,
EXP conversion and integer ensemble costs. It reads the dynamic profile directly
at195771 from the resource and uses the existing independent float32 raw-ink
weight reference. It reports exact equality and ULP counts instead of admitting
numerical tolerance. All observed weights agree exactly. The346MB capture is
not loaded as one Python object. Three truncated-stream controls reject EOF,
and a complete small control is accepted.

`audit_geometry.py` independently checks minX, float32 subtraction/scaling/
truncation/upper clamp and all1,600,173 normalized points. Actual short coordinates
range from -264 to734; signed saturation/overflow extrema are not exercised.
It reconstructs trajectory217 and raster199 plus their baseline9 features,
using the existing independent integer/raster reference with its pixel set
exposed as an additional result. Raster layout is statistics7,baseline9,raster192;
it is not the trajectory's append-only baseline layout. All13,765,804 channels
in the two complete256/238 inputs match. No normalized group is collapsed.

`audit_cache.py` parses the selected AL and21D prefix/tail directly from pinned
resource bytes: prefix ends566330; fallback1, dash weights0, direction divisors
binary32.9, closed weight0, threshold85173, minimum1, limit10, context selector2.
It reuses independently implemented special-symbol/direction corrections, then
performs a literal maxheap preserving equal-cost cutoff identities. The old
10-group oracle's sorted-set shortcut cannot audit this corpus because cutoff
ties occur; this diagnostic explicitly models them. Native static `1BBF80` calls its actual8-byte class heap `09D680`;
that body retains ascending-symbol insertion, rejects equal/worse full roots,
and prefers the right child on equal descending costs. The diagnostic heap
preserves that independently read branch order. It checks all195 final
stored costs, including fallback and quote channels and post-prune soft-hyphen
copy. It does not claim candidate quicksort/publication order or the subsequent
word-heap and language ranking consumer. Source-probability0/one-range and the
resource's disabled related-form/group-smoothing/wide-pair branches are the
actual admitted scope here.

Full independent313 directional feature extraction, native source membership,
unknown physical release events, temporal mutation traces, and later dictionary
request/heap/language ordering remain separate. Passing original NN arithmetic
with recomputed313 inputs cannot be substituted for that producer proof.
Existing scalar directional controls remain relevant but are not described as
an independent full-corpus313 oracle.

## Captures and reproducibility

Managed and Windows NativeAOT captures are byte-identical:346,930,574 bytes,
SHA256 `294933fe8635cab01fb226dab741a106b00dd7b1600274d870a884b88c8e59d2`.
The three complete oracles run on these exact common bytes; duplicate evaluation
of identical bytes is unnecessary. The old raw output projection equals baseline
SHA256 `f949995bd9e497bae710b13d8d1da27c72b1983d270a0cc503056f6de3b812fe`.
All models and input hashes are pinned in the frozen manifest. The final EOF-only
reader guard was checked separately; no successful-capture control flow changed.

The data and reports remain under
`E:/BlurMask-build-artifacts-20261014/review-russian-main-acoustic-20261021`.
No346MB recording/capture is copied into the repository. The NEW probe, three
oracles and this document are the only proposed repository additions. No old
runtime, model, application or CI file changes are part of this checkpoint.

```powershell
dotnet run --project tools/LegacyRussianMainAcoustic.Probe -c Release -- models blind-inputs-yflip.json capture.json
python tools/LegacyRussianMainAcoustic.Probe/audit_output.py models capture.json previous-full261-raw-output.json nn-audit.json
python tools/LegacyRussianMainAcoustic.Probe/audit_geometry.py capture.json geometry-audit.json
python tools/LegacyRussianMainAcoustic.Probe/audit_cache.py models capture.json cache-audit.json
```
