# Prepared source point chronology

`LegacySourcePointChronology` restores source218 point-map composition, original metadata lookup and the signed three-field range sort used before native glyph normalization. Inputs and outputs preserve inserted and explicitly resampled point identities; no source IDs or timestamps are inferred.

It supplies:

* `CreateOrdinary`: the checked unsampled/default219 ingestion path, using keys `(0, input stroke ordinal, original point ordinal)` and implicit identity mapping;
* `CreatePrepared`: explicit current stroke counts, native key records, original-point-to-key index map, optional prepared fixed-point map and native source factor;
* `ComposePointMap` and `RemapPreparedPoints`: native map update arithmetic, including chained previous maps and fractional positions;
* `WithInsertedPreparedInk`: the191950/18dde0 global original-point-map bridge, using incoming factor1;
* `Resolve`: the native original metadata index, key record and optional remainder byte for a prepared point;
* `OrderRangeIndicesForNormalization` / `OrderAtomIndicesForNormalization`: original input ordinals after the exact native start-key quicksort. Ownership and geometry stay unchanged.

## Native mapping path

Interface218 table1802df340 has virtual30=`168b40`, which calls **16aa00** with incoming factor1. Its first map branch updates secondary+118 while retaining the existing factor at+128. With no previous map, differing factors yield:

`newTicks = uint32(sourceFactor * incomingTicks) / incomingFactor`.

With a previous map, `whole = incomingTicks / incomingFactor`, `remainder = incomingTicks % incomingFactor`. A zero remainder selects the previous entry. Otherwise:

`newTicks = uint32(previous[whole] + uint32(uint32(previous[whole+1] - previous[whole]) * remainder) / incomingFactor)`.

These are unsigned32-bit operations. Assembly16aa60..16aa98 uses32-bit `SUB`, `IMUL`, `ADD` and `DIV` with clearedEDX; the no-previous-map branch16aad0..16aae3 also wraps its product before unsigned division. The implementation preserves this arithmetic instead of widening the products or replacing truncation with rounding. It rejects invalid map access and zero factors before performing division.

The complete16aa00 function also includes physical-source subset copying when its additional parameter5 is nonzero. That branch is not implemented here. `WithInsertedPreparedInk` covers the proven sampler193380 call, whose parameter5 is zero and incoming factor is1. Prepared point counts and coordinates are maintained by the separate prepared-ink component; this API owns only metadata.

Virtual90=`168d00` first resolves a physical/local point into the current global coordinate index using secondary+28 and stroke-end counts+38. With a prepared map, it computes the original metadata position as **ceil(mappedTicks/sourceFactor)**. An optional byte is zero at exact positions; otherwise it is the low byte of `sourceFactor - remainder`. This low-byte behavior matters for factors above255: a positive remainder can still produce byte0. The original position is mapped through secondary+d8 and returned as the twelve-byte key record at secondary+f8. Assembly168d51..168d86 establishes the ceiling and low-byte conversion.

Inserted cut points retain the preceding global original-point index in18dde0, already restored by `LegacyCursivePreparedInk.Insert`. `WithInsertedPreparedInk` composes these repeated indices with the previous source map. Both inserted points and the original preceding point therefore return the same original key. It does not synthesize chronology from interpolated coordinates or new prepared-point ordinals.

Initializer163970 sets runtime219+8c/+90 to zero. Writer167210 stores the key's first field as signed+8c minus one when nonzero, otherwise zero; its second field is+90; its third field comes from the explicit original sampling map, or zero in the opaque-input branch. The stroke counter increments at167bc2. The ordinary factory applies only to the unsampled/default path. Explicit source hierarchies, opaque sources and resampled maps require `CreatePrepared` rather than guessed default keys.

## Ordering scope and ties

Consumer **1b4e40** obtains each candidate range's start key through virtual90 and calls **1b49a0**. That sorter compares the first three fields as **signed32-bit integers**, confirmed by `JL`/`JG` at1b4a13..1b4a55. It uses the middle record's key as its pivot and swaps equal keys. Equal keys are common after insertion, so replacing this routine with a stable generic sort can change native range order. The restored sorter preserves exact swaps and returns indices, allowing callers to reorder their own bounded ranges without modifying them.

This is candidate glyph chronology for normalization. It does **not** establish native line-DAG seed order, horizontal component placement, special-token insertion or non-linear skip arcs. In particular, the output of this sorter must not be advertised as complete native19f850 line ordering. The separate lattice endpoint still expects a supplied valid native ordinary seed order.

## Validation

Independent Python controls exercise1024 unsigned32-bit composition kernels and768 source/key/range cases. Managed and Windows NativeAOT probes compare **42823 original point identities** and768 signed/tie orders. Cases include decreasing explicit kernel maps, overflow-wrapped products, varied fixed-point factors, indirect key records, negative and extreme signed keys, repeated metadata, shuffled range ownership and native unstable ties.

Additional controls cover actual prepared insertion chains, later-stroke identities, chained fractional remapping with integer truncation, ceiling lookup and low-byte remainder at factor512, owned input snapshots, malformed maps/ranges, bounds and cancellation. These establish the recovered metadata branch; they are not whole recognition accuracy or complete native search tests.

```text
python tools/LegacySourceChronology.Probe/reference_controls.py .artifacts/legacy-source-chronology-controls.json
dotnet run --project tools/LegacySourceChronology.Probe -- .artifacts/legacy-source-chronology-controls.json
```
