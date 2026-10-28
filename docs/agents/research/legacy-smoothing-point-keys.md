# Source265 smoothing point keys

`LegacyOrientationStrokePreparation.PrepareWithPointKeys` extends the checked
265 geometry producer with its native source point map at multiplier200.
`Prepare` delegates to the same core while keeping its previous result and
all geometry unchanged. The key-bearing result owns its physical point counts
and one integer key for every emitted coordinate.

The source265 virtual18 path173c10 invokes1728c0 with multiplier200, unlike
orientation's173b90 multiplier1/no-key-output path. The spline coefficients and
Bezier geometry are the same. Keys address the **current input global point
ordinal**, not wall-clock timestamps or new stroke IDs.

## Exact metadata emission

1728c0 tracks each accepted coordinate's original ordinal during its
last-accepted duplicate filter. A duplicate at the original final point updates
the last accepted metadata ordinal without moving that coordinate.
The short-stroke branches either emit every original point key or the accepted
ordinal keys, matching their respective coordinate paths.

For each smoothed segment, let`next` be the next accepted original global
ordinal. Initial output is the first accepted ordinal multiplied by200.
The recursive subdivision has two distinct native routines:

* 1722d0's terminal right endpoint emits exactly`next*200`.
* Its left subtree enters171f00. Every terminal in that entire subtree emits
  round-half-away of float`(float(next−1)+fraction)*200`. Both descendants of
  171f00 continue the fractional path.
* The right child of1722d0 remains1722d0. Consequently not every terminal is
  a fractional sample, and not every inserted point can reuse the whole
  segment endpoint. Splitting uses float`(from+to)/2`.

The expression is not linear interpolation between the previous and next
accepted input ordinal. After duplicate runs, its fractional part remains
relative to`next−1`. The native rounding helper24d8f0's bit shift/carry
implementation establishes half-away rounding; it is not floor or banker
rounding. Recursion stops at depth11 or the two strict squared-distance tests
documented in [stroke smoothing](legacy-orientation-stroke-smoothing.md).

Source173c10 stores the resulting map in state118/120. State16a130 initializes
factor128 to200. Source218's168d00 metadata getter resolves the ceiling of
`tick/200`, retaining the remainder byte. Truncating the map to factor1 would
destroy the inserted-point identity and change range chronology comparisons.
Existing source chronology helpers can retain factor200 and compose the later
297 insertion map without inventing point IDs.

## Verification

```powershell
python tools/LegacySmoothingPointKeys.Probe/reference_controls.py controls.json
dotnet run --project tools/LegacySmoothingPointKeys.Probe -c Release -- controls.json
```

256 independent full preparations check30613 exact integer keys,25971
fractional identities and1743 input duplicates. The oracle uses the existing
independent dense spline solve and a separate left/right metadata recursion.
It checks coordinates, every key, output counts, source ceiling metadata and
remainder bytes. All prior orientation, smoothing and source-orientation
Managed/Native AOT controls must also pass after the shared-core refactor.
The4096 output-point budget remains unchanged.

Associated/custom input, previous-map interpolation and hierarchy assignment
remain separate input-source paths. This phase does not enable Russian text in
the UI or establish final native word acceptance.
