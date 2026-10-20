# Proposed working timed ingress and complete ordinary2DB numerical phase

Reviewable proposal only: no C# implementation, old runtime edits or SDK
execution. It extends the previous incomplete-prefix proposal to a useful NEW
timed input route and the entire admitted ordinary vertical+horizontal phase.
ReadyGeometry means both numerical producers completed, not238B0/DOM/document
acceptance, recognized-text accuracy or a port of all MyScript behavior.

## Exact NEW-only API inventory

Four proposed NEW production files, all in the existing InkRecognition assembly:

1. `LegacyPhysicalRawInkIngress.cs`: immutable public constructor-parameter
   records, a private-constructor receipt, and `Create(strokes, parameters)`.
   It calls existing internal `LegacyOwnedRawInkDocument.Create`, owns that
   exact copied Document, constructs its actual `LegacyFreshRawInkCurrentView`,
   and retains one typed C1C/C49 semantic constructor product per source.
2. `LegacyPhysicalRawInkTiming.cs`: fixed admitted unit records/conversions,
   signed32 channel readers, original C49.V18/V50 and exact C4A/C4B temporal
   resolution for an actual same-document cropped view. No arbitrary label,
   index, source-order or selection ID can be used as a timestamp.
3. `LegacyFreshMainGlobalOrdinaryGeometry.cs`: typed WORD64/CHAR80/line48,
   exact ordinary050660 sequencing, whole-operation preflight and lifetime,
   mutation/failure receipts and complete boundary/aggregate output.
4. `LegacyGlobalOrdinaryGeometryKernels.cs`: private/internal04AA60/04A680/
   04BF30/04B4C0/04CD20/04D2C0/04C340/04C440/04C0C0/04E0E0/
   04DA20/04DD50/04E480 numerical bodies. They receive typed phase data;
   no public unvalidated "ordinary"/"native flags" callbacks.

Public shape, provisional names rather than an ABI promise:

```
LegacyPhysicalRawInkIngress.Create(IReadOnlyList<InkStroke>,
    IReadOnlyList<LegacyPhysicalStrokeConstructor>, CancellationToken)
  -> { Document, CurrentView, TimingReceipt, Validate() }

LegacyPhysicalStrokeConstructor.Uniform(float explicitRateHz, long baseTagMicroseconds)
LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(float explicitFormatRateHz,
    long baseTagMicroseconds, IReadOnlyList<int> actualSampleTimesMilliseconds)

LegacyFreshMainGlobalOrdinaryGeometry.Prepare(actualWordTextParent,
    actualSameParentCroppedSource, actualIngress.TimingReceipt, token)
  -> single-use phase.Run() -> ReadyGeometry or FailedPrefix
```

No old production file needs a seam. Existing internal Create and
`LegacyFreshMainRecognitionContext.PrepareGlobalDocument` are callable from
the NEW file in this assembly and already preserve exact Document identity.
For the actual shared3E/Seed route, use existing public
`LegacyLibraryOriginalTextFieldBootstrap.Prepare(ingress.CurrentView, ids)`
then its real selected-field context and
`LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected`. Those
owners retain the SAME raw Document. The new receipt accompanies this actual
scope; it does not replace the pre-NN scope or create a standalone old owner.
Verify ReferenceEquals all the way to parent.Scope.Document, not equal XY.

All physical parameters are copied/preflighted BEFORE inference. The ingress
really constructs this library's timed raw product. A host may explicitly pick
its own format parameters; this is documented library ingestion policy, not
an allegedly recovered SDK default. No public AttachTiming(existingXYOwner)
factory: old13 parents remain untimed and refuse full geometry. Retrying with
timing means constructing a distinct NEW document and rerunning the genuine
owner pipeline. There is no changed meaning for old InkPoint/InkStroke APIs.

## Constructor and temporal semantics

Uniform mode needs positive finite explicit format rate. Implement float32
1e6/rate, then float32+.5, then signed32 CVTTSS2SI exactly. Reject zero,
negative, nonfinite or out-of-range period before claim. Store the actual
derived signed32 period and supplied signed64 baseTag; C49.V18 is their signed
product with unsigned32 original point index, plus baseTag. An explicit
uniform physical sampler is required; irregular app events must not silently
be declared uniform. No resampling or reconstructed times are hidden here.

MeasuredMilliseconds mode contains one actual signed32 sample time per SAME
stored raw point and an explicit format rate (used by native constructor even
though the admitted time channel controls subsequent reads). Native channel4
is signed32, unit literal `ms`; the C1C target unitE7060 is microseconds.
The NEW bounded literal-unit constructor implements nativeB3A50("ms",1)
fields and B4450 conversion, then the native coefficient→signed32 conversion.
The time multiplier is derived, not supplied as a caller assertion.

New static evidence includes fullB3A50/B42D0/B4450 bodies,335 instructions,
four bindings, the full96-byte target unit and SI-prefix table. B3A50's `m`
prefix sets .001 and `s` selects dimension2 exponent1. TargetE7060 has exponent1
at the same dimension and scale1e-6. B4450 iterates all seven exponents/scales;
its double coefficient truncates to signed32=1000. A fixed literal enum and
its typed constructor receipt avoid accepting arbitrary unit ASTs. This is
an independent bounded implementation of the native leaf semantics, NOT a
claim that generic B42D0's C59/C5A/DE grammar, allocations or failures have
all been ported. If a future API accepts free-form unit strings, that full
parser producer closure is mandatory; it is outside this initial API.

Initial domain: monotone signed32 channel values, exact count, unit ms only,
finite XY and all existing raw-root point/object bounds. Preflight each
signed64 product/addition and every reachable C4B conversion. A conservative
absolute temporal bound below2^52 allows exact integer→double input and safe
signed64 conversion; do not rely on unchecked wrap or platform conversion
sentinels. Arbitrary other units/time channels refuse, not silently convert.

Resolve timing via the REAL cropped view tuple (same owner, WORD/CHAR source,
range ordinal, original raw index and proven crop fields):

* C49.V50 is its constructor baseTag, not its last sample time. C49.V18 at an
  original point reads actual C1C timing; those are different operations.
* C4A.V50 calls original C49.V18 at stored FirstOriginalPoint.
* C4B.V50 when FirstFraction==0 calls that same original V18. Otherwise:
  signed64 first/next→double; next-first; multiply by double(actual float32
  FirstFraction); add first; add .5; signed64 CVTTSD2SI. Preserve native order
  and negative-time rounding, do not use Math.Round or interpolate coordinates.
*04D2C0 invokes V50 ONCE per resolved view and takes scratch+30/+38 min/max
  of these view-start values. It does NOT take the time of the last point.
  CHAR80+40/+48 follows that scratch result; WORD64+30/+38 aggregates actual
  CHAR extrema only after both numerical producers succeeded.

## Complete ordinary numerical producer

Admission is the actual original-range mode1, one selected ordinary WORD/ALT
and one retained source line, with actual positive finite guide height,
ordinary raw C49/C4A/C4B ranges and supported selected rows. Preserve II10000
in WORD flags, CHAR metadata and selected row. Do not exclude MIR/II merely
because a LATER coverage stage may require238B0. No C25/C4C/DOM/prior owner is
manufactured. Reject actual special400 guide-write input before claim in this
initial ordinary admission. Width class1000 and its CHAR write are included.
Additional reachable unsupported categories refuse at their exact gates.

After whole preflight, execute050660 structure and actual UTF8 assignment
gates; source-line memberships; retained F8 angle mean/cos/sin;04B4C0;
04E480; global horizontal extrema; then WORD temporal aggregation. Keep all
written masks/padding honest. Full helper bodies execute conditional branches;
"one line" does not justify bypassing horizontal helpers or all gap handling.

Vertical04AA60 consumes actual WORD original source views for the eligible
same-line WORD branch, otherwise exact eligible CHAR views. Rotate in double,
round each coordinate to float as native, update ordered extrema and double
N/sumY/sumY² in point order; then native variance/std, guide extensions and
line boundaries. No bbox approximation or point deduplication.

Horizontal:04D2C0 initializes native scratch64 and timing sentinels; enumerate
the actual range views in native order; read V50; perform direct or<=20 chunk
point reads;04CD20 uses actual line-height Y clipping and weights1/.7. Its
count is integer, not Ghidra's guessed float.04C340/04C440 use actual04BF30
height, .1/.25 margins,2.8sigma and II10000 half-height.04E480 preserves
integer group indices and -1 sentinels, allocates the actual boundary vector,
copies scratch geometry into CHAR fields, then04E0E0,04DA20 and04DD50 in
native order.04C0C0 spacing and ordinary run/group adjustments are ported;
reached C25-property or transformed-source services explicitly refuse rather
than inventing bounds. Sparse/degenerate denominators and all finite-domain
branches need exact controls and a documented refusal domain.

04E0E0's mean-width predicate can OR02000000 into the actual logical CHAR
state BEFORE later helpers fail. NEW phase-owned metadata records retain
exact parent CHAR node identities, initial flags, current effective flags and
ordered before/after write receipts. All subsequent numerical reads in this
phase use that SAME mutable record. Parent C# metadata stays an immutable
pre-phase snapshot; it is not falsely claimed to have changed. Failed phase
snapshots preserve those writes and consume the claim; no rollback or retry.
The ordinary admission excludes the special400 guide mutation, so its guide
records are retained read-only. A later wider admission needs analogous
guide write receipts before any special input can run.

A consumer after geometry MUST take this typed phase's effective metadata,
not reread immutable pre-phase source flags. Existing selected/coverage entry
APIs are not silently reinterpreted. A NEW geometry-aware downstream join is
an additional reviewed integration step whenever class1000 writes occur;
until then no complete accepted-document route is claimed. Geometry output
itself can be used independently by a host for actual bounds/font layout.

## Lifetime, work bounds and verification

Preflight exact ingress Document/CurrentView/physical source receipts,
parent+cropped identities, Seed, all pending selection identities/order,
Next/Failed, chosen scalar/flags/UTF8/guide snapshots, source/range indices,
branches and allocation/work counts BEFORE the single-use parent claim.
Retain one input-map snapshot, not one per glyph. Reuse actual cropped
<=65536 views and<=1,048,576 point visits, actual raw<=256 objects/4096
total points; characters<=4096. Check all derived buffer/scratch/byte counts
with checked arithmetic and an explicit total byte/work budget. No quadratic
full-document snapshot per CHAR. Build O(1) tuple lookup for already-proven
views without reordering native range enumeration or deduplicating visits.

Ready requires completed WORD64/CHAR80/line48, rotation, both boundary arrays,
moments/scalars and computed temporal aggregates. Cancellation after entry,
allocation/numerical failure, source mutation, foreign owner, wrong selected
scope or disposal invalidates access; prefix capture remains explicitly
Failed and poisoned. Failed prefix ownership must not permit a second phase
on the same actual parent through a recreated wrapper. Use actual parent CWT
key, not an equal-geometry or caller-supplied token. Timing ingress can serve
multiple genuine recognition invocations on its document; their phase claims
are separate exact parent identities, not duplicate global shared-state owners.

Tests: original untimed13 must refuse ReadyGeometry; genuine newly timed9
described below; separately labelled prepared uniform M/MIR/I/II and edge
controls. Independently recompute full typed structural fields/masks, point
rotation/moments/weighted moments, all group/boundary vectors, guide/CHAR
read/write order, timing C49/A/B and temporal aggregates. Include negative
rounding, overflow, exact-vs-equal document, duplicate ranges, repeated views,
native failure-prefix/poison, parent mutations and disposal controls.
Managed/NativeAOT captures must be identical and all old global/EN/RU/
selected/index/cropped captures unchanged. No vendor execution oracle.
Portable Python oracle must implement the numbers independently, not call
the C# kernel or copy expected output scalar values into runtime inputs.

## Real input evidence and application route

The nine committed recorded word tracks already contain `intervalsMs` with
matching `sourceTimingSha256` provenance in the fixed manifest. There are1466
point/interval pairs. A new timed importer uses checked cumulative intervals,
INCLUDING the recorded first interval. It takes the actual first cumulative
time*1000 as C49 baseTag and stores each cumulative time minus that first time
as signed32 channel ms. Thus first channel sample0 and native C49.V50 both
refer to the real first point time; baseTag+channel*1000 reconstructs every
original cumulative timestamp. The clock origin is that recording's start,
not a stroke ordinal, and rebasing does not remove its measured initial wait.
Nominal format rate is an explicit
host constructor parameter, not an inferred acquisition rate. Each word stays
one actual continuous raw object, because penUp metadata is absent. No new
pen lifts, normalization or target labels enter inference. The old untimed
captures remain distinct, even when XY and newly computed recognition match.
Report all nine successes or bounded refusals honestly; this proposal has not
run a new timed geometry inference or measured recognition quality.

For app input, actual retained events need a NEW timing sidecar. Local Avalonia
12.1.2 XML confirms PointerEventArgs.Timestamp and GetIntermediatePoints;
the XML does not settle units/wrap/backend behavior. Initial cross-platform
capture can use one process monotonic Stopwatch clock at actual event delivery,
with an explicitly documented host clock-origin/precision policy. That is
physical delivery-time sampling, not a claim of device acquisition timestamp.
Do not fabricate distinct times for coalesced points without individual times.

Current InkMark stores List<Point>; AddPoint suppresses distances<.65 and
canvas caps points at20000. Record a timestamp only when that SAME coordinate
is actually retained, checking count before/after AddPoint. Current raw ingress
still admits at most4096 total points, so oversized app batches must refuse
or use separately reviewed splitting; do not silently truncate/resample.
Keep the point/time arrays together through AI grouping and coordinate
transforms. Duplicate/dropped points, erasing, cloned marks and remote imports
cannot borrow timing from an unrelated mark. Foreign/imported XY remains
untimed; undo/redo needs the same original sidecar or explicit refusal.

Future app edits, separately reviewed and NOT part of this NEW-only library
batch: CanvasInkSurface.cs and DesktopInkWindow.cs to call a new capture
sidecar at press/accepted-move/release; AiInkLayout.cs to preserve same-source
timing in grouped text input; AiRecognitionModels.cs to expose a new timed
experimental engine adapter. AiAutomaticLayout.cs may need the same adapter
where it bypasses text grouping; inspect and list that exact call before edit.
InkDocument.cs need not change if the sidecar keys the actual InkMark reference
and validates point snapshots. Do not alter old IStrokeTextRecognizer semantics
or activate incomplete new geometry in the app automatically. A NEW public
timed recognizer facade can compose the full admitted independent pipeline;
acceptance/unsupported-domain results must remain visible to its host.

Reproduce the STATIC audit (all SDK access is byte reading):

```
python tools/handwriting/audit_global_timed_geometry_plan.py REPORT.json --native-repo REPO --caller-plan-root FROZEN_CALLER_PLAN --ordinary-plan-root FROZEN_ORDINARY_PLAN --capture global-word-text-managed13.json --recorded-manifest REPO/tools/test-data/russian-recorded-words/manifest.json
```

Optional `--avalonia-xml` reads the installed12.1.2 Avalonia.Base.xml and
records its hash/API names. It does not inspect a running app or establish
platform timestamp units.
