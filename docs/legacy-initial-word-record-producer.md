# Initial native word prefix producer

`LegacyInitialWordRecordProducer` restores the data prefix written by
Text `1801f8d60` after its heap allocator has supplied a slot. This produces the
actual0x58 word prefix consumed by the previously restored native reference
reader and category/input-boundary block. It does not select a heap, approve a
candidate, choose a previous word record or report a recognition winner.

## Allocation and exact initialized data

`1f8d60` asks bucket router `1f8c90` for a slot through allocator `1f75b0`.
After its capacity/cost and heap movement decisions, `1f75b0` zeros bytes0..77
of the0x80 heap slot. The prefix writer then assigns fields0..53. Bytes54..57
remain zero from that allocation; they are not treated as uninitialized data.
The standalone writer assumes an accepted slot and restores these writes only.
Allocator/router decisions and the0x80 heap tail are separate lifecycle work.

All initializer inputs are explicit. Some fields have offset suffixes because
their complete semantic ownership remains unresolved;64-bit opaque values are
copied as data and never dereferenced as native pointers. The fixed native
sentinels at22/3e are ushortFFFF, and the field at28 is ulongFFFFFFFFFFFFFFFF.
The five dwords at40..53 are zero.

The packed dword at18 is:

`sourceNode16 | ((compositeCategory &63)<<16) | ((returnedSourceArc &1023)<<22)`.

The composite category's six bits therefore occupy the low six bits of the
ushort read at1a by `LegacyPreparedWordPathReference`. The full source arc is
also stored at1c, the previous arc at1e, and the signed span byte at20. Other
header/state bits preserve the native writer's truncation/masking rather than
assuming every incoming byte is a boolean.

`1f8d60` first clears its requested secondary-heap flag when the live knowledge
container's float+ec is zero. The data writer takes that finite scale explicitly.
This is not a scalar read from the Russian AK resource: knowledge2b3 constructor
`00c860` initializes its live ec/f0 pair to zero, while property setter `00e680`
can replace it. The fresh default zero is known; application-specific property
setting and routing still require their caller state.

## Ordinary startup producer to existing consumer

`TryCreateOrdinaryStartupPrefix` covers the first ordinary symbol-map alternative
in `1fd240`, not all startup paths. That branch takes the selected symbol's
category from sampler derived data+50, the actual `StartCompositeMap`. The
writer receives that category, the current owner node, the source arc returned
by frontend virtualF8 and the low byte of the supplied source span.

The wrapper resolves these native fields using the real category profile and
retains all other supplied initializer fields. The current node is also the
initial anchor-ending node; previous/anchor arc indices areFFFF. Missing map
entries are outside the supported startup record and return false without an
invented category. Alternative end-map/special-symbol/controller branches and
the source virtualF8 enumeration/filter are not synthesized. The returned arc
and source span remain explicit native caller inputs.

The emitted bytes round-trip through `LegacyPreparedWordPathReference` and feed
`LegacyWordPathBoundaryCost` using the actual resource category mapper and a
restored ordinary graph/input-descriptor callback. This closes the writer to
the existing prepared-record consumer; it does not guess which previous record
the full204290 search should select.

## Verification and remaining lifecycle

512 original Python controls assign ownership of individual bits/fields in an
88-byte record. Managed and Windows Native AOT compare every byte, including
signed costs/spans, high source/arc/category bits, nonboolean byte inputs,
zero/negative-zero secondary scale, sentinels and untouched zero fields. The
native reference reader must recover the resulting category/span exactly.

The actual Russian profile tests every195 symbol entry:187 supported records
are emitted and8 missing entries produce no record. Tests verify the resource
map, complete source-reference fields, signed low-byte span conversion,
ten-bit packed versus full sixteen-bit returned arc, and default secondary gate.
These produced records feed the existing category/boundary consumer with an
ordinary raw-source/cut/seed/merged-graph/input-descriptor snapshot, including
five nonzero category2/4 corrections. Bounds, unsupported scale and cancellation
guards are separate controls.

The full decoder still needs native heap allocation/routing, source virtualF8
candidate enumeration, record continuation/anchor selection, case/state policy,
winner and confidence. This stage is not a handwriting accuracy claim or an
automatic Russian acceptance policy.

```powershell
python tools/LegacyInitialWordRecord.Probe/reference_controls.py .artifacts/legacy-initial-word-record-controls.json
dotnet run --project tools/LegacyInitialWordRecord.Probe -c Release -- .artifacts/legacy-initial-word-record-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```
