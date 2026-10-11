# Sampler297 category bridge

`LegacyCursiveCategoryMapper` exposes the recovered sampler297 derived-table lookup independently of neural-network output slots. It owns copies of the frozen category profile's tables. Composite IDs, symbol-entry IDs, geometry prototype indices and original first-list IDs remain distinct.

The native source/runtime bridge is `196490` (source knowledge field+138, serialized type21b), frontend initialization150d00, frontend virtual48 target151bd0 and type21b runtime virtual170 target1acf30. The latter returns runtime+188. Initialization1a6980 clones source knowledge+88 (nested sampler297), stores the clone at+180 and obtains the derived structure through its virtual70. Optional282/2d4 wrappers are controlled by runtime flags; this mapper describes the confirmed unwrapped297 structure and does not invent their behavior.

For actual297, table1802e1490 slot+70 targets18008ee00. Its assembly is `mov rax,[rdx+40]; ret`. Builder0103e0 fills this derived structure's+70 U32 array with each composite category's original first-list ID. The four symbol maps at+50/+58/+60/+68 are respectively start, end, zero-start and zero-end composite IDs. Missing serialized mappings remain minus one. Zero-boundary composites map to first-list ID0. The text beam either takes the low six bits of its stored category byte or uses the start-symbol map, then looks up data+70. `GetBeamFirstId`, `GetSymbolCompositeCategory` and `GetSymbolFirstId` expose these exact, bounded operations.

Both Russian profiles contain195 symbol entries and35 composite categories. In encounter order, their derived first-ID table is:

```text
0 0 0 0 0 0 0 0 0 2 2 4 0 3 1 2 4 0 4 0 0 4 1 3 0 0 0 4 3 0 3 0 0 6 5
```

The probe checks this table in both original resource nodes, all four high-bit combinations for35 beam categories and invalid indices. It reuses256 independently generated serialized profiles and their independent two-dimensional category oracle. All four maps and derived first IDs are checked, including27102 missing mappings. Managed and Windows NativeAOT console runs pass.

These categories are not an identity mapping from the seven acoustic output slots, nor the complete lexical history category system. Special space/newline handling, optional runtime wrappers, scalar acoustic normalization and full candidate search remain upstream responsibilities.
