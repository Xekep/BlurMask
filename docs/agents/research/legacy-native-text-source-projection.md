# Legacy decoded-source references to indexed raw geometry

The ordinary legacy bridge is not an implicit old247→modernEED adapter. MyScriptText `0283b0` consumes legacy273 and `024f30` converts its character/word source references through global service2BE.virtual30=`05afa0`. Normal references contain two signed `(group,inputOrdinal,pointOrdinal)` keys in24bytes. Optional first/end residual bytes represent fractions subtracted from the ordinal with float32 division by200.

`LegacyNativeTextSourceInputMap` implements `05a5b0` on **actual prepared**40-byte input rows and cumulative group-end offsets. An adjusted group≤0 uses the flat row vector. A positive group uses the preceding cumulative offset and its local span. `05ad80` traverses intermediate groups through those cumulative ends, preserving reference order and applying overrides only to the first/last resolved row. An input ordinal is not a Doc source object ID: each resolved row retains its independently owned indexed source identity.

The producer of those40-byte selected rows is actual MyScriptInk C20.virtual148=`06c9d0`, dispatched by Text `0569b0`. The fresh ordinary fields match the already recovered `LegacyNativeSelectedInkProducer`: source index, actual point count, first/last point, fractional endpoints and closed flags. Existing cached geometry is a separate path. Source wrapper prefix/group-offset production remains a required prepared input; legacy raw physical ordinals are not automatically interpreted as modern object IDs.

`LegacyNativeTextSourceRangeProjection` ports service2C1.virtual18=`053f90` for fresh ordinary rows. It computes selected-view relative length, honors−1 override sentinels, maps fractional first/end segments back to actual original point coordinates, then packs mode3/mode5 positions with the exact shared `053320`/Ink`00db00` float32 codec. It preserves native open-start7/open-end1 rules and terminalFF restoration. A quantized reversed output is refused, as in the native consumer. Known-text/C25, cached source geometry, invalid source-indexFFFFFFFF, nonnormal signed references and owner callback append/allocation failure paths remain outside this bounded API.

This closes an executable prepared legacy-reference→indexed-owner→actual cropped point geometry link. It does not fabricate a winning label, native group prefix, source owner or character bounds. The full `05afa0` status/dispatch preflight, winner materializer, other signed-key branches and runtime owner construction remain separate boundaries. Both binary contracts come from statically read pinned DLLs; no DLL is executed. Text SHA256 is `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`; Ink SHA256 is `b553d9742c369d70e118756aa36647020f2cdab151fe00d5e4a50afd3789a02a`.

Reproduce the independent original controls:

```powershell
python tools/LegacyTextSourceProjection.Probe/reference_controls.py D:/BlurMask-build-artifacts-20261011/text-source-projection-controls.json
dotnet run --project tools/LegacyTextSourceProjection.Probe -c Release -- D:/BlurMask-build-artifacts-20261011/text-source-projection-controls.json models/line-geometry/character-line-categories.json
dotnet publish tools/LegacyTextSourceProjection.Probe -c Release -r win-x64 -p:PublishAot=true -o D:/BlurMask-build-artifacts-20261011/text-source-projection-aot
& D:/BlurMask-build-artifacts-20261011/text-source-projection-aot/LegacyTextSourceProjection.Probe.exe D:/BlurMask-build-artifacts-20261011/text-source-projection-controls.json models/line-geometry/character-line-categories.json
```

Controls cover4096 exact-bit fractional projections,30720 flat/prefix lookups,256 cross-group/residual traversals and128 actual indexed raw-source compositions through C79 positions, cropped point bounds, portable category model, prepared IRLS and SDK line metric. In the composed fixtures source objects have a different order from decoded input ordinals, proving identity preservation. Resolved labels/endpoints are explicit prepared fixtures, not recognition accuracy. Source ownership, bounds, negative/sentinel values and cancellation are checked; no confidence or live application font-size correction is claimed.

The independent implementation caps references, resolved output rows and prefix rows at65536 and total group/row visits per projection call at1048576. The latter prevents repeated traversals of empty groups from consuming unbounded work despite a small output. These are explicit portable resource budgets, not inferred native model thresholds; exhausting them refuses the request without publishing partial output.
