# Math2 formatted units to source-bound symbols

This static checkpoint connects the complete EE4 output to the source-position
owner used before the first-result copy. It adds no runtime source projection,
DOM publisher or accepted formula. It corrects the possible inference that a
raw-only input necessarily leaves each output symbol's source collection empty.

## The native range guard is enabled

`047C50` starts its C79 local at zero (`047D77`). It invokes the actual C6D V10
with the address of that local as argument seven (`047F24/047F2E/047F3D`). The
registered C6D primary table `0DECE8` binds V10 to Ink `014BE0`.

When that argument is nonnull and points to zero, `014C4C..014C79` creates C79
and writes the new object through the supplied pointer. It then calls the
registered C79 V8 (`014CA0/014CA4`), bound to `00F130`, on the actual selected
input. Allocation or initialization failure cannot produce successful C6D
processing. The same C79 local is loaded into R9 at `048437`, immediately before
`046AE0` at `04844C`. This is the actual same-owner chain, not a shape match.

After `045C60` has initialized the 40-byte symbol rows, `046DB1/046DB4` tests the
C79 argument and `046DC1/046DC5` tests EEE byte51. The independently pinned cold
EEB request initializes byte4 to one and forwards it to EEE51. Therefore the
ordinary successful fresh native host call supplies C79 **and** enables this
branch. No-existing-C25 and no-auxiliary policy do not make this guard false.

Consequently the current library output's opaque row00..0F cannot be described
as a proved empty native source collection. A literal first-copy owner may
preserve an explicitly absent portable range projection, but that is a bounded
representation gap, not proof that native raw rows contain no source ranges.

## Position identity survives feature preparation

C79 initially owns original input source collections, packed point positions,
per-input separators and source-kind flags. Packed positions carry the original
source object index in the high DWORD and a point index/fraction/mode in the low
DWORD. A processed frame index, sorted stroke ordinal or segment number cannot
replace that original source identity.

The actual pipeline carries C79 alongside the transformed ink:

- Resampling `017060` creates packed position coordinates while producing the
  sampled points, then invokes C79 V18 at `0184EA/0184EE`. V18 is `00FB30`:
  it composes point/fraction positions through the prior position vector and
  updates the separators. Fractional interpolation is a literal float32 chain.
- Stable mean-X ordering `01EEF0` invokes C79 V20 at `01F3BE`; V20 is `010240`.
  It gathers entire position segments and source-kind lanes by the actual
  permutation and rebuilds separators. It does not replace original source IDs
  with the new ordinal.
- `013F20` writes `strk_ends` from that same C79 separator vector. Its raw-source
  branch accumulates the differences of adjacent separators. Thus the NN's
  segment endpoints and the source-position domain have an explicit common
  producer; equal tensor dimensions alone would not establish this relation.

`00F7E0` is the InkMapping **validator**, including separator, source-index and
position-order consistency. It is not the range-cache writer. Existing docs
that broadly call it postmerge cache production must not be used as evidence
for constructing a new cache. The existing public fresh C79 snapshot creates
integer-point positions; it cannot by itself represent the processed fractional
and reordered vector. Its `010850` consumer is reusable only when supplied the
actual processed fields and when the supported empty-cache lifecycle is proved.
General known-text/C25 cache entries remain a different branch.

## Exact attachment and later aggregation

For each run, `046AE0` reads its position endpoint, calls C79 V30=`010850` for
`[previousEnd,currentEnd)`, and then reads its signed mapped unit index.
A nonnegative index chooses `record78 + index*40`, where global C21 VB0 appends
and merges that range collection. A negative index skips attachment but still
advances the previous endpoint (`046E79`). A single unit can therefore receive
several intervals; the correct operation is the native C21 union, not a stroke
slice, bounding box or concatenated list chosen from a label.

First-result `043AC0/043740` copies those source collections and re-interns text
alternatives in a new Library. Next, `028A80` reads actual F0 rows with stride240,
selects their symbol vector at98/countA0, and calls `026780` to copy the 40-byte
symbols into a separate outer vector/library. For multiple F0 sources it adds
actual `<table>`, `<\>` and `</table>` symbol intervals and can remove duplicate
outer table delimiters. These are source-group aggregation operations, not a
policy to join every recognized alternative into a formula string.

Only that resulting vector is passed to `028070/027E20`, whose range serialization,
negative CB8 tag allocation, freeze matching, DOM/Ink edits and original target
owners remain distinct. The existing symbol-DOM audit pins that later boundary.

## Smallest coherent owning runtime proposal

No runtime changes are made here. The next bounded entry should be source-bound
library raw ink, with the following explicit ownership and seams:

1. A named raw-source factory makes one immutable original document before any
   mean-X reorder. It retains original input IDs and point counts. It rejects
   C25, external source selections and unsupported budgets; it does not claim
   that library IDs are an SDK page's IDs.
2. A shared `MathPreprocessor` core produces the **same** features and NN tensor
   tuple while retaining the packed positions created at its actual resampling
   and sorting writes. It must not reconstruct source positions afterward from
   XY distances, labels, `strk_ends` or a segment ordinal. Old feature APIs and
   captures stay unchanged.
3. A private immutable processed-position owner binds those vectors to that
   exact raw source and feature result. A narrow internal processed-snapshot
   constructor can reuse the existing `010850` range consumer. Its empty-cache
   branch needs an explicit no-C25/cache-write audit; no caller-supplied snapshot
   or success flag becomes an owning token.
4. A new source-bound output wrapper uses a locked metadata seam on the actual
   complete output before payload copies, checks total retained payload, then
   consumes the exact mapped runs. It resolves each previous/current interval
   and uses the existing original-document C21 VB0 union. It retains the same
   Record/Source/Library/unit-row identities. Old output rows and old first-copy
   APIs retain their strict opaque-range scope.
5. A separate named first-copy entry copies these actual owned collections,
   not empty placeholders, and re-interns intervals into its own new pool.
   `028A80` aggregation and DOM publication remain subsequent typed owners.

Expected edited existing files are only the shared feature core, raw-only source
factory and the narrow internal processed-snapshot seam, after coordination.
New math source-position/output wrapper files should hold the private provenance;
no parent first-copy file, completed-record owner, search loop, formatter or app
path needs mutation for the source producer itself.

The first real test should use the existing actual width1/3/16 runs, retain
identical NN/record/EE4 captures, and independently derive range bytes from the
captured raw point-position vectors and real mapped runs. Required discriminators
are reordered input IDs, fractional endpoints, repeated ranges, crossing a source
separator, negative-index endpoint advancement and several runs mapping to one
formatted unit. Same-source/record/model mismatch, stale/disposed owners,
cancellation, preallocation budget failure and postclaim prefix poisoning must
be tested. Prepared coordinate cases must remain distinct from actual NN events.

## Reproduction

`audit_math2_source_position_transport.py report.json` verifies both complete DLL
hashes,14 full function regions,60 instructions and11 concrete C79/C6D/C21 bindings, plus the existing cold
request producer audit.
It uses only PE bytes. Read-only exports are the existing
`Math2FeaturesStage4`, `math2-result-transport-20261018` and the preserved full
Document export. No SDK DLL, GUI or inference is executed by this checkpoint.
