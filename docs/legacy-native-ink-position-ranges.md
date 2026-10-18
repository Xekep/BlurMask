# Native source-position ranges and fresh C79 snapshots

This reusable port implements actual Ink source-range selection and the fresh
C79 position producer/query branch. It carries source object indices and point
positions; it does not fabricate recognized character boxes, final text labels,
font x-height or calibrated confidence.

## Producer and consumer boundaries

The default Ink context initializer **001910** binds its+8 table from descriptor
**0e5c90**, classC21, table**0e5ac0**. This proves the global+8 virtual148/150
callbacks used by **00f130** are **081ea0** and **081ed0**. Their input is an
ordered collection of16-byte source-position ranges. The high32 bits of each
endpoint are a **source object index**, not a native pointer or arbitrary UUID.

`LegacyNativeSelectedInkProducer.Select` expands each inclusive source-index
range in order, including repeated/overlapping ranges. It derives the same
first/last point indices, float32 fractional offsets and closed-end flags as
081ed0. FractionFF denotes a complete terminal endpoint; other fractions are
multiplied by float32.005. Partial first/last fields apply only to the outer
objects in each range. Interior objects use point0..last, both closed.

Actual source owner metadata supplies each object's point count and whether
its runtime kind is C25 known text. The convenience `CreateSnapshot` composes
these selected descriptors with the fresh **00f130** producer. The low-level
snapshot factory also accepts already prepared descriptors and the original
source collection explicitly, for independently recovered callers.

`LegacyNativeInkPosition.Pack` ports **00db00**. Static disassembly confirms its
floor callback **0cfc10** is IEEE floor (`roundsd` immediate9 or equivalent CRT
fallback), followed by separate float32 multiply200 and add.5. Quantized
fraction200 carries into the next point; terminal modes3/5 use fractionFF.
The portable modes and layout are byte0 mode, byte1 fraction, ushort point
index at+2 and uint source object index at+4. The implementation does not
rename these modes into unproved application actions.

Fresh snapshot production preserves start/end ordering, explicit intermediate
position markers and the separator after each selected source object. Known
text uses the two literal native positions `sourceIndex<<32|3` and
`sourceIndex<<32|0x3ff05`. That special representation is not a claim of three
recognized characters. Coincident ordinary endpoints require both endpoints
closed, as the native producer does.

`Canonicalize` ports **00ebe0**, including same-point fractionalFF handling
and mode conversions. `ReadRange` ports the **010850** fresh empty-cache branch:
full range returns a clone of the original collection; equal endpoints return
empty; separator-adjacent gaps can return empty; remaining intervals are split
by source separators and canonicalized. In particular, an endpoint range is
not a simple slice of raw stroke indices.

`ReadDecodedItemRanges` consumes the actual **046ae0** item-index/position-end
record fields. It starts the previous endpoint at0, obtains each interval,
and appends it only for a nonnegative item index. A negative item index still
advances the previous endpoint. No text label or rectangle is supplied to
this operation. Native source-collection append/coalescing is not implemented:
the returned ordered range segments preserve the input evidence instead.

## Bounded scope

The port owns value snapshots and portable indices. It rejects invalid source
owner references, nonfinite/out-of-range point positions, invalid modes and
oversized inputs; supports cancellation; and limits selections to65536 and
positions to1048576. Native heap allocation, pointer identities, **00f7e0**
InkMapping validation and any nonempty range-cache production/substitution
remain outside this API.00f7e0 is a validator, not evidence of a cache writer.
Applying this fresh branch to an owner with an existing nonempty range cache
would be incorrect. The [processed Math2 source proof](legacy-math2-source-position-transport.md)
also identifies the separate resampling/reordering position producers; this
fresh constructor does not create their fractional processed snapshot.

This stage uses no learned model or vendor DLL. Therefore a successful
geometry probe is not a compatibility or handwriting accuracy result for
old Russian247/135 and modern EED/EEE resources. The modern decoder's genuine
winning endpoint producer and old-model adapter remain unfinished.

Native global178 **083630** resolves each selected descriptor through the
original source owner's virtual90, then obtains that object's bounds through
the default context+0 C1E table (descriptor**0e6c60**, table**0e6980**, virtual48
**092e90**). Those tables are distinct from context+8 C21. Object resolution,
partial-stroke cropping, C1E bounds and their use by0531b0/C1A remain the next
integration boundary. The current app's connected-word bounding rectangle
has not been replaced with native line x-height by this batch.

## Independent controls

```powershell
python tools/LegacyInkPosition.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/ink-position-controls.json
dotnet run --project tools/LegacyInkPosition.Probe/LegacyInkPosition.Probe.csproj -c Release -- D:/BlurMask-build-artifacts-20261011/ink-position-controls.json
dotnet publish tools/LegacyInkPosition.Probe/LegacyInkPosition.Probe.csproj -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/ink-position-aot
D:/BlurMask-build-artifacts-20261011/ink-position-aot/LegacyInkPosition.Probe.exe D:/BlurMask-build-artifacts-20261011/ink-position-controls.json
```

An original standard-library Python oracle independently expresses floor,
float32 quantization, mode transitions, source descriptor expansion and
separator interval selection. Managed and NativeAOT controls cover10152
packing cases (including float32 neighbors of all200 rounding thresholds),
4096 canonicalizations,68 snapshots,128 full source-selection compositions,
and27850 intervals. They also check negative decoded-item consumption,
owned arrays, malformed references, cancellation and invalid inputs. None
of the expected text labels, models or native DLLs is executed by these tests.

Read-only PE exports: `InkC79PositionCodecNext`, `InkC79FloorProofNext`,
`InkC79SnapshotProducerNext`, `InkC79DecodedSourceIdsNext` and
`DocumentDecodedSourceItemsProducerNext` under the private artifact directory.
