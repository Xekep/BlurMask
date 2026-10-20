# Numerical 2DB dependency closure: static result and proposed boundary

This checkpoint adds no runtime. Forty-three Ghidra functions were exported
headlessly from read-only Text/Ink projects. The companion data records all73
actual body ranges, linear PE bytes, hashes,1594 call/branch/flag/compare pins,
27 descriptor/table bindings and the call graph. CRT functions have disjoint
Ghidra ranges; they are deliberately not described as contiguous functions.
Failed exploratory exports are outside this proof's selected successful inputs.

## Source operands and ordinary cropped views

238B0 sets R8B=1 and stack geometry byte=1 before actual2DB.V18. R9 retains
result2DA; stack argument5 retains Ink from the factory's+10 field. Consequently
ordinary04B4C0/04E480 use original source ranges. Mode0's Ink.V128 path is distinct:
C20.V128=06C880 uses its own C51 registry,09EDE0 compares complete64-bit IDs, and
a missing selection returns null. It cannot use a source ordinal as an ID.

The original ranges still resolve objects through Ink.C20.V90=06BCB0→06B7F0.
Full raw root curves are concrete C49, but cropped requests can yield C4A or
C4B through C1E.V8=0914E0. Assuming every resulting object is C49 is wrong.
For integer endpoints it creates C4A and08E750 retains the original curve,
start/count and open/closed endpoint bytes. V78=08E950 reads its actual count;
V88=08E9A0 points to original floatXY+start*8; V90 delegates offset chunk reads.

Fractional endpoints create C4B through08EB50, which allocates one/two endpoint
rows, computes each channel as float `(next-current)*fraction+current`, and
retains the exact resulting count. Its V80=025C90 returns0 and V88=0222C0
returns null. V90=08F400 supplies synthetic endpoints plus original interior
points, honoring chunk offsets.04AA60 uses20-point chunk requests for this
noncontiguous path, and the pointer path only when actualV80 allows it. It
rotates each point in double, rounds XY to float, updates ordered extrema, and
accumulates double N/sumY/sumY². No whole-stroke bbox replaces cropped geometry.

The raw-root C49 constructor/channel lineage is separately proven by the
existing raw-current-view audit. C4C affine objects and C25 are separate routes;
their constructors are exported for branch identification, not declared
implemented or admitted by this bounded ordinary proposal.

## Numerical helper map

-04A680 solves the equality of two Gaussian distributions. It checks absolute
 standard deviations and the quadratic coefficient against2^-23, uses log of
 their ratio and the discriminant, accepts roots strictly between the means,
 and otherwise returns their midpoint. Root ordering and NaN comparisons matter.
-04C340/04C440 compute left/right endpoints using04BF30's actual per-line height
 (or special transformed geometry),0.1/0.25 margins,2.8σ bounds and selected
 flag10000's half-height rule. These flags are actual selected CHAR40 fields.
-04D2C0 obtains actual source objects through source-range enumeration. Ordinary
 point geometry passes through04CD20, whose count is an integer stack argument
 despite Ghidra's inferred float signature.04CD20 clips Y-distance against h/2h,
 weights observations1 or0.7, and updates X moments/extrema in native order.
-04DA20 groups runs without ordinary source geometry and adjusts ordered
 boundaries using04C0C0's spacing.04DD50 handles gaps between differing source
 categories and may consult C25 properties; these are not unconditional no-ops.
-04E0E0 first derives mean width from boundary differences whose selected row
 has `(flags&7000)==0`. It then processes class1000 widths against4*mean,
 computes a separate spacing mean with neighbor/WORD gates, and applies the
 native1.2*mean fallback when that spacing is below0.1*mean.
-04A860 is DOM/source selection remapping, not a mathematical helper. It reads
 content/activeArea, locates input/tag and resolves the same Ink.V128 selection.
 An ordinary constructor does not manufacture that DOM or a successful lookup.

04E480's complete body orchestrates these operations, scratch64 rows, grouping,
boundary writes and failure paths. The fully exported03 helpers04BF30,04CD20,
04C0C0 close the newly found direct geometric edges. Remaining direct calls are
classified explicitly: memory/array allocation, stack/security machinery,
CRT log/sin/cos/sqrt and their exceptional/range-reduction helpers, provenance
clone branches and transformed-source support. They are not hidden geometric
black boxes. Finite-domain BCL numerical replacements would still require an
independent oracle and measured error bounds before runtime acceptance.

## These geometry producers can mutate their owners

04B4C0's successful special-selection branch can alter the referenced source
guide baseline.04E0E0 ORs02000000 into an actual CHAR source metadata flag when
class1000's width is greater than4*mean. The latter happens before04DA20 and
04DD50 can fail, so later failure does not erase that assigned prefix. Both
writes require typed, ordered ownership; a pure immutable bbox return would
silently discard native behavior.

The pinned13-parent capture contains37 actual chosen CHAR rows. None has
class1000 or special400; one (II) has10000. All actual metadata is ordinary and
unmasked by10600000. This establishes their input branch gates, not execution
of native geometry or OCR improvement. In a new ordinary implementation,
admission must prove those gates remain absent through the whole producer;
prepared controls must separately exercise the mutation branches. Broad
admission cannot reuse this corpus's zero counts as a blanket proof.

## Proposed NEW typed API, still requiring runtime review

`GlobalGeometricSourceSnapshot.Prepare(completedGlobalWordText)` should retain
the exact same scope, original raw document, chosen WORD/CHAR links and markers,
source-range identities and source lines. It must validate selected row scalars,
pending transaction IDs, guide bytes and all source metadata again before claim.
No old configured2 owner or interval AVL is coerced into this identity.

`GlobalGeometricIndex.Build(snapshot, cancellation)` should produce typed
WORD64/CHAR80/line48 and native boundary/scalar snapshots, together with ordered
guide/CHAR mutation receipts when admitted. A bounded ordinary entry may prove
mutation gates absent; it must explicitly refuse C25/DOM, missing IDs, transformed
objects or nonfinite numerical domains. C49/C4A/C4B are derived from actual
range resolution and carry their real point/chunk receipts. Runtime follows
whole-operation preflight budgets for cropped point visits, repeated helper
passes, scratch, UTF8, indices and boundary allocations, with native partial
prefixes and poisoned retries if mutation begins.

The index exposes separate typed line/CHAR/WORD results forV60/V68/V70. Tests
must include strict boundary equality, half-open X intervals, negative zero,
integer/fractional endpoints across20-point chunks, empty lines, neighboring
distributions, nonzero rotation, flag10000, cancellation and mutation failures.
Full arrays and scalar bytes need an independent raw-coordinate oracle and
Managed/NativeAOT parity.43 exported bodies are not43 implemented algorithms.

238B0/234D0/339E0/36630 source-membership mutations, the parent's independent
global document bridge and original acceptance remain outside this batch.
