# Prepared compact word record publication

`LegacyPreparedCompactWordRecords.Append` restores the non-detailed-cost branch
of `206D30`. It appends the supplied symbol sequence to forty-byte storage2460,
links each record to its predecessor and publishes each generated record index
back to the caller. It does not produce that sequence, select the final word,
allocate native heaps or replace the full `21FFD0` backtracking controller.

The input contains symbol/kind from the native twelve-byte descriptors and the
coordinate qword selected from the separate input table. The public inputs keep
these channels separate. Arbitrary coordinates and field00 are copied literally;
they are not inferred from a character count or reconstructed pointer identity.
The options name unresolved fields by their offsets rather than assigning them
unproven meanings. Caller flags13/14/15 are restricted to boolean values.

Each intermediate row writes field1e=FFFF and field24=-1. Its flag byte20 becomes
kind&7: the machine code masks the OLD byte with7 before the XOR replacement,
so its upper five bits are cleared. The final row replaces bits3/4/6/7, clears
bit5 and replaces the lower three kind bits. Bytes22/23 remain allocator residue.
Preexisting records, unused capacity and unused output-index slots remain intact.

Final bit6 first accepts either runtime251c/251e symbol without consulting the
beam/category. Otherwise a mismatched ushort beam node with a negative signed
beam flag, runtime24e2/24dc/knowledge0e symbols, or symbols>=FFEB clear it.
Remaining symbols compare beam1a&3F against the actual source category byte.
The category callback is evaluated only on that native branch. This layer does
not replace source categories with Unicode character categories or host casing.

Validation covers2048 independently generated full-storage cases/9216 records,
all final boolean flag combinations, signed beam flags, uint source nodes above
ushort range, special-symbol bypasses, lazy category reads, high kind bits,
untouched allocator residue and eight invalid/cancelled input guards. The static
audit checks nine decompiler bindings and thirteen exact instruction byte spans
against the pinned PE without loading or executing vendor code.

The caller still owns the actual `207CB0` symbol-sequence/coordinate producer,
source node→record offsets2478, optional detailed-cost bookkeeping2368, allocator
growth/failure and complete output segmentation in `21FFD0`. Prepared byte
publication is not evidence of completed Russian recognition or UI integration.

```powershell
python tools/LegacyCompactWordRecords.Probe/reference_controls.py E:/compact-controls.json
dotnet run --project tools/LegacyCompactWordRecords.Probe -c Release -- E:/compact-controls.json
python tools/handwriting/audit_compact_word_records.py E:/compact-static.json
```
