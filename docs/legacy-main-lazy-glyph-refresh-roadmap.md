# Ordinary main lazy glyph refresh

This read-only investigation follows **217670**, the actual ordinary240.V90
callback used by273.V40=13C6E0 before alternative selection. It identifies
an owning next-stage contract; it does not execute a vendor binary, change
the runtime or declare the final string wrapper complete.

## Owners at entry

Native parameters are engine, main240 controller, TOP273 result, and the
actual **64-byte274 AVL owner node**. The last parameter is not a40-byte
glyph record or a120-byte WORD. Its canonical source key is at10, source
scalar1C, glyph count/capacity18/1A and glyph array20. Its additional fields
28/30/38 own source24-byte references, their count and the two residual bytes
per reference. Those fields are not represented by
`LegacyNativeGlyphRecordNode`'s lookup snapshot alone.

Frontend222.V248=0CA630 returns the same graph21B at frontend+70. The graph
has21B.V140=1AA470, V170=1ACF30 (category metadata188), and V1C8=05A400(false).
Consequently frontend280=155710 returns false for this actual ordinary graph.
The native publication06 additions guarded by frontend280 are unreachable in
that scoped branch; no caller boolean is required. Capture2368 must remain
the existing owned fresh false state. The trace/capture and other graph kinds
are not established by this mapping.

The graph/acoustic preparation callback frontend228=152A70 still occurs before
source work: it calls graph.V138=1AC0E0, forwarding sampler.V100, and acoustic
V158. Its result byte is reset again before each scalar symbol resolution,
the exact callbacks are now pinned by
`tools/handwriting/audit_main_lazy_preparation.py`. For this actual fresh
version106 sampler, table2E1490+100 is null, so the graph forwarding call
returns without work. Ordinary acoustic table2E4180+158 is1C2530: its entire
body writes word3C2=FFFF and returns. This invalidates the last-group cache
selector; it does not recompute scores or mutate source geometry. The portable
owning scorer must retain its immutable source/model identity and account for
that cache lifecycle. This proof does not apply to other sampler tables or
justify replacing an unknown callback with an empty function.

## Nonnegative owner arc: complete native order

1. Read signed arc16 from owner.key10 bits24..39. For a nonnegative arc,
   call graph.V140 with the key, an output base-source key, and the owner
   source-vector28/count30 in count-only mode. Failure returns false.
2. Allocate **count *26 bytes**:24-byte references followed by2 bytes per
   reference. Store the allocation at28 and residual-tail address at38.
   Call V140 again to fill both regions and return the actual base-source key.
   Allocation/fill failure leaves the preceding native writes; there is no
   rollback or invented empty-source success.
3. Decode signed node24 and arc16 from that returned base key. Store
   frontend158's float at owner1C. Frontend158=1529C0 forwards acoustic.V90,
   ordinary1C1520: **first candidate vector10[0]**. It is neither normalized
   candidate24.field0 nor TOP alternative mass.
4. Read the base node's **real2478 start/end** and scan its complete compact
   forty-byte records at2460 in stored order. A row participates only when
   compact byte20 has bit20, full compact08 equals the returned base key,
   and byte20 low3 subtype is zero. A matching node alone, candidate group ID
   or same normalized geometry is insufficient.
5. Append/find an actual descriptor in the target owner using274.V20=13ACC0.
   Its descriptor ID is the compact index minus that base node's2478 start.
   Force-duplicate is false. FFFF is failure. This occurs **before** symbol
   resolution; an ensuing failure must retain that allocated row.
6. Call216860 with actual AL, the base node/arc, compact symbol1C,
   conversion low byte0, previousFFFF and nextFFFF. Its first output is the
   emitted symbol; second is normalized-original; external variant starts0.
   Pass emitted symbol/count1/component0/external variant to274.V30=13AF30.
   The current Russian no-wide/no-Arabic scalar resolver and same-graph
   replacement callback can supply this branch, but the callback must not
   be replaced with a fabricated character.
7. Resolve the actual new glyph record. Request frontend1B8 with base
   node/arc and **normalized-original**, store its float at10. Frontend1B8
   forwards acoustic.V80=1C1300 and the owned knowledge88 channel0. Its
   calibrated vector is a different channel from candidate normalized weight.
8. Build the exact16-byte frontend1C0 request. Node0 and arc4 are the base
   source, candidate ordinal6=FFFF, symbol8=normalized-original. GeometryA
   isFF for24DC (actual F0001); otherwise use selected category metadata:
   EndCompositeMap when compact20 bit40 is set and its map entry is not-1,
   StartCompositeMap otherwise, then CompositeCategories.GeometryIndex.
   BytesB/C and remaining request residue are native zero in this request.
   Store the returned integer at14; a genuinely absent callback uses04000000.
   The actual ordinary frontend has the callback, so absence is not a
   production substitute for its cost producer.
9. Store0C as `float(compact.byte21) /255.0f`. Set/clear descriptor04 bit8
   from compact20 bit40, retaining other bits. This is not calibrated1B8 and
   is not a second call to normalized-weight generation: compact byte21 is
   already owned by the actual207CB0 producer.
10. If2368 were enabled, native copies0x288 side-cost bytes from the actual
    compact-side owner into captured payload. The current fresh owning path
    refuses that configuration. The frontend280-controlled source flags also
    remain outside other graph types. Continue in stored compact order.

Thus a refresh can add unselected candidates to a source owner. Existing
WORD glyph links remain valid, but the later13A830 alternatives vector need
not contain only those links. The freshly restored selection kernel must run
**after** this real owner mutation, not on a forged selected-only substitute.

## Source identity is the most important remaining join

For an ordinary base arc,1AA470 returns the full input source key and calls
1A9AE0 with its actual context endpoints. For a higher arc, it exports merged
source membership while recursively resolving **originalLast** as the base
key for compact scoring. These two identities must remain separate: target
owner/merged sources versus original acoustic group/compact frame interval.

`LegacyOwnedRussianMainResidualSource.ResolveBaseReference` already restores
that original-last relation for its owned higher ranges. Its
`CopySourceRanges` returns **sampled point ranges**, not the24-byte raw source
records produced by1AA470. `LegacyOrdinaryGraphSourceRanges.ResolveCandidate`
implements the ordinary forward1A9AE0 source-key path and owns chronology,
but its public result contains only first/last triple keys. It does not
currently export the separate2-byte residual array used by this callback.

That distinction is observable: native residual endpoint bytes are generated
from the exact prepared/source chronology and can affect later source-point
projection. They cannot be silently filled with zero, reconstructed from a
whole-word box, or replaced with sampled point ordinals. A typed V140 bridge
must preserve actual source-count preflight/fill, sorting/coalescing and
residual bytes. Forward ordinary base/higher-kind0 branches can be scoped
explicitly; reverse/protocol/other higher-kind paths must refuse until ported.

The normal forward residual producer is recoverable from existing owners.
`1A8E60` converts each actual sampled range into this temporary32-byte row:

| Offset | Native producer | Already retained owned value |
| --- | --- | --- |
|00 pointer /19 byte|source.V90 at range start|prepared chronology `First.Key` / `First.RemainderByte`|
|08 pointer /1A byte|source.VA0 at range end-minus-one|`ResolveEndingMetadata(lastStart)` key and remainder|
|10 pointer /1B byte|source.V90 at range end-minus-one|prepared chronology `LastStart.Key` and remainder|
|18 bit0|source.VC8 at range end-minus-one|the proven ordinary `Tail` condition|

Normal1A9AE0 sorts these rows and coalesces them with the existing native
endpoint/next-key conditions. It writes first triple from00 and first
residual from19. It chooses last triple from08 if tail is true, otherwise10;
the last residual comes from the **matching**1A or1B field. The coalesced
row keeps the first row's start identity and the final row's last identities.

`LegacyOrdinaryGraphSourceRanges` already retains precisely those four
values in its private `Row(First,Last,LastStart,Tail)` throughout the verified
forward walk/sort/merge. `LegacySourcePointIdentity` includes a real
`RemainderByte`; it is only discarded by the method's final key-only result
projection. An additive output method or shared-core refactor can therefore
export24-byte records plus the two actual bytes without a new NN, arbitrary
rounding policy or guessed zeros. The old key-only API/numerics should remain
unchanged. Original retained-key owner forwarding must remain intact after
physical reorder; indexing original metadata by current physical ordinal is
not equivalent.

## Negative arc branch

For a negative signed arc, the callback uses signed node24 and computes an
end node by adding the signed arc. Frontend188 is called twice to count and
fill span24-byte source references, with the actual source key's bit56
forwarded. It allocates24 bytes per reference, stores source scalar1C=1.0f
and does not scan ordinary compact candidates. It must not be implemented
by interpreting that negative arc as a graph group or using the source of
the nearest ordinary glyph. An owning span source-reference producer is
required separately; existing protocol keys remain opaque in raw UTF8 output.

## Existing services and proposed bounded adapter

| Required service | Existing verified implementation | Needed join |
| --- | --- | --- |
| Actual compact2460 +2478 | `LegacyOwnedMainCompactWordMaterialization` | Same reference as current publication/pool; no replayed golden buffers |
| Base/higher source identity | Main source geometry + residual overlay | Actual V14024B references and2B residual bytes, not sampled-range substitution |
|216860 ordinary scalar | `LegacyOwnedRussianGlyphSymbolResolver` and source callbacks | Same CASE/DIRECT source owner; emitted and normalized outputs kept distinct |
|1B8 and owner1C scores | `LegacyOwnedRussianMainGlyphConfidence` / group | Exact same sampled group, channel0; first-vector score and symbol-vector score separate |
|1C0 cost | `LegacyNativeGlyphScoreCache.ReadRequestCost` | Owning cache construction currently private in `LegacyOwnedRussianOrdinaryGlyphProducer` |
| Descriptor allocation/symbol update | `LegacyNativeGlyphRecordPool`, `LegacyNativeGlyphSymbolUpdate` | New descriptor's explicit allocator residue, partial-write ordering |
| Owner alternatives | `LegacyPreparedGlyphAlternativeSelection` | Refresh result lifecycle and actual optional marker |

A useful next runtime owner would bind one actual materialization, result
ownership/pool, residual overlay, sampled source and category/scorer owner.
It would retain source metadata separately from portable native-pointer
placeholders and reject foreign or stale components. Refresh would be
single-owner state with explicit partial-failure invalidation, then provide
the exact current pool snapshot to13A830. An additive internal owned cost
method can reuse the already verified private ScoreCache machinery without
constructing fake72-byte backtrack rows merely to call `Produce`.

However, a complete owning217670 adapter is **not ready from existing public
APIs alone**: V140 source/residual publication and preparation-callback/cache
lifecycle need closure first. The smallest useful following port is the
ordinary source-key/residual producer using the owned chronology, retaining
the resolved base key. It can feed this adapter without introducing manual
label aliases, fake scores or document object IDs. No such runtime edits were
made by this read-only task.

## Static evidence

Text PE SHA256:
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
Whole217670 interval `[180217670,180217DB0)` SHA256:
`8321a0ed9770f4e6f4ff036643ff8d1490ad6989dd616084b739dd5a2c0b176b`.
Actual table bindings read directly:

* 240 table2E7CB0+90=217670;
* 21B table2E1FF0+140=1AA470;
* 222 table2DE610+248=0CA630;
* 222+158=1529C0, +1B8=152F50, +1C0=152F90;
* 222+280=155710 ->21B.V1C8=05A400(false).

Assembly corroborates count26 allocation21786E, source residual-tail2178AA,
first-vector score store217920,216860 call217A5E, emitted-symbol update
217A7E/217AAC, normalized-original load217ADB and1B8 store217B04.
The symbol outputs were checked against the actual216860 signature/stack
arguments; the decompiler's anonymous locals must not reverse their meaning.
