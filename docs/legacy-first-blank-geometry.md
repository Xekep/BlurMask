# Owned first ordinary blank geometry

`LegacyCursiveBlankGapCost` restores the scalar `1941B0` arc-cache field 10. It selects current min-X or max-X through sampler flag `10C`, subtracts the preceding prefix max-X, divides by the line height, subtracts scalar `70` and multiplies by negative `108`. It preserves the binary32 stages, sigmoid-table indexing and native rounding. The rounded float is compared with the impossible cost before converting to an integer, so very large finite tails and overflow to infinity cap correctly.

`LegacyFirstBlankTransitionGeometry` binds this cache to the actual first factory, cuts, chronology and resource-cap/pruned source-range graph from `LegacyFreshFirstCandidateGraphPreparer`. First sampler `297/v106` has threshold `70=-0.5` and `10C=1`, so its blank cache uses max-X. The actual blank composite category is 33, first ID 6 and geometry prototype 9; these are read from the owned resource rather than inferred from a label name.

The ordinary kind-5 `195C40` branch first requires graph `108/1A82A0` to return zero. This endpoint supports the validated same-descriptor, unsuppressed, no-protocol, source-type-0 case. With `10C` enabled, every range of the current arc must start at physical point zero; when there is a next node, its arc zero must satisfy the same condition. It then adds cache field 10 and compares the resulting signed cost with the impossible limit. It does not apply the ordinary kind-0 translation correction.

The separate pair correction accepts an explicitly resolved previous geometric ancestor. It applies the blank category's `195B30` residual, or skips the residual when the supplied previous symbol is the owned alphabet's U+FFFD, as `1ACFA0` requires. It does **not** assert that the caller has performed native ancestor backtracking. Original prior acoustic cost and prior row identity remain separate inputs to the future controller; changing a span to disguise a skipped ancestor would be incorrect.

Independent Python controls cover 2,048 scalar min/max, lookup, rounding and cap cases. Three own raw fixtures pass the actual first-source preparation and pruning stage: printed MIR, a rectangle with an interior stroke and widely spaced vertical strokes. Their 51 bbox/prefix/cache states, 39 nonroot callbacks (14 accepted and 25 rejected) and 123 pair values are independently recomputed from captured prepared points and ranges. These fixtures enter the real pruning stage but happen to remove zero arcs. Managed and Native AOT validation also checks the replacement-symbol budget boundary and float overflow tails.

Blank acoustic fallback, blank proposal insertion, skipped-ancestor traversal and the complete controller winner are still separate missing consumers. This component does not make Russian or cursive recognition production-ready.

```powershell
python tools/LegacyFirstBlankGeometry.Probe/reference_controls.py D:/blank.json
dotnet run --project tools/LegacyFirstBlankGeometry.Probe -c Release -- D:/blank.json models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-separation-profile.json D:/actual.json
python tools/LegacyFirstBlankGeometry.Probe/audit_actual.py D:/actual.json
python tools/handwriting/audit_first_blank_geometry.py --resource models/ru-RU/ru_RU-ak-cur.lite.res --output D:/static.json
```
