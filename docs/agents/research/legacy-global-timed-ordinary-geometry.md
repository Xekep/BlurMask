# Timed raw ingress and bounded ordinary WORD/CHAR geometry

This NEW-only library phase implements the admitted ordinary branch of native
050660,04B4C0 and04E480. Its inputs are a genuine global WORD/TEXT publication,
the exact original-source cropped views, and a physical timing receipt created
before inference on the SAME raw Document. It produces typed WORD64,CHAR80 and
line48 field snapshots, boundaries, moment diagnostics and ordered mutation
receipts. A Ready snapshot means this numerical phase completed. It does not
mean238B0, document adoption, better handwriting accuracy or a complete engine.

Four new production files implement this batch:

- `LegacyPhysicalRawInkIngress.cs`: explicit constructor parameters, owned
  bounded snapshots and a distinct raw Document/current-view pair.
- `LegacyPhysicalRawInkTiming.cs`: admitted C1C/C49 constructor conversion and
  original/C4A/C4B start-time semantics for actual cropped source views.
- `LegacyFreshMainGlobalOrdinaryGeometry.cs`: exact parent/source/guide lifetime,
  preflight, typed phase ownership, execution order and failure prefixes.
- `LegacyGlobalOrdinaryGeometryKernels.cs`: ordered moments, weighted horizontal
  statistics, ordinary link-chain traversal, boundaries and metadata writes.

No preexisting library file, application code, settings or default engine
changes. The existing XY-only APIs keep their old behavior. A receipt cannot
be attached retroactively to an old Document; equal coordinates do not confer
identity. All thirteen independently rebuilt XY-only parents explicitly refuse
the foreign timing receipt from their same-coordinate timed counterparts.

## Physical constructor policy

The public entry is
`LegacyPhysicalRawInkIngress.Create(strokes, perSourceConstructors, token)`.
It copies one constructor per source before creating the immutable Document.
`Uniform(rateHz, baseTagMicroseconds)` declares an explicit physical uniform
sampler. `MeasuredMilliseconds(formatRateHz, baseTagMicroseconds, timesMs)`
contains one real signed32 time value per stored point. The independent host
chooses these parameters. They are not claimed as recovered SDK defaults.
Source IDs, raw-object ordinals and NN group IDs are never substituted for time.

The genuine recorded nine-word corpus has actual intervalsMs. The probe sums
those intervals, including the first, sets baseTag to the first absolute time,
and stores each remaining sample as its measured offset. This records one
continuous raw object per word; missing pen-up metadata is not invented. Its
explicit1000Hz format parameter does not infer a physical rate from irregular
events: the measured channel controls subsequent sample reads. Four older
synthetic M/MIR/I/II inputs use separately labelled prepared uniform controls.

C1C derives its signed32 period by float32 `1e6/rate`, float32 `+.5`, then
truncation. Positive finite rates whose derived period is not a positive
signed32 integer refuse. The admitted C58 literal `ms` conversion follows the
native seven-dimension ratio to targetE7060 microseconds and truncates to1000.
The general unit grammar is not ported. C49.V18 is base plus the measured
signed32 sample times1000, or the uniform period times unsigned point index.
C49.V50 returns base, rather than silently using the first channel value.
C4A.V50 resolves the original first point. C4B.V50 uses the actual float32
start fraction, double delta/product/add, `+.5` and signed64 truncation; its
negative-time asymmetry is intentional. A zero fraction returns the original
time without an extra rounding step.04D2C0 reads V50 once per view and updates
both temporal extrema from that start; it does not use view end times.

Before any claim, every original and fractional time is validated inside the
explicit `±((1<<52)-2)` domain, with checked integer arithmetic. Rates, channel
lengths/order, finite XY within±1e6, source/point budgets and cancellation are
checked before Document creation. Caller collections are read with bounded
indices; arbitrary enumerators are not trusted. Changes to copied channel or
source arrays cannot alter the ingress.

## Admission and ownership

`Prepare(actualParent, actualCroppedSource, ingress.TimingReceipt)` requires
ReferenceEquals on parent, raw Document and typed cropped owner. The parent
must have completed one actual WORD, one alternative and one line; unsupported
WORD10200000, CHAR10600000 and selectedC00 branches refuse. The chosen row must
have positive1..4-byte UTF8 and a valid actual signed marker. The real string
pool and source keys remain authoritative. CHAR view blocks are partitioned
by actual link occurrence, range index and raw-object occurrence. Equal source
nodes do not merge separate link inputs. WORD views retain original WORD
ranges, including exact range ordinals, rather than selected-key geometry.
The genuine II selected10000 case is retained.

All rows, UTF8, range vectors and point visits are preflighted: at most4096
CHARs,65536 records/views,1048576 point visits,4MiB keys/UTF8 and32MiB bounded
payload estimate. The payload estimate is not a claim about CLR allocator or
object-header memory. Raw ingestion is capped at256 sources and4096 points.
Guide fields come from the actual sourceLine56 record. Positive height,
bounded coordinates/angle and every timing conversion are checked before
owning the phase. No source-class coercion or legacy count-one owner exists.

A CWT keyed by the exact parent retains its geometry phase. Repeated Prepare
returns that same owner. Run is single-use, including entered failures.
Validation retains exact pending selection references/counter, CHAR links,
WORD48, flags, keys, chosen rows, strings and sourceLine56. Changed or disposed
upstream state refuses. Preclaim cancellation consumes nothing. After entry,
failures retain the written prefix and poison retry. Snapshot access validates
the upstream lifetime. Already returned snapshots contain independent scalar
and byte copies and do not masquerade as live native pointers.

The raw ingress has its own immutable lifetime: original sample reads may
remain usable after an inference scope is disposed. Cropped-view time reads
and geometry access validate that scope and fail after disposal. This is an
intentional distinction, not an alleged global invalidation of every byte.

## Native sequencing and numeric policy

Structure is published first: WORD intervals/selection, CHAR marker/UTF8 byte
positions and metadata, and memset-equivalent line48 numerical defaults.
Global minimum starts FLT_MAX; the native global maximum starts positive
FLT_MIN (`0x00800000`), not negative FLT_MAX. Unwritten values are nullable or
carry their actual native sentinel and an explicit written mask.

Rotation uses the actual guide angle.04AA60 rotates each original WORD point
and accumulates vertical moments in native order.04B4C0 writes line fields and
top/bottom boundaries.04D2C0 evaluates every actual CHAR source occurrence;
04CD20 uses weights1,.7,.3 according to its actual vertical distance. Native
unroll4 count, first-moment and second-moment expression orders are preserved.
C4B chunks20 preserve those groups. The standard-deviation floor is
double(heightFloat)*.05. Horizontal scratch64 written masks distinguish links,
time sentinels, complete weighted accumulators and completed mean/deviation.

04E480's ordinary chain retains its native trend, anchors and bridge writes.
Boundary order can differ from CHAR insertion order; CHAR80 copies remain in
insertion order.04C340/04C440 use the actual half-height10000 branch and margin
flags.04A680 handles epsilon equality, zero deviation, root admissibility and
fallback midpoint.04E0E0 computes ordinary width statistics and may OR02000000
into phase-owned CHAR metadata before later helpers run. Its test is strict
`width > 4*meanWidth`; unrelated metadata bits are preserved. The single actual
WORD0 excludes the native spacing-contributor branch, but its native fallback
spacing calculation still runs.04DA20 and04DD50 retain their real predicates;
reached nonordinary04C0C0 or differing-category C25 services explicitly refuse.

The native write targets shared source+18, not a private word inside each
CHAR occurrence. This phase therefore owns a metadata-cell map keyed by exact
payload reference with ReferenceEqualityComparer. Repeated occurrences retain
one cell; equal numeric node IDs/keys in another scope do not alias. Historical
InitialMetadata stays fixed, EffectiveMetadata reads the current shared cell,
and one native write produces one receipt for its triggering CHAR. Subsequent
04DA20/04DD50 reads see that write immediately through every exact-source alias.
Original scalar flags must agree across every preclaim reference to the cell.

The native malformed-chain fallback is not an invented portable repair.
04F2AA–04F33E detect multiple/missing heads/tails and clear local_1af8.
04FB43–04FC5B traverses Next, checks output membership, and falls back at04FC15:
line+28 is cleared, insertion indices are written into boundary+4, and the
last endpoint index is repeated. Scratch Previous/Next is left as mutated.
The NEW focused122 instruction pins cover both exact PE ranges; full04E480
and its direct dependency graph are also checked by the composed closure.
The normal all-ordinary fresh-scratch controls do not establish that malformed
branches arise naturally. No production repair of source owners is performed.

This portable implementation uses System.Math sin/cos/log/sqrt in the admitted
moderate finite domain. It preserves the surrounding native arithmetic order,
float32 casts and output bits tested here. It does not claim a bit-exact port
of the vendor CRT's broad argument reduction or exceptional/nonfinite paths.
Finite-output checks refuse values outside the numerical phase's bounds.

## Verification and limits

The NEW probe retains the exact old WORD/TEXT capture as a separate artifact.
Its numerical capture contains all13 cases/37 CHARs/54 views, all physical
parameters/sample times and actual point bits, plus independently labelled
prepared controls. The Python oracle reconstructs source views from original
coordinates/ranges and timing from recorded intervals. It checks every field,
float/double bit, source boundary, chain link, UTF8 byte index, mask, aggregate,
metadata mutation and native stage order. Expected transcription never enters
inference or the geometry reference.

Prepared checks include175 fractional-time cases,8 period cases,30 weighted
moment controls,144 independently replayed chain controls,324 Gaussian cases,
120 endpoint cases, twelve failure/cancellation prefixes and73 refusal guards.
Eighteen kernel-only horizontal controls cover positive threshold equality, strict
greater-than, zero eligible width, zero equality, reversed boundary ordering,
unrelated metadata-bit preservation and failure after a metadata write. They
also check shared-source alias propagation, preserved historical values, later
lazy-helper reads and a negative case: two genuine distinct payload references
with equal numeric node IDs and keys keep separate phase metadata cells. Those
controls have synthetic numeric points/flags and never create a genuine Ready
geometry owner. The actual37 chosen records have class7000==0 and no natural
04E0E0 write; prepared coverage must not be reported as natural corpus behavior.

Managed and NativeAOT captures must match byte for byte. Twelve old Managed/
NativeAOT probe captures are checked against their existing hashes, including
global glyph/CHAR/WORD/TEXT, EN119, Russian source720, document coverage,
selected sources, AVL indices and cropped originals. The NEW static auditor
composes original constructor/unit/timing, cropped-view and numerical PE proofs
with the independent arithmetic oracle. The SDK is read only as data.

## Required next join; not implemented here

Native2DB precedes selection. Current selected/index/cleanup/transform/counter
owners retain their established nongeometric parent snapshots and cannot be
reused as if they consumed this phase's geometry. The new WORD64 contains the
actual chosen source/index/selection, CHAR/line intervals and min/max V50;
CHAR80 contains source identity, marker, actual UTF8 byte end, WORD/line index,
phase-effective metadata, extrema, moments and min/max V50. Line48 contains
CHAR bounds, X extents, mean/deviation, baseline endpoints and direction.
Horizontal/vertical boundary vectors and phase mutations are additional output.

Current genuine post-TEXT coverage consumes source links, lines and ranges;
selected stages snapshot full CHAR AVL metadata and WORD selected-link order;
source indices retain actual selection/AVL/membership owners. They do not yet
snapshot or validate this full2DB payload. A future NEW geometry-aware factory
must retain this exact Ready phase, validate every source/WORD/line identity,
and carry its phase-effective CHAR metadata and ordered failures into genuine
selected/index/cleanup/transform/counter owners. Numeric lookalikes from an
old global scope do not confer authority. That join needs a separate reviewed
proposal; this batch changes none of those stages or the application endpoint.

## Live CI parent validation (2026-10-23)

The timed-geometry oracle previously required the historical parent capture
`1f2d2f6e2bb57e6d249fc11325a5e9736f358a5cd9dce1fe2664dd8ecdebea58`.
After subsequent recognition corrections, current parent alternatives and
compact-row counts changed. This reproduces on Windows as well as macOS;
replacing the fixed hash alone would hide the dependency.

Managed and Native CI now supply `--models` and `--child` together. The oracle
first runs the independent raw child and WORD/TEXT parent composition checks,
then verifies all timed geometry from original coordinates and measured timing.
Historical static callers without these options still require their original
exact capture. Existing Native/Managed byte parity remains required. No
recognition or geometry implementation changes are part of this adjustment.

The current 13-parent run passed 37 CHAR links, 63 selection commands and all
54 timed views. Controls reject a mismatched old child, changed parent flags,
changed geometry, missing paired arguments and an unpinned historical parent.
