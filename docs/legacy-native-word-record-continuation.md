# Native word-record continuation

`LegacyContinuationWordRecordProducer` ports the nontracing `MyScriptText.dll`
`1801f9080` writer and connects it to the native heap router/allocator. It takes
an explicit immutable parent row and explicit search-state inputs. It neither
chooses the next parent nor finalizes the native word rows or output winner.

## Exact lifecycle

The native parent lookup addresses `runtime+23e0[parentEndingNode]` with stride
`0x58` and the full parent arc index. Its old header byte at `+4` selects the
source-state descriptor and its bucket byte `+0xc`. The continuation uses the
same secondary/full-root promotion rules as `1f8c90`, but its primary branch
does not use the `2458` override. Routing precedes the new header write.

An accepted `1f75b0` slot receives all 88 parent bytes. The writer then replaces:

| Offset | Written field |
| --- | --- |
| `00` | explicit 64-bit source tag |
| `08` | explicit opaque 64-bit auxiliary value |
| `10`, `14` | total and incremental signed costs |
| `1e` | parent arc narrowed to the stored ushort |
| `20` | `ff`, the continuation span sentinel |
| `21` | low six state bits; inherited high two bits remain |
| `27` | low secondary bit; inherited high seven bits remain |
| `28` | explicit opaque 64-bit context value |
| `3e` | explicit ushort state |
| `40..53` | five explicit dwords |
| `58..5b` | `ffffffff` |
| `5c..63` | explicit opaque 64-bit auxiliary value |

Unlike the initial writer, the continuation writes only four `ff` bytes at
`58`. Calling the initial tail helper here would destroy the auxiliary value
at `5c`; the continuation ports its own verified tail overlay.

With runtime flag `2609` false, the anchor is the new prefix itself unless
`anchorArc3c != ffff`. In that case it is the exact stored row at node `38`, arc
`3c`. Previous node at `68` is the signed computation
`~(sbyte)anchor[20] + current[38]`. If nonnegative, context at `70` comes from
the stored record at that previous node and `anchor[1e]`, field `28`.
With `2609` true, node `68` is `-1` and context `70` is zero. The record's
inherited category and anchor fields are not reconstructed from a glyph or word.

The knowledge property `+ec` scale-zero branch forces the effective secondary
byte to zero before both routing and storage. The API returns allocation
details; it does not imitate the native function's outer return convention
(which returns one even when allocation rejects).

## Bounded inputs and ownership

The lookup must supply exactly one immutable 88-byte stored row. The wrapper
captures a parent snapshot once; native reads its header before allocation and
copies it after allocation. Their equivalence requires the explicit row to
remain immutable during allocation/promotion. Each allocated slot is exactly
128 bytes; the allocator has already zeroed `00..77` and retained `78..7f`.
The standalone writer preserves bytes it does not write. Missing required
anchor/history lookups refuse instead of supplying an invented record.

Opaque tags, payload, context and auxiliary values are transported without
dereferencing. Trace mode `2368`, its five-record/640-byte clone, native row
storage/finalization, language state scheduling, priors, winner and confidence
are outside this API. The ushort parent-arc input is a checked subset of the
native int lookup parameter; indices beyond that stored width are not exposed.

## Independent checks and actual resources

Managed and Windows Native AOT passed 512 independent immutable-parent overlay
controls covering every slot byte and 497 exact anchor/history lookups. The
oracle uses declarative field overlays over random parent/slot bytes and a
separate immutable row dictionary. Controls include scale zero/nonzero, bit
ownership, inherited anchors, overlapping `5c` transport, cancellation,
nonfinite scale, malformed payload, unavailable and truncated row lookups.
A routing control uses a parent's old state byte different from the new tag
and an active initial-record override, proving that continuation ignores that
override and routes using the old parent header.

Actual RU ingress/cuts/graph/context/neural/cache/scoring produces six retained
initial records. A second actual glyph classification provides ten accepted
acoustic increments. Explicit Cartesian diagnostic scheduling runs 60
continuations through native writer/router/allocator, with 27 allocations,
13 successful promotions and eight retained records. All 87 parent/history
lookups address the supplied real-resource rows. The resulting category and
source-boundary consumers preserve inherited state; explicit total costs retain
the best eight across the supplied heap capacities. This is a lifecycle test,
not a recovered native search scheduler or handwriting accuracy claim.

```powershell
python tools/LegacyWordContinuation.Probe/reference_controls.py .artifacts/legacy-word-continuation-controls.json
dotnet run --project tools/LegacyWordContinuation.Probe -c Release -- .artifacts/legacy-word-continuation-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
