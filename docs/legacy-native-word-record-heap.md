# Native word heap allocation and initialized-record lifecycle

`LegacyNativeWordRecordHeap` restores Text `1801f75b0`'s signed-cost max heap
over owned0x80-byte slots. It connects allocation to the checked initial word
prefix writer and the nontracing anchor/history tail. Language/bucket routing,
continuation records, row finalization and native winner/confidence remain
separate stages; the capacity is explicit caller state.

## Exact allocator behavior

A zero-capacity heap or candidate cost above3FFFFFF is rejected without
initialization. A full heap rejects a candidate whose signed cost is equal to
or worse than its root. An accepted full-heap candidate invokes the optional
evicted-root callback before any slot movement. Native reduction chooses the
greater-cost child, preferring the right child on equal costs, and shifts whole
0x80-byte records until the accepted cost fits. An append bubbles up only when
its cost is strictly greater than the parent's cost. These rules preserve the
native encounter/tie order, rather than replacing the heap with a stable sort.

The allocated slot is cleared at bytes0..77, while78..7F are retained from the
selected storage location. Cost is written at10 before the initializer runs.
The API can receive explicit initial storage, so tests do not assume a native
allocator returned zeroed bytes. Returned snapshots and eviction data are owned
copies. Mutation of the same heap during its callback is rejected; callbacks
can route an evicted record to a different heap. Initializer failure can leave
a partially initialized allocation, as documented; transactional rollback is
not claimed. The initializer must retain the allocated cost.

`TryInsertInitialPrefix` connects this allocator to the restored0x58 writer,
leaving tail initialization explicit. `TryInsertInitialRecord` additionally
uses `LegacyInitialWordHeapTail` for the checked nontracing branch.

## Anchor/history tail producer

In `1f8d60`, heap offset58 is set to ulongFFFFFFFFFFFFFFFF. When byte2609
suppresses the prior context, node68 is-1 and context70 is0. Otherwise the
anchor is the initialized record itself, unless its ushort3C is notFFFF;
then native selects the supplied row at node38/arc3C. Previous node is
`~signed(anchor[20]) + currentRecord.node38`, with32-bit arithmetic.
When it is nonnegative, the row at that node and anchor's ushort1E supplies
its ulong28 to heap context70. The port requires those row records through an
explicit callback, preserving lookup order and identity. It never substitutes
an invented previous record or context. Trace cloning and offset78 are outside
the nontracing tail producer; that storage remains untouched.

## Independent and resource-backed controls

256 independent Python streams express insertion/reduction as functional tree
paths and slot permutations, rather than replaying the port's iterative loop.
Every retained cost is also checked against a separate mathematical best-K
multiset. Managed and Windows Native AOT compare12288 allocation attempts,
3805 accepted records and1732 evicted roots byte-for-byte. Fixtures include
equal costs, negative/extreme costs, zero/full capacities, rejection timing,
complete slot copies and retained78..7F bytes.

256 further controls supply independent word-row records and verify complete
tail bytes and conditional anchor/history lookup order. Actual Russian resources
run prepared MIR geometry through neural/context/final195-cache and acoustic
candidate costs, then through allocation, initialized prefix, nontracing root
tail and the existing category/source-boundary consumer. Ten accepted acoustic
candidates produce eight allocations/four evictions and four retained records
at the explicit capacity4. Their costs equal mathematical best-K and their
category/source references and root tails remain intact.

The resource test uses explicit record tags/span and the existing original line
frame; it verifies record lifecycle transport, not virtualF8 enumeration,
language heap routing or SDK-equivalent handwriting recognition. No winner or
acceptance confidence is emitted.

```powershell
python tools/LegacyWordHeap.Probe/reference_controls.py .artifacts/legacy-word-heap-controls.json
dotnet run --project tools/LegacyWordHeap.Probe -c Release -- .artifacts/legacy-word-heap-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
