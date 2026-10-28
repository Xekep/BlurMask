# Native ordinary21b successor selector and source root

`LegacyOrdinaryCandidateConnections` wraps the validated ordinary noNN candidate
graph with the exact successor count/ordinal interface needed by first23f
initial/continuation glyph requests. It does not substitute a predecessor set
for a selector-specific outgoing list or manufacture a winning transcription.

## Getter and producer evidence

Native addresses and offsets are hexadecimal. Class21b table2e1FF0 slot50 is
1a76a0 (count) and58 is1a7700 (nth ordinal). Each uses the supplied node and arc
ordinal plus an extra byte selector;58 additionally takes unsigned16 nth.
Node−1 resolves the special source-root record228/230 and ignores the ordinary
arc ordinal. Other nodes address array248 and0x20-byte arc records. Their
record+8 points to predecessor/successor data.

In the linked branch254=false, successor selectorS reads the linked head at
data+10+S*10 and unsigned16 count at+18+S*10. Each0x10-byte linked node holds
unsigned16 ordinal at0 and next pointer at8. Missing nth returnsFFFF. The
compact branch reads the same logical vector through boundary differences and
the packed ordinal array; compact-table production is outside this wrapper.

Producer1a37b0 copies right-arc outgoing vectors with19d3b0, preserving order.
It copies left predecessors, then adds its newly allocated arc ordinal to each
previous arc's vector at `(merged.span+1)*0x10`. When the previous node is
negative, it writes the same vector on the source root.19d1d0 appends to the
linked **tail**, without sorting or set deduplication.19d3b0 also copies/appends
in original order. Therefore the native byte selector is **current span** and
corresponds to `LegacyCursiveCandidateSuccessor.NodeOffset=span+1`.

First controller1ee3b0 derives previous node as `current−span−1`. Its request
at+8 contains that previous node; request+10 contains the previous record's
arc ordinal (FFFF for the source root). Request+20 is the current span; it is
not a fixed selector0 or a general predecessor-mask flag. Frontend222F0/F8
(1522f0/152320) forwards these fields to the graph50/58 getters. The connection
allows the current arc only when the returned ordinal equals that current arc.

The adapter retains outgoing list order and duplicates from the bounded native
ordinary graph. Root vectors derive from arc.span==node and predecessor−1. Each
node/selector's native allocated arc ordinals increase as root entries append,
so this derivation retains the demonstrated construction order. Source-root
lookup ignores the supplied ordinal, and missing nth returnsFFFF. Cross-source,
special token/geometry and unrelated compact-array layouts are not inferred.

## Independent and actual checks

The probe reuses the exhaustive independent stable-move/partition oracle from
`LegacyCandidateGraph.Probe/reference_controls.py`.192 cases enumerate15194
complete partitions **through successor selectors and root getters**, covering
8352 ordered outgoing links. They preserve span versus placement, root ordinal
independence, list order, missing-nth sentinel, invalid bounds and cancellation.

Actual mode reads first21b@12727's **shift0** profile and feeds the owned first
247 source row through24a/265/297, native geometry seeds, `CreateShiftZero`,
ordinary candidate merging, root selector, first float context and NN, final
cache and ranked class/category query. Five variable root glyph groups reach
this actual resource-backed consumer; no second/main21b profile is substituted.
Symbol masks/pairs remain explicit, maximum5 atoms is the fixture's bounded
diagnostic scope, and there is no full23f beam/winner claim. Full packed record
lifecycle and three merging stages remain open.

Managed and Windows Native AOT execute vendor-free and actual modes:

```powershell
python tools/LegacyCandidateGraph.Probe/reference_controls.py candidate-controls.json
dotnet run --project tools/LegacyCandidateConnections.Probe -c Release -- candidate-controls.json
dotnet run --project tools/LegacyCandidateConnections.Probe -c Release -- candidate-controls.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json
```
