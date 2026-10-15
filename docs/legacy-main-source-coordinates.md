# Ordinary main source coordinate items

`LegacyNativeMainSourceCoordinates.WriteSingle` restores ordinary21B.VC8
(`19A1D0`). The main frontend reaches it through222.V220 (`152A50`) and
ordinary21D.V150 (`1C2510`). This method produces a packed source-node/arc
reference; the word "coordinate" does not mean screen X/Y or a glyph box.

The native method returns required count1 regardless of the supplied capacity.
If capacity is at least1, it writes `(node & FFFFFF) | (ushortArc <<24)` into
the first item's qword and clears its flag byte8. Bytes9..15 and all subsequent
items retain their previous contents. The supplied glyph-difference argument
is unused in this ordinary implementation. The managed method derives capacity
from complete sixteen-byte storage and rejects partial records.

The producer is intentionally prepared: it does not invent a graph node, source
arc, character position or glyph bounds from a recognized string. The caller
must supply the actual frontend node/arc. `2075E0`'s context surgery/difference,
count3A0/3A4/3A8 production and `206960` normalization remain separate stages.
Restoring this item writer does not complete `207CB0` materialization or Russian
recognition.

Independent controls cover2048 inputs, including24-bit truncation boundaries,
high-bit nodes, all arc-bit ranges, zero capacity, arbitrary padding and extra
capacity. Managed and Native AOT use the same full-storage comparisons. The
static audit pins the entire writer and both forwarding thunks against the PE
without loading or executing vendor code.

```powershell
python tools/LegacyMainSourceCoordinates.Probe/reference_controls.py E:/source-coordinates.json
dotnet run --project tools/LegacyMainSourceCoordinates.Probe -c Release -- E:/source-coordinates.json
python tools/handwriting/audit_main_source_coordinates.py E:/source-coordinates-static.json
```
