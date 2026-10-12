# Native word-record comparisons, ordering and bounded pruning

This batch connects the eight `1fc*` record comparisons to `1fc650` quicksort,
the ordinary one-alternative duplicate loop in `2144f0`, `203de0` heap rebuild,
and the shrinking branch of `1f73d0`. It preserves full 128-byte records and
then feeds the existing `213410` row materializer and `1f9080` continuation.
It does not select the controller's global capacities or native output winner.

## Comparator proof

The exact x64 bodies in `MyScriptText.dll` provide the signedness and field
ordering. `LegacyNativeWordRecordComparer` exposes the literal address variants:

| Variant | Ordered fields |
| --- | --- |
| `1fc940` | U64 `00`, U16 `24`, U32 `18`, I32 `30`, I32 `34`, U16 `1c`, byte `21` bit7 |
| `1fc9c0` | U64 `00`, U16 `24`, low6 of U16 `1a`, byte `21` bit7, byte `27` bit0 |
| `1fca30` | `1fc940` with U64 `70`, I32 `68` before `21` bit7 |
| `1fcac0` | `1fc9c0` with U64 `70`, I32 `68` before `21` bit7 |
| `1fcb50` | U64 `28`, byte `21` low6, U16 `24`, U32 `18`, U16 `1c`, byte `21` bit7, U32 `50`, source priority |
| `1fcc20` | common continuation fields through `21` bit7, byte `27` bit0, U32 `50` |
| `1fccc0` | common continuation fields through `21` bit7, I32 `68`, source priority, `1f7230` payload |
| `1fcdb0` | common continuation fields through `21` bit7, byte `27` bit0, I32 `68`, `1f7230` payload |

Unsigned branches are `jb/ja`; signed fields use `jl/jg`. Ordinary field
comparisons return exactly `-1/0/+1`, rather than .NET ushort's arithmetic
`CompareTo` difference. Cost at `10` is not an equivalence key.

Source priority applies only when byte `04` IDs differ. If the left descriptor
header's `&1e00` is `1400`, the result is `-1`; otherwise a right `1400` gives
`+1`; otherwise zero. Thus two different IDs both having `1400` compare as
`-1` in both directions. This native behavior is preserved, not repaired into
an antisymmetric generic comparer. Descriptor headers are explicit live inputs,
not guessed acoustic categories.

`1f7230` takes the payload beginning at `40` and source ID byte `04`. It uses the
minimum first-byte count, compares differing source IDs when that count is
nonzero, then traverses its signed dwords backwards from that count to one.
The first nonzero signed32 subtraction is returned with native overflow wrap.
Different counts then order shorter first; equal counts fall back to signed32
subtraction of payload dword at `50`. Unsupported reads beyond the explicit
five-dword payload refuse. Differing IDs still short-circuit without reading
those dwords, as native.

`2144f0` chooses history variants when `22f6` is false and `(22e8 & 7)>1`.
Its frontend virtual `280` result (`local163`) chooses reduced variants.
This is an explicit frontend flag; it is not identified with trace mode `2368`.

## Quicksort, duplicates and rebuild

`1fc650` clones the middle slot as pivot, scans left while comparison is
negative and right while positive, swaps entire slots, and recurses lower
before upper. The implementation preserves that swap/recursion order with an
explicit stack, and never calls .NET generic sort. If a malformed comparator
input would scan outside its explicit partition, it refuses rather than
imitating an out-of-bounds native read.

The `2144f0` duplicate loop compares the current compacted leader with each
sorted record. With `2404 < 2`, a lower signed-cost duplicate swaps its entire
128-byte record with the leader; an equal-cost duplicate retains the leader.
Distinct records copy forward into the compacted prefix. `2402 > 1` and
`2404 > 1` require native `1f93a0`/`1f9830` alternative merges and are currently
refused, rather than replaced by a dictionary or a cost-only merge.

`203de0` resets the active count and reinserts the compacted prefix through
`1f75b0`, carrying all 128 bytes. Its targets cannot exceed the current source
index, so immutable source snapshots are equivalent to the native forward
in-place walk. The API takes complete capacity storage, including unused slot
bytes; it returns both exact sorted/compacted storage and the rebuilt heap.

## Capacity reduction

`LegacyNativeWordHeapPruning.ReduceCapacity` ports the `1f73d0` branch that
shrinks capacity to no more than the current active count. Native truncates to
that physical heap prefix and reinserts the old tail. A valid max-heap prefix
retains its physical order when appended into the new heap. Accepted tail
records trigger the callback on the old root before movement; rejected tail
records trigger it on that tail source record. Equal-cost replacement rejects.
The callback order and whole-record identity are retained, including capacity
zero. The returned heap is owned; the original input heap remains unchanged.
Callbacks may have external effects, and transactional rollback is not claimed.

Growing/reallocation bytes, the `212ec0` global capacity decision, cross-bucket
quotas, controller factors and multiple-alternative merging remain separate.
The caller's explicit new capacity is not presented as recovered native policy.

## Independent and actual-resource controls

Managed and Windows Native AOT passed 4,096 tuple-field comparator controls,
2,048 dispatch combinations and 256 functional immutable quicksort/permutation
and heap/pruning streams, checking all slot bytes and 607 dropped identities.
Controls include the two-special-ID non-antisymmetric case, signed subtraction
overflow in scalar comparisons and complete quicksort streams, ignored fields,
signed/unsigned boundaries, duplicate cost ties, whole-slot winner swaps,
capacity zero, cancellation and unsupported alternative merge refusal. Pruning
costs also match independently sorted mathematical best-K values.

Actual RU source, cuts, candidate graph, prepared context, NN families, final
cache and acoustic scorer feed per-bucket initial sorting/rebuild before native
row materialization. Six stored initial records and ten second-glyph acoustic
increments produce 60 explicit diagnostic continuations, 28 allocations,
14 promotions and eight retained records. Sixteen replayed real-resource paths
(eight explicit +1-cost diagnostic duplicates) become eight native equivalence
groups, then shrink to three records with five exact drop callbacks. An explicit
two-slot promotion consumer receives those drops. Materialized rows retain the
real category and original anchor node/arc, and feed a third continuation with
exact parent/root lookup and no invented previous context.

Comparator flags, diagnostic duplicate prior, bucket capacities and new capacity
three are explicit test inputs. The Cartesian diagnostic scheduling is not the
native language search. Native winner, confidence and production Russian
acceptance remain unfinished.

```powershell
python tools/LegacyWordOrdering.Probe/reference_controls.py .artifacts/legacy-word-ordering-controls.json
dotnet run --project tools/LegacyWordOrdering.Probe -c Release -- .artifacts/legacy-word-ordering-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
