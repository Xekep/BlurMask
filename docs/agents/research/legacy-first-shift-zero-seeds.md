# First21b zero-shift ordinary source graph

The first source21b@12727 and the main source21b@189379 share version10A but
have different serialized reorder flags at relative306..308: **01/00/00** versus
01/01/00. Their backward tolerance is.3 and insertion fraction.6. Selecting the
main profile merely because its version matches would change the first
acoustic-only controller's native source order and placement alternatives.

Reader `LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode` now accepts only
the demonstrated zero/one shift values, preserving all other existing guards.
The `19f850` producer already inserts a deferred seed at its chosen native
ordinary slot plus the stored shift; it now admits shift0 and retains the shift
on the owned result. The existing shift1 graph APIs reject a shift0 result
instead of silently applying their placement alternatives to it.

`LegacyOrdinaryCursiveSeedGraph.CreateShiftZero` implements the noNN, ordinary,
same-source branch of1a1ef0, keeping the earlier APIs unchanged:

Physical-source overlap flags use an equivalent interval difference pass:
each source crosses boundaries `first+1` through `last`. This keeps native
flags and order while reducing the calculation from scanning every physical
source at every node to linear time in node/source count. The independent
controls still use explicit interval tests.

* In the deferred placement loop, the lower bound is−2*resource12D. At0, only
  offset0 runs. The stored insertion displacement is0, so no mismatch bit is
  set. The displaced restoration loop's `(1−2*12D)<1` guard is false: it does
  not run. Thus each already-sorted seed has exactly one arc and one reference.
* Deferred-first and adjacent deferred seeds are valid in this branch. The
  root predecessor is−1; other nodes link to the sole preceding arc0, and each
  nonfinal node has sole successor0. No shift1 late-ink permutations are added.
* Initial start/end context IDs retain native `node*128` and `node*128+3`.
  The1a4f10 physical-overlap pass clears bit4 iff a physical stroke appears in
  the prefix excluding this group and suffix including this group. The new
  linear implementation computes the same predicate from each physical ID's
  first/last seed positions.

The scope remains **no neural source reordering, ordinary source kind, existing
geometry seed sort and confirmed profile flags**. It is not an arbitrary shift
implementation or a transcription winner.

`tools/LegacyShiftZeroSeed.Probe/reference_controls.py` constructs512 independent
geometric-slot cases. Ordinary boxes are disjoint, and each late mark has a
known nearest native insertion interval. Its expected sequence is constructed
per geometric slot, without duplicating the producer's nearest-distance loop.
It covers4413 arcs,258 deferred-first cases and355 adjacent-deferred positions,
including tied marks, root context IDs, one-reference linear paths, physical
overlap, cancellation, and cross-shift ownership rejection. Managed and Windows
Native AOT pass both vendor-free and actual first-resource profile modes.

All existing raw-source512, native-seed771, seed-graph384 and candidate-graph192
control suites were rerun in Managed and Windows Native AOT with unchanged
actual resource word diagnostic results. The source arrays/record producers
are headless; no vendor DLL or local GUI runs.

```powershell
python tools/LegacyShiftZeroSeed.Probe/reference_controls.py shift-controls.json
dotnet run --project tools/LegacyShiftZeroSeed.Probe -c Release -- shift-controls.json models/ru-RU/ru_RU-ak-cur.lite.res
```

The downstream successor/root accessor and actual five variable first-model
root groups are checked by `LegacyCandidateConnections.Probe`, described in
[the connections evidence](legacy-ordinary-candidate-connections.md). Full23f
packed record recurrence/three merging passes, result construction and winner
confidence remain distinct unfinished stages.
