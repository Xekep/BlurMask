# Actual Russian MAIN2B1 producer, revision 3

The pinned Russian bundle selects the actual native2B1 producer. Revision3 adds
its correct BC280 landmark pass and an explicit prepared-sample facade bound.
This is a native correctness change, NOT a quality improvement or finished
Russian cursive. Strict fixed-dataset132/261 and342 edits remain below the
historical generic2B0 result137/325. No model weights, beams, thresholds or
expected-candidate selection changed.

CI input identity correction, 23 October: the four procedural controls are
unchanged. Their JSON fixture can have CRLF locally and LF after Git checkout.
The small-capture oracle now checks the capture's hash against the actual
fixture bytes, pins the parsed fixture independently of whitespace, and checks
the exact ordered sample IDs. Both newline forms pass; changed input points,
wrong hashes, reordered/missing IDs and a true labels-read flag fail. The saved
four-case capture still reproduces all61920 probabilities and15480 raw costs
exactly. This fixes a test portability failure, with no production or accuracy
change.

## Native variants and owned production

Exactly TWO old production files change: LegacyRussianModelBundle verifies
actual raw88=1/89=2 and all1096 supplied calibration values against owned native
defaults, then selects an internal factory; LegacyDirectionalGlyphClassifier
preserves its PUBLIC generic2B0 constructor and uses the new producer only for
the actual fixed-hash bundle. Basic256/238 families, masks, context and costs
retain their existing implementations. CreateWordRecognizer remains the
explicit historical separated-glyph/generic2B0 experiment, not global/app.

NEW LegacySaturatedDirectionalFeatures shares common Prepare(strokes,100,false)
between temporal and spatial families. It explicitly checks1..255 PREPARED
samples before borrowed consumers. Native2B1 stores ushort memberships; this
facade deliberately retains a narrower byte-consumer subset. Raw4096-point and
15-stroke budgets remain distinct from the prepared bound.256 identical RAW
points validly collapse to one prepared point and are not falsely rejected.

The NEW language-neutral LegacyDirectional2B1Landmarks is a separately frozen
shared dependency, SHA0ca2d654962f31f78fac668e34ef44eb5ad40e86610c6056adeb8e220d879e68.
ActualBC280 loads point1Y before point2; old genericB6D20 deliberately retains
point0Y. The generic class and its negative-control expected bytes are unchanged.
The helper returns the existing owned result, checks whole point budget BEFORE
addition, and rejects empty physical strokes. LegacyInkPoint stores signed
short X/Y, so coordinate subtraction safely promotes to int. No unreachable
int-domain guard is invented.641 independent literal BC280 controls plus analytic
reversal/plateau/physical-boundary/cancel/budget cases pass Managed/NativeAOT.

Both whole and LOCAL no-unit flags are false. The shared2B1 assembly saturates
transformed values, calibrates their int16 results at1..273, saturates again,
then appends baseline9/context28/boundaries2. Its default catalog and assembly
are unchanged from the separately frozen shared English implementation.

Revision1 omitted the whole no-unit branch; revision2 borrowed the wrong generic
landmark variant. Both freezes remain immutable and were blocked from Russian
main integration. Neither is described as complete native producer evidence.
The exact19 complete bodies/seven paired helper reviews/10 landmark instructions
are in directional-2b1-helper-pairs-20261021.md. Other helpers preserve formulas
in the admitted <=255 subset while widening native index/class storage. Their
manual semantic review and exact bytes are NOT formal whole-program equivalence
or transitive alias analysis. Unsupported overflow/allocator domains refuse.

## Actual outputs and independent checks

The probe executes actual PrepareGlobal/Run and inspects original retained D0
NN/cost snapshots without replacement inference. Its inputs are RECOMPUTED from
the same retained source, not captures of private input arrays. The Python
reference explicitly selects literal BC280 independently of the English wrapper;
its separate negativeV2 reference explicitly selects genericB6D20. No shared
implicit default decides which native variant is being audited.

All261 inputs yield27866 original NN groups, zero unevaluated groups. Managed
and NativeAOT full capture match347010527 bytes, SHA256
31ad0ebe0fcea0c143f7770402a28ef19034bf41b6097e4a74a10e7e3959dae7.
Independent checks pass20,063,520 original probabilities,5,015,880 raw costs,
all27866 dynamic weights with ULP0,15,772,156 directional geometry/baseline shorts,
13,765,804 basic channels/1,600,173 normalized points, and5,433,870 final cache
costs.1,671,960 context/boundary checks use retained owner values; they do not
constitute a new independent context-producer proof. Later lexical/history/DOM
acceptance is outside this feature proof.

19 guard/admission/ownership checks and NINE procedural cases pass5634 short
and3240 actual-model probability comparisons. The early-Y case is a deliberately
geometric one-unit reversal after normalization: it differs from wrongV2 in8
shorts/197 probability entries, without text labels. Whole-unit negative controls
still distinguish48x64 from64x80 and64x48 from80x64. Control capture Managed/AOT
SHA3e52ad77291e0a266defde435edba036ba3c792879a454f274d7938d2f90bac9.

Four committed procedural input cases inspect86 actual NN groups and explicitly
retain one stored/unevaluated graph group. Independent61920 probabilities,
15480 raw costs,48676 geometry/baseline shorts and5160 owner channels pass.
Small Managed/AOT SHA9bc1302bfb342442f7d4f3fe5ed3224329b24225382b9a5d6956111632724855.

Exactly ONE old TEST helper changes: the strict streaming samples() reader in
LegacyRussianMainAcoustic.Probe/audit_output.py fixes a refill-leading comma.
Its formulas are unchanged.23 generated controls and byte-exact historical
numeric-report evidence are frozen in V1/V2; the same unchanged reader also
passes this full261 audit. Generic directional/prepared/word checks and existing
FIRST-owned4/57NN, MAIN-owned315B8/14rows/5IRLS independent checks pass freshly;
the last two captures remain b9fdedaf... and a48b16e3.... Parent owns affected
actual-model CI collector repairs and application integration checks.

## Fixed-dataset quality and limits

Production/capture/oracle hashes froze before reopening previously seen targets:
inference-before-targets.json SHA256
f53bcdee86f44d4b4938b6e0c795c6cd0831a0bd2b5de589911651da3d2f2cf7.
This is a fixed-dataset replay, NOT a new unseen/blind evaluation. Strict results:

|Stage|Exact /261|Edits /1276 characters|Host accepts|Correct/wrong accepted|
|---|---:|---:|---:|---:|
|Historical generic2B0|137|325|122|93/29|
|Intermediate V1|131|345|117|90/27|
|Blocked intermediate V2|132|342|121|92/29|
|V3 actual BC280 subset|132|342|121|92/29|

Against historical baseline16 raw outputs change:3 improve edit distance,11
worsen it and2 have equal distance. Host Managed/AOT261+11guards match bytewise
SHAe6ce3bf3d046044232b261270c6504cabbdc4c7e4c0a9ac2a10615788cf5d7b7,
identical to V2. The .5 host gate remains uncalibrated; diagnostic30s budget is
separate from the unchanged normal UI2s budget. Internal arithmetic correction
therefore does not establish better selected words or readiness.

29 first-word targets start with Latin U+0063 rather than Cyrillic U+0441;
case/yo caveats are separate from strict metrics and were not normalized away.
The primary recorder discarded physical pen-up markers, so supplied one-list
geometry is not proven a continuous physical stroke. No fake boundaries were
introduced. Vendor-training overlap is unknown. Full native document acceptance,
all profiles/helper branches, English word integration and a ready release are
not claimed by this batch.

CI commands (substitute native executable for Managed):

    dotnet run --project tools/LegacyRussianMain2B1.Probe -c Release -- MODELS --controls controls.json
    python tools/LegacyRussianMain2B1.Probe/audit_controls.py MODELS controls.json report.json
    dotnet run --project tools/LegacyRussianMain2B1.Probe -c Release -- MODELS tools/LegacyRussianMain2B1.Probe/small-inputs.json small.json
    python tools/LegacyRussianMain2B1.Probe/audit_output.py MODELS small.json report.json --small

The small file has canonical LF bytes pinned as input provenance. Numerical CI
oracles do not whitelist platform-dependent whole output JSON hashes; hashes
above identify the scoped frozen Windows evidence only. Large captures remain
under E:/BlurMask-build-artifacts-20261014/review-russian-main2b1-v3-runtime-20261021.
