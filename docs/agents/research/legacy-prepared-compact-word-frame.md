# Ordinary prepared2075E0 frame producer

`LegacyPreparedCompactWordFrameProducer.CreateOrdinary` composes native207420
context surgery, literal zeroing of the entire944-byte2075E0 frame,2067C0
context difference, ordinary21B coordinate production, callback-present
coordinate counters and206960 normalization. It returns the rewritten context
identity and a private owned frame; `CopyFrame` always copies its storage.
The input frame is not allocator residue: native2075E0 explicitly clears3B0
bytes before writing these fields.

This endpoint accepts a shared context factory, current/previous IDs, source
row88, uint node, glyph-difference byte, prepared runtime policy and the exact
source category callback. It snapshots the source row before any external
callback. The graph arc comes from that row's ushort1C. Packed coordinates are
node/arc references, not inferred XY or glyph boxes.

## Exact context surgery

`RewriteCurrentContext` restores207420. It eagerly reads the current context's
parent when the current ID is nonzero, before testing source metadata. On the
same-node branch, source2300 and flag2308 must both enable associated quotes.
Matching apostrophe24BA or quote24BC then compares row1A&3F with source category
for251C or251E. A match replaces the current leaf under its actual parent.
If both special input symbols coincide, the native first/second category call
order is retained. With no match, the original identity is unchanged.

Different ushort row18 versus the full uint node, combined with negative signed
row5, replaces the entire context with a root knowledge0E symbol. Positive
signed row5 leaves the context unchanged. New child nodes receive their parent
link; reused children are not relinked. Allocation failure propagates ID0,
without substituting the previous context. Subsequent2067C0 retains its own
metadata branch, so it can still emit knowledge0E even when that context
allocation failed. Category-callback cancellation is checked before insertion;
a completed insertion retains its parent link before post-insertion cancellation.

## Alphabet-derived symbols and explicit policy

`LegacyPreparedCompactWordContextPolicy.FromAlphabet` restores20C1C0's symbol
values from an actual supplied acoustic alphabet. Engine053F20 reads serialized
header0..6 into runtime8/A/C/E/10/12/14, then reads count7 at1A, skipping16/18.
Thus runtime knowledge0E is serialized header index3, not the count or a Unicode
fallback. The count virtual method052790 reads1A. Text20C1C0 binds descriptor60
as controller28, resolves24BA=`'`,24BC=`"`,24DE=U+200C, and sets251C/251E to
count+1/count+2 with native ushort wrap.

Associated-source presence2300, associated quote flag2308 and sentinel flag24B8
remain explicit prepared policy. This helper does not infer them from a
resource's stored case triplets, select an SDK driver or reconstruct mutable
property state. The source category callback is called only on its exact quote
branches. The ordinary coordinate callback is present and returns one item;
its prefix counters are0/glyphDifference/0. Null-callback fallback and custom
coordinate implementations have different contracts and are outside this route.

## Validation

768 independent complete-frame cases check724,992 bytes, rewritten context
identities, category callback order, same-node/uint-mismatch and signed flags,
shared/different prefixes, both quote branches, coincident input quote symbols,
and associated/sentinel policy combinations. The generator reuses only the
separately verified independent Python list normalization oracle, without
executing its CLI or importing production C# code.

Focused checks cover new versus reused child linkage, idempotent identity,
allocation failure0, five invalid/cancelled input guards, callback cancellation
and callback mutation of caller source-row storage. Actual RU neural networks
and the owning four-node DIRECT pass supply214 nonempty ordinary row contexts;
all complete normalized frames match independently assembled symbols and packed
node/arc data. Runtime association/sentinel flags in that prepared fixture are
explicit. These checks do not assert a final expected recognized string.

The read-only static audit pins complete207420/2075E0 instruction regions,
Engine reader/getter offsets and count vtable, controller AL ownership and the
three text literals, without loading vendor binaries.

This completes the prepared ordinary2075E0 producer/consumer chain. Actual
207CB0 predecessor marking, primary alternatives, local candidate ordering,
node offsets2478 and compact-index writes22 remain separate.21FFD0 needs those
published compact records, including field1E's alternative override. Neither
text output nor acceptance confidence is claimed by this frame API.

Generate controls with `tools/LegacyCompactWordFrame.Probe/reference_controls.py OUTPUT`.
Run the probe with `controls [AK LK engine separation]`. The static audit accepts
`OUTPUT [--text PE --engine PE]` and reads the two pinned binaries only.
