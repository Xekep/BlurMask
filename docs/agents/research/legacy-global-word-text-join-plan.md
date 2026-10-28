# Global CHAR to WORD/TEXT: static closure and next owner proposal

This is a static proposal on `1587914`, not a runtime implementation or an
approval to change existing owners. Global CHAR publication is now real and
independently verified; WORD/TEXT aggregation and adoption by the old document
route are still missing. No recognition-quality improvement is claimed.

## Actual parent and child order

The native outer owner is0283B0. It interns the WORD source at028E25 before
calling024F30. A fresh source receives payload defaults and a zeroed48-byte
choice array, with the count incremented before processing its CHARs. The
WORD payload initially clears bit800000 and sets200000. The child024F30 clears
the latter flag on an eligible non-skipped positive source, refreshes every
eligible reference before selection, then processes each actual CHAR and
appends its24-byte typed link. The WORD48 flags accumulate the chosen CHAR
text-record flags. They are not the CHAR metadata flags: prior bit1000000 and
the CHAR degenerate bit must not be copied from payload18 into WORD48.

The already implemented standalone global CHAR wrapper returns these values
after successful processing, but it does not currently expose a native parent
WORD prefix. Therefore a publisher that first calls its public Publish and
only afterwards creates the WORD source would have the wrong native ordering,
especially on failure. A correct next owner must reserve the child, intern and
initialize the WORD parent first, then drive the same CHAR implementation with
an internal **typed parent receipt**, not arbitrary callbacks or supplied flags.

Selected CHAR text uses the actual marker when its payload40 override is−1.
The fresh global payload already proves that default and a zero prior-vector
count. Empty traversal's prior byte1 is not equivalent to a prior-vector owner:
coverage and ordinary-union membership still choose the actual interned CHAR
key when count38 is zero. No zero/old count1 input owner may replace it.

## Three different source memberships

The WORD interner key is the actual completed WORD source collection, projected
from its source26 allocation into the original input IDs. It is separate from
the complete original root and from the union of processed ordinary CHAR keys.
Native registration at0290DA passes payload20/count28, the WORD's original
interner key. It does not pass the accumulated ordinary vector at payload30.

The fresh WORD metadata, relative to node+18, initializes00=0,08=−1,
selection10=`long.MaxValue`, flags18=0, ordinary vector30 empty,
override40=−1,48=−1, and choice vector50/count58 empty before allocation.
The exact original-key storage at20/28 belongs to the interner, not a copied
bounding rectangle. After CHAR processing, flags18 ORs every WORD48.flags.
If200000 stays set, selection10 becomes−1 and no WORD command is appended.
Otherwise native0210C0 kind1 registers that original key, and bit800000 is
recomputed from its single-range same-coordinate/fraction-FF predicate. On
registration failure the metadata prefix is retained; no invented selection ID
or rollback is allowed. A fresh sole choice keeps its native48-byte zero score
fields; scores from WORD120 are not transplanted into it.

The document alternative then appends a24-byte WORD link with a typed source
sidecar at08, actual selected WORD-choice ordinal at10 and byte14=0. Its32-byte
choice flags preserve existing upper bits and OR only the selected WORD48's
low3FF bits. In the bounded fresh case the output scalar row starts zero. The
global first-alternative snapshot proves one actual alternative; the existing
configured route proves two. Neither count may be fabricated to satisfy the
other route. Require the actual WORD.WordIndex to resolve the sole real choice;
refuse an out-of-range index instead of substituting zero.

TEXT is another key. Native028771 supplies a NULL reference list to05AFA0;
this is different from a nonnull list with count0. It visits each actual retained
selected40 INPUT row, invokes2C1.V18/053F90 with no overrides, skips mode4 and
normalizes/unions through C21.VA8.0210C0 kind2 registers that resulting full
selected-input key after all alternatives and source-line publication. It uses
the same Seed, INPUT binding and pending C4D as every CHAR and WORD command.
It must not use the WORD clip merely to obtain matching source coverage.

The already recorded real global13 examples demonstrate this boundary:
eleven WORD keys equal their original root, while procedural MIR andII do not.
The current document0248D0 supported branch requires exact range-array equality,
not containment. Those two inputs must explicitly refuse at that later boundary
until238B0 uncovered-source processing is independently ported. Their correct
WORD/TEXT registrations may exist as retained prefixes before that refusal.

## Source lines and whole-document ordering

Native021AB0 obtains frontendV250's actual source218, scans its descriptors for
the largest nonnegative assigned line ID+1 excluding kind3, appends that many
56-byte slots, then writes through sourceV40/V100 guides at wrapper68+i.
Allocation count and wrapper write offset are different.028902 resets wrapper68
for each alternative; the global first alternative therefore performs one real
append. It does not get a duplicate zero tail just because the configured2 path
has one. CHAR SourceLineId already comes from the actual global group through
its descriptor ordinal and descriptor+10, not an assumed line0.

The global cut source retains both actual descriptor88 and guide40 copies from
its exact `Preparation.Input.Input.Input` line owner. Use that lineage and the
already proved fresh main0/no-property rotation gate; do not borrow a sampled
old source or compute a guide from raw glyph bounding boxes. Guide geometry
uses8/C/10 or corrected14/18, including the unadjusted baseline atC. After TEXT,
025E40 may use the equal-actual-line no-split branch only when every relevant
CHAR link actually proves the same assigned line. Truncation derives the last
referenced line+1. Split/insertion branches025940/024A90 remain explicit refusals.

## Proposed first runtime batch: two existing seams

This inventory is a request for later review, not authorization to implement it.

1. `LegacyFreshMainGlobalTextSourceWrapper.cs`: reserve the unused wrapper for
   one exact new WORD parent and add an internal publication path accepting
   only its unforgeable typed receipt. Update the parent ordinary flag at the
   native first-pass point and append its actual ordinary union/CHAR24 link and
   selected flags at each successful native second-pass point. Keep the current
   standalone public Publish output and tests byte-identical. Reserving before
   WORD interning also blocks a standalone retry after a failed parent prefix;
   the child cannot be silently reused as a fresh operation.
2. `LegacyOwnedMainTextSourceInterner.cs`: add a separate typed global WORD
   source sidecar and internal InternGlobalWord receipt entrypoint. Preserve the
   exact unsigned comparator, AVL/insertion order, existing CHAR/old getters and
   public prepared behavior. Require the actual scope.SharedState.Words owner,
   initially empty, and the same original Document/Seed. Existing prepared/old
   WORD nodes cannot be retrofitted with global provenance. The CHAR tree and
   WORD tree stay distinct even when their range arrays happen to be equal.

New files can provide the global parent/WORD payload, native24/48/32 typed rows,
source lines, TEXT receipt and a NEW selection partial
`LegacyOwnedFreshTextSourceContext.GlobalWordText.cs`. The partial validates the
actual global owner/Seed/INPUT/current prefix and calls existing private
CreateCore kinds1 and2; it does not widen old source-selection entrypoints.
No edits to old configured WordSourceChoices, old candidate owners, category
readers, old source classes or old scope constructors are needed for this first
batch. The one alternative/one WORD restriction stays explicit; different-span
WORD remapping, cached WORD sources and multiple fresh global alternatives
remain outside it. Natural global duplicate CHAR-key coverage is still0.

Native-order tests must capture the WORD node/default/zero-choice prefix before
the first CHAR refresh, intermediate flag/link counts, failed CHAR registration,
failed WORD registration, failed line/TEXT publication, consumed counters and
poisoned retries. Successful tests independently recompute full unsigned keys,
all24/48/32 scalar bytes and typed identities, the actual WORD index, selected
flags versus CHAR metadata flags, guide56 bytes, CHAR/WORD/TEXT memberships and
native ordering. Use the existing13-input set, including nine genuine recorded
tracks, without expected labels; keep prepared
failure/negative/special cases explicitly separate. Do not claim natural reuse
or source-coverage success for MIR/II. Managed/NativeAOT and previous public13,
EN132/old EN124/RU source720 captures must remain exact.

## Existing document route is a further typed boundary

There is no honest current call from global1 into
`LegacyLibraryTextPublicationDocument.PrepareOriginalSelected`: it accepts the
old scope and `LegacyOwnedMainWordSourceChoicesResult`. The following twelve
files expose concrete old owner/configured2 assumptions, checked by the audit:

| Existing file | Boundary that must be handled explicitly |
| --- | --- |
| LegacyFreshMainWordSourceChoices.cs | two actual configured candidates and old source views |
| LegacyFreshMainSourceLinePublication.cs | old sampled guide owner, exactly two alternatives |
| LegacyFreshMainDocumentTextSelection.cs | old source-line result, two allocations/alternatives |
| LegacyFreshMainDocumentLineConsistency.cs | old sampled lineage and prior two-line allocation |
| LegacyFreshMainDocumentSourceCoverage.cs | configured2 owner and exact WORD/TEXT key equality |
| LegacyFreshMainDocumentCharacterCoverage.cs | old CHAR payload and FirstProcessedSource key sidecar |
| LegacyFreshMainDocumentSelectedSources.cs | old WORD/CHAR typed overlays and actual chosen rows |
| LegacyFreshMainDocumentSourceIndices.cs | claim keyed by old WORD owner and old CHAR key headers |
| LegacyFreshMainDocumentInputTransforms.cs | concrete old chain and D8==2 |
| LegacyFreshMainSourceRangeUtf8.cs | old typed CHAR/WORD/TEXT sidecars |
| LegacyFreshMainDetachedTextDom.cs | old payload dictionaries and alternative count2 |
| LegacyLibraryTextPublicationDocument.cs | old publication scope/result claims and orchestration |

Seed.Validate additionally accepts the old WORD owner; control cleanup and
counter handoff retain the concrete old state chain. These are not automatically
approved seams. A later source-neutral internal contract must retain a genuine
old-or-global typed owner, source tree, selection transaction, all alternatives,
actual line owner and Seed. Each document algorithm needs a reviewed internal
consumer of that contract or a separate genuine global overload; public old
getters must keep refusing foreign/global owners. Reconstructing an old
configured candidate, source collection, count1 sampler or a fake second
alternative is prohibited.

The numerical kernels for indexing, formatting, DOM arena/edit transport and
original registry/DOM apply can potentially be reused, but this static package
does not prove a complete typed consumer refactor. In particular it does not
authorize bypassing coverage, line splitting, control expansion, prior source
lookup, transforms or original history/DOM ordering. The first batch should stop
at the new correct WORD/TEXT receipt until that bridge has its own proof and
seam inventory. This is a known remaining integration task, not completion of
the requested whole engine.

## Reproducible static evidence

`audit_global_word_text_join_plan.py` pins six complete regions,54 exact
instructions and composes five earlier native proofs: WORD/CHAR shapes and
registrations, source-line allocation/guide ownership, NULL TEXT projection,
line consistency and current global CHAR ownership. It independently verifies
the managed global producer contracts and preserves the actual old document
blockers. Optional `--capture` reports actual WORD/root equality as evidence;
it does not feed labels into recognition or stand in for the numerical oracle.
Vendor libraries are read as bytes only, never loaded or executed.

```text
python tools/handwriting/audit_global_word_text_join_plan.py REPORT.json --capture GLOBAL13.json
```
