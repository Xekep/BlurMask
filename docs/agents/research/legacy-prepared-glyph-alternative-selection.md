# Prepared glyph alternative selection

`LegacyPreparedGlyphAlternativeSelection.Select` ports the complete bounded
**13A830 / actual274.V60** owner-selection kernel. It consumes an explicit
`LegacyNativeGlyphRecordNode`, optional incoming uint32 marker and an explicit
selection allocation image. It owns snapshots of all supplied40-byte source
rows and the final active allocation; source keys and raw score bits are not
rewritten. It does not execute the lazy217670 producer or claim the final
0213A0 wrapper has been joined.

The caller supplies an initial capacity and the bytes of its possible future
allocations in one bounded image. The current allocation begins at that
capacity. Each growth preserves existing bytes and exposes the next image
tail unchanged. This makes native capacity changes and opaque trailing bytes
reproducible without inventing allocator residue. A growth beyond the image
is an **explicitly injected allocation refusal**, not an assertion about
native memory exhaustion. No allocator callback, vendor code or native
pointer value is simulated.

## Native behavior

The native owner count is `((short)(packedCount << 2)) >> 4`. The loop clears
the output count but retains its initial capacity/storage. A zero or negative
signed count takes the empty successful path; the source allocation remains
an owned unchanged snapshot. Positive counts must fit the supplied source
allocation. Counts, alignment and resource budgets are checked before work.

Records deduplicate on exactly:

* descriptor04 low3 kind bits;
* complete ushort descriptor08;
* publication06 high byte.

Descriptor ID00, kind upper bits, publication low byte, contextual costs and
source payload do not form that identity. The result stores actual **uint32
record ordinals**, not Unicode scalars or serialized source-reference keys.

New identities insert before the first strictly lower float32 record0C
score. Equal scores retain prior order. Better duplicates replace their old
representative and may move toward the front; the total count is unchanged.
The native comparison instructions matter: insertion is `COMISS / JA`;
duplicate rejection is `COMISS / JBE`. Both reject an unordered NaN as a
new better score. The decompiler's apparent duplicate `else` must not be
implemented as replacement on unordered comparison. Positive/negative zero,
infinities, signaling/quiet NaN encodings and subnormals are not normalized
or written back into source records.

Capacity grows to `max(count + 1, trunc(double(capacity) * 1.5))`. A refused
growth happens before the insertion or capacity write. The returned result
retains the previous partial selection, its exact active capacity and every
unwritten byte. `Succeeded=false` returns the original optional marker
unchanged, matching native final-writeback gating. Successful selection maps
the original selected record through replacement/deduplication and then to
its output position; missing selection writes uint32FFFFFFFF. A null marker
remains null. Input arrays are never mutated, including on failure or
cancellation.

Portable limits are65536 source rows and65536 selection slots; these are
explicit ownership/work budgets. They are not learned thresholds. Native
packed count still determines how many source records are actually visited.

## Ownership and integration boundary

The kernel copies the canonical owner key, packed count and **complete**
source allocation supplied by the caller. It neither changes the key nor
merges records from different owners. Its result can feed the already
implemented prepared0213A0 caller once the actual wrapper owner/marker is
available. `273.V40=13C6E0` additionally refreshes the owner through217670
before this kernel when packed-count bit4000 is clear. This new API does not
set that bit, assert refresh succeeded, construct a TOP owner, choose an
alphabet or create document input IDs. See
[the wrapper roadmap](legacy-main-string-wrapper-roadmap.md).

## Verification

The independent original Python model uses symbolic identity/representative
lists, then commits their ordered indices into the allocation image. It does
not copy the runtime's byte-shift implementation.2105 controls cover every
pair of13 raw score classes for both duplicate and distinct identities,
optional/invalid markers, random raw40-byte records, irrelevant flag bits,
count/balance/refresh/dedup packed bits, signed negative counts, empty output,
allocator growth/refusal boundaries and an owner with2040 records. The
Managed and Windows NativeAOT probes each compare14974 emitted rows,
663 injected allocation refusals and7 guard cases. Full source snapshots,
partial output and unused capacity bytes match. Defensive-copy checks mutate
the caller arrays and returned copies after selection.

```
python tools/LegacyGlyphAlternativeSelection.Probe/reference_controls.py CONTROLS.json
dotnet run --project tools/LegacyGlyphAlternativeSelection.Probe -- CONTROLS.json OUTPUT.json
python tools/LegacyGlyphAlternativeSelection.Probe/reference_controls.py CONTROLS.json --audit OUTPUT.json
python tools/handwriting/audit_glyph_alternative_selection.py STATIC.json
```

The read-only static audit checks36 exact instructions, the growth multiplier
and274 table2DC680+60 binding. Text PE SHA256 is
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Native byte interval `[18013A830,18013ABA0)` SHA256 is
`9381032ff21ab1220e8bb50266882281a4f17528c00ba036a572e1c13bc940eb`.
These controls are explicit prepared owner data, not fabricated handwriting
winners or recognition-accuracy claims. No vendor binary is executed.
