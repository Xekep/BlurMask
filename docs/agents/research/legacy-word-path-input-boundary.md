# Prepared word reference and input-boundary correction

`LegacyWordPathBoundaryProjection` restores a bounded producer/consumer bridge
inside Text `180204290`: native packed word-path fields, the actual sampler297
category mapper, ordinary primary graph group/input ownership and the
frontend input-descriptor boundary callback. It exposes the exact integer
correction for this block. It does not attach that correction to the existing
original dictionary beam by guessing a previous native word record.

## Source boundary is not physical F0 metadata

Frontend222 runtime table `1802de610` virtualE8 is `180152280`. It calls raw21b
virtual68 (`1801a77d0`) to resolve a graph node/arc's group, then virtual78
(`1801a7920`) to read that group's ushort input ID at group-record offset0.
Source218 virtual10 returns the corresponding native0x58 input descriptor;
frontend152280 returns its signed dword at offset10. A missing group returns-1.

`LegacyPreparedSourceBoundarySnapshot` owns those descriptor boundary fields
and supplied group/input indices, and performs the primary node/arc→group→input
lookup. Primary ordinary candidates use their restored group IDs directly.
Supplemental source-cell remapping, cross-source joins and graph mutation are
not synthesized. Explicit descriptor/group snapshots can contain distinct,
unknown or negative boundary values; only-1 is the native unknown sentinel.
This is a different field from physical record F0 at offset18.

The fresh factory covers only ordinary unsampled/default input without existing
recognized text, extended input metadata, scope setters or lifecycle mutation.
Initializer `163970` sets source219+94/+98 to-1 and new-scope byte8e to0.
Writer `167210` creates a low-type0 input descriptor, writes source+94 at
descriptor+10, and reuses that descriptor on later physical strokes while
byte8e remains0 and low type remains0/6. The bounded fresh branch never changes
the low type or requests a new input record, so it owns one input ID0 with
boundary-1. The factory validates graph range ownership against its supplied
ordinary prepared source.

Input dispatcher `1a61c0` loops the actual source input descriptors and passes
their ordinal (`local_res8`) to `19f850`. In the ordinary sampler loop,
`19f850` stores that current input ID in the temporary atom record's offset0
(line323), preserving it in the actual atom group. Its auxiliary placeholder
record is separate and must not be treated as an ordinary atom. The merged
group writer in `1a3b30` copies the left group's input ID at lines298-300; the
checked same-source branch retains input0. The factory's-1 is therefore a
proved fresh **source boundary**, not an invented runtime glyph category.
Other source contexts require supplied descriptor records and group input IDs.

## Packed previous-word fields and category block

`LegacyPreparedWordPathReference.Read` reads exactly one native0x58 word-path
record. Category bits are the ushort at1a; source span is the signed byte at20;
ending source node is supplied from the record's owner row. The span's signed
load is confirmed by `MOVSX` at `1802050ff` (and related204290 branches).
It is not inferred from output string length, glyph count or a candidate's
placement byte. This adapter does not select the previous native word record.

The checked block at204290 lines643-672 is active only when runtime byte22f6
is zero and the packed path type's bits1c exceed7. It maps category bits' low
six bits through sampler297 derived table+70. The mapper consumes the actual
35-entry Russian resource table; it does not substitute the caller's generic
word-category option or a constant category.

The source start node is `endingNode - signedSpan`; preceding node is start-1.
When preceding is nonnegative, start is below the supplied actual available
node count and a callback exists, native queries preceding node **arc0**. Only
if that result is not-1 does it query start node arc0 and compute the signed
32-bit wrapped boundary difference. An unknown current boundary still differs
from a known previous boundary; it is not silently skipped. Missing callback,
unknown previous boundary or unavailable source nodes leave difference0.

The block adds8192 only for mapped category2/4 when preceding is-1 or the
boundary difference is nonzero. `LegacyWordPathBoundaryCost` returns that
projection; it does not invent source record selection, current lexical type,
native beam/winner, trace-buffer activation or acceptance confidence. Those
inputs and producers remain separate recovery work.

## Validation

1024 independent Python controls serialize original0x58 word/input records,
assign independent group/input ownership, and compute the conditional result
without calling the port. They exercise high category flags, signed spans,
root/interior/out-of-range starts, mode/path gating, absent callbacks, unknown
previous/current boundaries, repeated values and signed difference wrapping.
The managed and Windows Native AOT probes compare correction values and the
exact conditional source-lookup order using the actual Russian35-composite
profile. They check every primary merged-graph node/arc and fresh ordinary
source input, as well as missing-group, malformed-record, invalid-input and
cancellation behavior.

This is not a full native word-record producer or word search. The packed
previous reference remains explicit; restoring its allocation/selection and
case/category state is required before automatically using this correction
in the application.

```powershell
python tools/LegacyWordPathBoundary.Probe/reference_controls.py .artifacts/legacy-word-path-boundary-controls.json
dotnet run --project tools/LegacyWordPathBoundary.Probe -c Release -- .artifacts/legacy-word-path-boundary-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```
