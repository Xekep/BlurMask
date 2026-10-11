# Ordinary unknown-row pair-to-group consumer

`LegacyUnknownLineGroupSelector.Select` ports the empty-prior ordinary branch
of Text `18007c160`: no supplied guideline rows, no known/locked physical flags,
and no pre-existing groups. It consumes the actual pair evidence produced by
`LegacyNeuralLineProposals`. Source flags are a **supported scope restriction**,
not inferred from the Russian alphabet or presented as every native input mode.

The native caller `18007faf0` forwards a pitch of
`double(scale) * 1.4500000476837158` without user guideline rows. Pair processing
uses the descending-confidence middle-pivot sort `180072360`, including equal
key exchanges. Label-one edges are not joined. Physical membership initially
contains `-1`, and creating, extending, merging and deleting groups preserve the
native group-index updates. Unassigned strokes remain `-1`; this API does not
replace the later `18007a850` attachment/singleton/ordering pass.

The associated evidence table follows `18007be60`: physical endpoint order,
then `log(confidence)` and `log(float(1.0000001192092896-confidence))`, with
the channels swapped for label one. `180079750` accumulates float join/split
logs across the supplied group IDs. Its uncertain band uses the native
`-5`/`5.5` thresholds and the centroid-spacing predicate `1800794a0`.
`180078f30` weights mean Y by `max(arc length,1)`, sums weights in float and
weighted positions in double. The caller's `0.99` creation and `0.98` merge
guards remain unchanged.

The `180079280` adjacent-guideline exclusion is false in this scope. With
rowCount zero, a negative centroid keeps nearest index `-1`; a nonnegative
centroid plus `0.2*pitch` yields starting index at least `-1`, and the scan
only visits negative rows or no rows. Its nearest index therefore also stays
`-1`. The two indices cannot be adjacent. This derivation applies to positive
pitch and the supported zero-row branch; it does not remove that predicate
from other guideline modes.

The `180079a10` merge guard retains sorted physical-ID 5%/95% bounds and the
wide-flat-group overlap checks. Known-group lock checks are outside this
declared scope. Host `log` and floating arithmetic are not claimed to match
every vendor instruction bit for bit.

The lower 5% index is round-half-away of `float(count)/20`, not ceiling:
`18024d8a0` directly rounds float bits and uses the `0.5` subunit threshold.
The upper index instead floors `float(float(count)/20)*19`. An analytic
interleaved two-element flat-group regression distinguishes the lower-index
rounding from an incorrect ceiling implementation.

## Live model-to-word diagnostic

The new probe can run live C# geometry → native proposals → both embedded
networks → native top1/pairs → group selection → original class24a Gaussian
frame → native deskew → existing Russian cut/glyph/final-cache/word pipeline.
Before the geometry callback, a copy of each group's physical IDs is sorted as
in source `18015b260`. All three original Russian scalar nodes at offsets
5789, 11421 and 187976 are tested explicitly; no single default callback profile
is guessed. Initial scale, source direction/boundary metadata, other cache flags
and the existing bounded word search remain explicit diagnostic policy.

Managed and Windows Native AOT pass 386 independent cases and 473 produced
groups. These include equal-confidence histories, wide-flat strokes, near native
thresholds, merges/deletions, explicit unassigned strokes, malformed pairs and
cancellation. The live own printed MIR fixture gives one group; two translated
copies give two groups. The inferred first frame is baseline 83.515 and height
71.425, with the second baseline 283.515. Using these frames makes `мир` the
best retained word for all three resource profiles and both row positions,
cost 54456. The former manually supplied 90/80 frame had it fourth.

This is a useful real-model regression for **one original printed fixture**,
not Russian cursive accuracy, a corpus evaluation or automatic UI acceptance.
The source219 scale sample/configuration chain, vertical sweep/pruning, full
`18007a850` post-selection policy, native winner/confidence and production
integration remain unfinished. No vendor DLL or GUI is executed.

```powershell
python tools/LegacyUnknownLineGroup.Probe/reference_controls.py D:/BlurMask-checks/groups.json D:/BlurMask-checks/proposals.json
dotnet run --project tools/LegacyUnknownLineGroup.Probe -c Release -- D:/BlurMask-checks/groups.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/ru_RU-lk-text.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res
```

Without the optional proposal-controls argument, the generator creates only
vendor-free scalar/group controls. Its probe accepts that single controls path.
