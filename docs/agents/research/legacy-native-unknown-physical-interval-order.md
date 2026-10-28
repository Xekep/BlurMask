# Prepared unknown physical interval order

`LegacyNativeUnknownPhysicalIntervalOrder.Order` ports the grouped ordinary,
horizontal, coordinate-mode2 branch of `15F050`'s physical interval insertion.
The caller provides genuine current physical point buffers and genuine provider
memberships. The prepared contract excludes associated physical records,
direction/boundary categories, vertical ordering, coordinate-mode0 and existing
recognized source strings. It does not produce memberships or source guides.

Each provider group is processed in its existing group order. IDs inside it are
sorted ascending as `0722C0` does. `0127F0` supplies float X bounds from every current
point. Fresh association?1 keeps those exact bounds; the associated-record0.3
contraction is not applied. Insertions do not cross group boundaries.

The forward scan (`15FBA1` through `15FD18`) finds the first existing interval for
which `newRight + sourceTolerance * primarySourceScale < existingLeft`. This
comparison promotes the serialized float tolerance and float right edge to the
native double expression. If it finds no interval, it appends the ID, preserving
native chronology even if a naive X sort would choose another place.

At an insertion, `15FD40` walks backwards while `newLeft < existingLeft`, using
strict float comparisons. Upon finding an anchor with left edge no greater than
the new left, `15FDC0`/`15FE1C` advance past subsequent intervals whose left edges
are strictly less than that anchor's **fixed** right edge. The anchor right edge
is not updated during that scan. Adjacent edges that merely touch stop it. The ID
is inserted after the resulting anchor. This is not a generic transitive overlap
sort or a simple coordinate comparator.

516 independent Python scalar controls cover random intervals/provider groups,
reverse input, exact contacts, zero width, equal left edges, tolerance equality
and fixed-anchor overlap. Managed and Windows NativeAOT run the same controls.
Bounds, missing/duplicate physical IDs, malformed parameters and cancellation
are checked. These controls need neither vendor resources nor vendor execution.

The function is a verified **prepared numeric consumer**, not a complete owned
`15F050` invocation. A real invocation must first preserve/reset the2E3 receiver
state on each new descriptor, derive its current groups from that descriptor's
points and carry the provider's neural scale between calls. `1680D0` also invokes
its E8 callback before the physical pass; depending on the actual owner, this can
materialize/deskew guides. Those producers must be restored before feeding this
consumer automatically from main ingress. No first-row guide or guessed callback
skip is used to claim that boundary complete.

```powershell
python tools/LegacyPhysicalIntervalOrder.Probe/reference_controls.py D:/BlurMask-checks/intervals.json
dotnet run --project tools/LegacyPhysicalIntervalOrder.Probe -c Release -- D:/BlurMask-checks/intervals.json
```
