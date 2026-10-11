# Legacy spatial directional4f profile

`LegacySpatialWindowStatistics`, `LegacySpatialFeatureAssembly` and
`LegacySpatialDirectionalFeatures` restore the inspected first4×4 uniform
spatial313 profile. They complement the temporal3f profile; copying its local
features into the4f network would be incorrect. No trained model, calibration,
activation or other vendor asset is distributed, and no SDK authorization is
bypassed. All experimental models remain outside AI-pen UI.

The spatial local helper `1800b82c0` retains the same selected-point covariance,
directional reversals, physical endpoints and intersection counts as the
temporal helper. Its first four raw channels instead contain:

1. Selected-cell bounding width divided by whole-glyph width, exponent10.
2. Selected-cell bounding height divided by whole-glyph height, exponent10.
3. Selected-point count divided by whole-glyph count, exponent10.
4. Mean cumulative physical position divided by total physical arc, exponent10.

The native first truncates the cumulative position mean and then scales/divides
by total arc. Pen-up gaps are absent from the caller cumulative array. Empty
cells yield raw zeros. The native local coordinate unit follows the existing
checked temporal rule: a negative scale shift contributes zero, rather than
the post-resampling whole-glyph unit. Flags/covariance use the shared restored
arithmetic. Counts/lengths and membership IDs remain bounded; unsupported
nonmonotone or overflowing cumulative domains are rejected.

The serializer `1800ba060` preserves bias1024 and the global17 prefix. For its
local cell transforms, width/height use divisors1024/columns and1024/rows;
density and normalized loop fraction use2048/(rows+columns). All are256 in the
inspected4×4 profile. Temporal local features use different slots and divisors.
Signed covariance and integer count transforms preserve short wrapping; the
caller-supplied spatial274 calibration applies before the shared raw baseline9,
context28 and boundary2 append. Values are not silently clamped. The inspected
static default spatial tables are `1802b70e0`/`1802b7310`, distinct from temporal
calibration; the public API supplies no vendor defaults.

Composition includes the previously checked power-of-two coordinate prep,
target100 physical resampling, reversal/intersection passes, global statistics,
uniform spatial memberships, local statistics, calibration and append order.
Caller baseline, context and boundary values are explicit. Additional spatial
grids, native automatic line/candidate fit, joined-character cuts and acoustic
ensemble/candidate masks are not supplied by this implementation.

Managed and Windows Native AOT pass640 independent spatial-statistic controls
and512 independent calibrated274-channel controls, including signed/extreme
integer wrapping and equal calibration ranges. Analytic tests cover a line,
singleton, empty cell, zero total arc, density/mean position, transform divisors,
malformed domains and cancellation. Separate full313 composition controls
confirm identical shared global/append channels and differing local channels
between temporal and spatial profiles; the tests make no accuracy claim.

Optional caller-owned real-network diagnostics load RU4f313→200→180
(span467101..566296) and, optionally, temporal3f. They use the same original
six-physical-stroke printed МАМА fixture as the context-window probe, including
separate А crossbars and explicit line/baseline/reference40, matching caller
calibrations and boundary values1024. Managed and Native AOT agree:

- Without context, spatial4f heads М by Russian м at12233/32768, whereas temporal
  3f heads it by н at8727/32768.
- With restored context, the first М has spatial Latin m11151 and Russian
  м10474; the second has both m and м10712. Different language candidates need
  alphabet/case/dictionary policy, not a declaration of failed or successful
  complete-word recognition.
- Spatial А is headed by Latin A5971 without context and10892 with context for
  the first occurrence. Temporal3f heads it by Russian а19188 and18961.

This is a narrow diagnostic of independently restored family inputs. It does
not establish full МАМА decoding, broad printed accuracy or joined Russian
cursive readiness. The next requirement is the inspected native acoustic score
combination/candidate policy and word-level controls, not arbitrary averaging.

```powershell
python tools/LegacySpatialStatistics.Probe/reference_controls.py .artifacts/ru-spatial-statistics.json
dotnet run --project tools/LegacySpatialStatistics.Probe -c Release -- .artifacts/ru-spatial-statistics.json
dotnet run --project tools/LegacySpatialModel.Probe -c Release
# Caller-owned assets, plus optional matching temporal calibration:
dotnet run --project tools/LegacySpatialModel.Probe -c Release -- acoustic.res activations.json spatial-calibration.json temporal-calibration.json
```
