# Initial number D0: additive budget/call-inventory revision, 2026-10-23

This STATIC supplement replaces only the budget/call plan in frozen126cf3.
That package remains immutable; its native/provenance conclusions are retained.
No runtime has been written or approved by this supplement. NEW5/OLD0 remains
the proposed runtime inventory. All arithmetic is checked int64 before payload
copy, model loading, context geometry, NN evaluation or source map mutation.

## Exact intended call path

One private AK snapshot copy and its SHA plus the existing Context.Prepare SHA
and GlyphGeometry.Prepare SHA give FOUR complete572763-byte resource visits:
one copy, three SHA. No existing bundle/full-global scorer is called, so no
other resource hashing/source duplication is silently borrowed. The AK-only
loader loads FOUR glyph networks once. Context.Prepare loads its own initial
5991-byte network once: FIVE loads, total376371 serialized bytes. Do not load
the initial network again in the private glyph model. Each Load copies its
complete envelope and validates matrix/row scales; reserve2*376371 separately.

Keep exactly THREE ordinary382839-byte prefix traversals: weight configuration
LoadV110Prefix, CacheProfile.ReadV110Node and ReadDisabledPowerScale (which calls
ReadV110Node). They skip network envelopes; this is conservatively charged
traversal length, NOT a claim of three matrix decodes or weight allocations.
AL5134, four calibration vectors2192, confidence centers/slopes6144 and LUT704
have separate fixed parse/copy/validation charges. No JSON model tables.

SourceRanges.Prepare takes one chronology copy, validates all prepared point
identities and copies original twelve-byte key records. Context.Prepare calls
CopyStrokes then clones every float-point array again: TWO complete P copies.
Geometry.Prepare does the same TWO complete P copies and takes a second
chronology copy. CopyChronology constructs counts, identity point map and the
remapped chronology's keys/index/map arrays; reserve these duplicate passes.
PreparedInk.Create is called ONCE after geometry, with its one32-byte-per-point
list/array construction and forward-delta pass. Hoist this private immutable
owner outside the group loop; Classify/NormalizeOrderedRanges retain it and
do not mutate it. Never call Geometry.NormalizeGroup here, as that would copy
the entire prepared source per group. Public inspections must deep-copy data
and never expose this private owner's recoverably mutable memories.

Existing PreparedInk.Create admits totalP<=4096, therefore revise the facade
admission to P0<=4096 and P<=4096 (formerly8192); P0 is original retained
source-key cardinality, read from the exact source used by geometry, not from
final smoothed coordinates. Existing raw ingress also caps total points4096.
S,D<=32, N,G<=65535,A<=262144,R<=256,T<=64,L<=255,C<=96,atom8 remain unchanged.
Nonempty selected groups/ranges and numeric guards remain mandatory. C is
counted over ALL descriptors: k=LastPhysical-FirstPhysical contributes0 for0,
1 for1, else3*k-3. No assumption that descriptor windows are disjoint.

## Lookup and selected-range inventory

Retain the already authenticated bootstrap graph; no full graph conversion,
rebuild or clone. Read N/G/A counts, hoist Groups once, read node0 once and scan
all R arcs once. Select actual span0 ordinary groups in native callback order,
with duplicate claims tested against at mostT retained IDs (charge R*T).
Copy at most8 ranges per selected group during preflight and calculate L and
all context-window point sums from SAME final point counts before payload work.
The at-most32 output counts copy is128 bytes within fixed metadata reserve4096.

Per selected group ReadFrame and GetContext each resolve actual descriptors;
ResolveGroupSource uses up to TWO Steps.Single scans, hence4D combined, reserved
as8D. CopyChronologicalRanges runs for frame input, boundary and primitive scope;
each copies/sorts at most8 ranges. Primitive metadata has at mosttwo chronology
lookups plus ResolveEndingMetadata. The typed source chain retains the original
raw owner or one owned physical-reorder view of it; its private forwarding is
not arbitrary caller-built recursion. Generic chronology order/point iteration
and selected raw-coordinate copies are charged under point and bounded-group
terms below. GetContext visits at mostL points and considers at mostC cached
prior groups (native line/physical selection), then28-channel weighting.
No SourceRanges.ResolveCandidate/ResolveForwardWithResiduals DFS is called.
No per-group N/G/A graph scan is planned: group indexed lookup is constant;
descriptor owner lookup is charged by D. Never substitute a new source range
query, independent graph or independently normalized snapshot to save work.

## Revised reservations

Let K=572763,E=376371,V=382839,H=5134,J=2192,Q=6144,U=704.
Fixed work F=4K+K+2E+3V+4H+4J+4Q+4U+65536. These whole-resource/model costs are
ADDITIVE and cannot be hidden inside quadratic feature slack.

Preflight/source work I=N+G+A+32(P0+P)+64S+64D+4DS+R(T+64)+4096.
Context work X=C*(5828+32768+256*min(P,4096)+16S+16D+2048).
Selected work Y=T*(368720+64L*L+64*4096^2+32768+8D+16C+64L+64S+4096).
W=F+I+X+Y. L uses255 for whole reservation, not an observed easier candidate.
5828/368720 are complete stored context/glyph dot terms.32768 covers the two
128-square context raster passes (per C), or fixed selected vector/cache work
(per T).256*P accounts for bounded segment raster painting and window point
visits.64L^2 is the bounded selected-feature pair allowance; an ADDITIONAL64*4096^2
per T covers DynamicAcousticWeights' independently resampled point domain,
not payment for resource hashes, parsing or unrelated traversal. Source copy
passes/chronology use32(P0+P); per-group sorting/normalization, temporary raw
points, context weights and fixed permission/cache work have explicit terms.
These are conservative protocol/work cardinality charges, NOT exact CPU
instructions, cryptographic compression rounds, elapsed-time or RSS bounds.

B0=K+2E+4H+65536.
B=B0+512(P0+P)+128D+256S+16(N+G+A)+1024C+327680T+4096.
Point allowance includes both source coordinate double copies, two chronology
copies/remapping, original key records and the single32-byte PreparedInk list
plus retained array. Context allowance reserves stored30-float/30-short/28-int
window data and selected28-float priors; group allowance reserves four vectors,
NN outputs/rows, normalized255-short geometry, selected raw points, cache and
owned result copies plus dynamic resampling. Dynamic resampling has an
independent4096-point cap: do NOT substitute L255 or configured SampleCount
as an output bound. Two copied point/owner buffers and five segment float
arrays reserve at most44*4096 logical bytes plus65536 other per-group data,
within327680. The additional64*4096^2 work term pays for both directional
exclusion scans and pair consideration. This conservative work cap may be
large; it is not a claim of typical observed work or acceptable worst-case UI
latency. A tighter cap would require a separately reviewed resampling proof. Retained original owners are not counted as copied bytes;
graph-domain charge is conservative metadata allowance, not full graph clone.
Managed object headers, dictionary capacity, allocator internals and native
unused bytes are explicitly outside this logical serialization reservation.

The companion auditor recomputes maxima and pins20 exact existing source files.
Source hashes are evidence of this call-plan review, not a runtime issuer.
Public default bounded factory/internal smaller test overload remain unchanged.
Tests must reject exact-minus-one reservations for this NEW formula, exercise
deterministic cancellations before/after each stage and prove no publication.
Prior frozen maxima are superseded, not reused as current positive admission.
No actual D0/query result, recognition quality or native-equivalence claim.

Recomputed simultaneous-admission maxima: B=32983513 logical bytes; W=69123317896 work-cardinality units. Fixed work is4887306; source/preflight700414; context104663424; selected69013066752. The large selected maximum explicitly includes the independent dynamic-resampling cap; no claim that every extremum is jointly realizable.

Canonical repository supplement: source digests normalize ONLY CRLF bytes to LF for cross-platform checkout consistency. The immutable7d635a package retains its original Windows byte pins. Formulas and source text are unchanged.

Canonical cleanup accounting supplement: immutable7d635a had W69122745133/F4314543. Current code explicitly adds one572763-byte finally cleanup clear, yielding W69123317896/F4887306, Bunchanged32983513. This is distinct from the original four model input visits (copy1/SHA3). No inherited feature slack pays for this clear. Original7d635a and earlier captures remain preserved.
