# Ordinary native frontend symbol qualification

`LegacyNativeWordSymbolPreparationSession` connects actual ordinary graph cache
preparation and source permission refresh to the frontend110 symbol qualifier.
It owns the graph/cache session and emits a detached qualifier together with the
real permission pass. It does not manufacture a language mask, source ID, guide
frame, query context or accepted word.

## Static producer and consumer

The Text frontend222 runtime table `1802de610` binds slotD0 to `1801520e0`
and slot110 to `180152520`. The ordinary backend21d runtime table
`1802e4180` binds slot50 to `1801c0f60`.

`1520e0` first calls the graph preparation and backend48 cache/source-permission
producer. A failed preparation returns false. After successful preparation:

* With param5 nonzero it returns immediately. Existing frontend88 bytes survive
  unchanged, including noncanonical nonzero byte values. This branch still runs
  cache preparation and source refresh; it is not an alias for a fresh union.
* With param5 zero it clears exactly alphabet count plus3 bytes, visits every arc
  at the actual anchor in ordinal order, selects graph.virtual70 span equal to
  the requested span, and calls backend50 for each selected arc.
* `1c0f60` resolves the actual group cache, then writes each byte as
  `cost < 0x4000000 || previousByte != 0`. Negative costs qualify. A cost exactly
  equal to the impossible sentinel does not qualify. The extra three slots are
  included; the union does not apply the native acoustic scorer's threshold.

The new session constructs the existing
`LegacyOrdinaryGraphAcousticPermissionSession` itself. Thus row selection,
group identity, computation/reuse, cache-buffer ownership and source permissions
share one actual graph. On the fresh union branch it verifies the emitted ordinal
positions and group IDs against that graph before consuming the final cache rows.
Caller-provided row-kind or lifecycle flags are not used.

`152520` checks frontend98 only when query length is nonzero, context is nonnull
and the optional bitset exists. It follows context parent links. Every context
symbol below the base alphabet count must have its bit set; control and extra
symbols at or above the base count are skipped. It then tests the current
symbol's frontend88 byte. Query length is an enable condition; it does not limit
parent traversal. Frontend98 presence is explicit, never inferred from locale.

The earlier `150d00` optional frontend98 allocation depends on frontend38.
This batch does not restore that configuration owner or synthesize its bitset.
The resource-backed test uses the supported absent-bitset branch.

## State and ownership

The first reuse call requires caller-supplied exact existing frontend88 storage
or a successful earlier fresh preparation. Missing storage is unsupported;
uninitialized native allocation bytes are not represented as zeros. Input bytes
are copied. Successful published qualifiers remain immutable across later pool
reuse, table clear/refill and source refresh. Public snapshots are detached.

Calls must be serialized. A failed, canceled, unsupported or throwing preparation
invalidates the session. It returns no usable qualifier on a failed cache pass.
New cancellation checks cover callback return and final publication. Alphabet,
bitset and table extents are bounded before allocation; context traversal is
bounded by the owning context container's node count.

`LegacyNativeWordSymbolQualifier` also exposes the bounded table/consumer over
explicit final-cost rows for independent controls. That constructor alone does
not claim recovery of native graph selection.

## Independent verification and actual resources

`tools/LegacyWordSymbolQualification.Probe/reference_controls.py` independently
builds1024 finite-cost/bitset/parent-path cases and256 owning cache traces with24
passes each. It includes exact sentinel boundaries, signed extrema, absent and
present optional bitsets, zero and nonzero query length, control symbols, empty
selection, repeated computation, cross-node pooled cache reuse, and the important
nonzero-param5 table-preservation branch. The first reuse preserves bytes
`128,0,1,2,0,255`; later fresh preparation produces canonical0/1 bytes. Earlier
qualifiers and source buffers remain unchanged. Additional controls reject missing
storage, width mismatches, failed passes, callback cancellation, invalid context
ownership and pre-canceled qualification.

The real pinned Russian resources test constructs ordinary cuts and candidate
graph, a prepared linguistic mask, actual acoustic cache producer, and actual
source permissions. The whole-word span selects one real NN/final-cache row.
The permission-filtered native typed dictionary query emits10 labels in one page;
all10 pass actual frontend110 qualification. Actual source-root successor order,
both serialized category-map variants, exact startup80 requests and frontend128
scoring produce10 accepted allocations and6 owning heap rows. The test compares
the qualification producer's final class costs with independently composed cache
and geometry costs before using them in the acoustic request scorer.

These are producer/consumer transport controls, not recognition accuracy or
acceptance-confidence evidence. Guide90/80 and external cache switches remain
explicit diagnostics. Main first-result-derived source/frame ownership, complete
`1fd240` special/case and writer-field production, parent continuation`201270`,
full scheduler`2144f0`, winner selection and calibrated UI acceptance remain
unfinished. No complete Russian AI-pero or full native-engine claim follows.

Run the generator, then the managed probe with these five arguments:

```text
controls.json
models/ru-RU/ru_RU-ak-cur.lite.res
models/ru-RU/ru_RU-lk-text.lite.res
models/ru-RU/legacy-engine-profile.json
models/ru-RU/legacy-separation-profile.json
```

The same controls and resource consumer run in Windows NativeAOT. Static review
uses decompilation and raw table/assembly reads; no vendor DLL is executed.
