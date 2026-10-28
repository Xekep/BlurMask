# Prepared dictionary query ring and pagination

`LegacyPreparedDictionaryQuerySource` restores the single-packed-label path of
Text `180093b50`, skip consumer `180091e10`, materializer `1800920a0` and
eight-slot invalidation `180092840`. It consumes the checked type2b4 dictionary
and prepared working/preserved/auxiliary maps. This is source enumeration, not
native word search or winner selection.

## Storage and query state

Dictionary reader `180248d90` selects single packed labels when wire flag bit4
is clear. The portable loader accepts wire flags0, including the actual Russian
dictionary. Native backend+10 holds that packed-label object, selecting the
guarded256-label-block path. Split-byte and split-ushort backends remain outside
this loader/API. Raw label and terminal bits come from the same validated graph.

`091ce0` indexes a backend node directly; there is no one-based conversion.
`093b50` additionally increments the child start for query node0, excluding the
first raw root child before map filtering. The old `GetTransitions` helper
implements the lower-level backend gate and does not perform this root skip.

Each query retains its source owner and selected slot, matching native fields40
and48. A new owner claims the next of eight slots. Wrapping does not invalidate
the old query's slot reference: node/signature comparison determines its rebuild
when reused. Reset's final eight-byte write at21fc also zeroes ring index2200.
Existing query references remain attached to their slots after explicit reset.

The cache key comprises node, requested accepted offset38, parameters1c/20,
shorts24/26 and auxiliary mode. Category byte18, context identities08/10 and
capacity3c are not keys. A cached accepted counter equal to the next requested
offset permits continuation; otherwise the query restarts and skips eligible
edges again. Rejected maps and auxiliary nonterminals consume raw positions
without incrementing that counter. Blocks hold at most256 raw labels.

Map selection uses auxiliary60, ordinary working50 or preserved58 when query
byte24 contains bit40. Permission refresh does not automatically recount an
existing cursor. `SetWorkingMap` and `ResetQueries` are therefore separate,
matching the native source lifecycle rather than adding generation invalidation.

## Records and explicit context service

Each emitted32-byte record owns the following layout:

| Offset | Value |
| --- | --- |
| 00 | next dictionary node, uint32 |
| 04 | zero byte4; byte5 flags root-origin1 and terminal2 |
| 08 | opaque context identity, uint64 |
| 10 | zero uint32 |
| 14 | mapped acoustic symbol, ushort |
| 16 | query category byte; byte17 zero |
| 18 | eight zero bytes |

No parent context selects the lexical-indexed initial identity table. A nonzero
parent without a context root emits zero. Otherwise an explicit lookup/create
callback receives the parent identity and **mapped acoustic** symbol. A nonzero
newly created identity invokes explicit parent linkage. Data is never treated as
native pointers. Native AVL allocation `0918d0`, its actual context factory and
word-history/controller binding remain separate work. Callbacks and query fields
must remain stable during a call; reentrant source mutation refuses.

Optional extra costs clear12 bytes per emitted record. An offset beyond all
eligible edges can return a negative native count; the port retains that count
and returns no records. Extra-buffer clearing refuses this case, where native
would request a negative `memset` length. Capacity is bounded to1..512, offset
to0..2,000,000 and node to the validated backend. Outputs own their arrays and
support cancellation without loading an SDK DLL or GUI.

## Verification

An original dictionary has600 root children and340 nonroot children, requiring
three raw blocks. The independent Python oracle filters semantic edge lists,
then slices them; it does not reuse native block/cursor loops. Managed and Windows
Native AOT agree on128 query streams,5,120 operations and165,045 complete records.
They cover ring reuse/reset, signatures, auxiliary/terminal gates, preserved-map
override, context lookup/linkage and signed out-of-range offsets.

Separate checks retain a cursor across pages, demonstrate permission refresh
versus explicit recount, forward non-key metadata, zero extra costs, reject
malformed requests and prevent reentrant mutation. The actual Russian backend
produces132 mapped root records after the native root skip, versus133 edges from
the lower-level gate. Native descriptor probe node1 is checked as a leaf. Category7
and zero context identities are explicit diagnostic inputs, not reconstructed
word-controller defaults or recognition confidence.

```powershell
python tools/LegacyDictionaryQuery.Probe/reference_controls.py D:/BlurMask-checks/dictionary-query.json
dotnet run --project tools/LegacyDictionaryQuery.Probe -c Release -- D:/BlurMask-checks/dictionary-query.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json
```

Query state/category producers, full context allocation, other dictionary
encodings, native word scheduling and final acceptance are not claimed restored
by this source API.
