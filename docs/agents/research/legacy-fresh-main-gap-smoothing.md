# Fresh main29B gap publication and265 chronology

`LegacyFreshMainOrdinaryPostprocessing.Prepare` consumes the actual owning
`LegacyFreshMainShearPreparation` output. It reads the next29B/v100 and265/v102
processors from the pinned main source, evaluates the restored non-mixture29B
branch, and smooths each genuine descriptor's physical interval with its
current main guide height. It preserves the original source metrics and
point metadata. No gap, first-row frame, replacement source720 box or result
label is supplied to this owning stage.

## 29B bounded branch

Text `175160` initializes processor4C to0; `1751C0` copies it when cloning.
`1763E0` reads the actual29B resource fields. `16A130` initializes fresh
secondary9C to−1. `175350`'s genuinely zero-gap branch writes the serialized
value1.5. This does not establish1.5 as a general missing-data fallback.

For nonzero gaps, `175350` center-sorts each descriptor's source720 rows with
`175260`. The restored quicksort swaps the complete(center,rowID) record,
including equal-center records; a stable host sort is not substituted.
The forward frontier retains the maximum prior right bound. Each normalized
gap uses float subtraction/division and clamps to0..4. Double accumulation
then computes population mean and variance over these actual observations.

The Gaussian-mixture blend is zero when no individual descriptor has more
than the profile's minimum gap count10. Several shorter rows may still
produce enough total observations to publish the mean/variance estimate.
When total observations are fewer than10 and variance is below the native
double comparison against `float(0.1)`,29B does not write secondary9C.
The owning fresh stage therefore retains−1 in that branch. Otherwise it
publishes `min(maximumGap, mean + meanOffset − stddevMultiplier*sqrt(variance))`.

`LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture` supports this exact
bounded branch and explicitly refuses a nonzero mixture blend. The prepared
helper accepts actual incoming9C to preserve it correctly; the owning fresh
wrapper derives−1 from its constructor lineage. The two-Gaussian EM fit,
restarts/intersection and active-mixture output remain unimplemented. This
stage is not a claim that all of29B is complete.

## 265 ownership

Text `173C10` iterates ordinary descriptor physical intervals and calls
`1728C0`. It uses the current guide height when source guide count80 is
nonzero, as it is in this owning path. It passes metrics secondaryA0/A4
from `15ABE0`, which precedes249/24B/24D. The wrapper therefore extracts
metrics from the original owned current source, preserving those values
rather than recomputing them from already rotated/sheared points.

The existing independently verified265 spline/subdivision implementation
produces coordinates and factor200 keys for every actual physical stroke.
Per-descriptor local input offsets are lifted into the full retained source
domain. `LegacySourcePointChronology.RemapPreparedPoints` then implements
the native `173C10` prior118-map composition, including uint32 subtraction,
product/addition wrap and unsigned division. Original physical/raw-point
identity survives both earlier physical permutations and fractional spline
insertion. Source720 ranges and geometry remain in the native pre265
publication domain; `173C10` does not rewrite that buffer.

## Verification and remaining scope

The independent control oracle checks160 cases:134 exact quicksort/frontier/
mean/variance/publication results and26 active-mixture refusals. It includes
equal centers, overlapping boxes, multiple lines and incoming9C preservation.
The real integration starts with six raw physical strokes and two lines,
runs both embedded2E3 networks, split/reorder, first acoustic/cache/protocol
recognition, actual720, main24B and24D. Its four source720 rows yield
gap−1/assignedfalse, then265 produces176 points. An independent dense NumPy
spline/Bezier oracle checks those coordinates and exact fractional keys;
an independent integer oracle checks the resulting original chronology map.

Managed and freshly published Windows NativeAOT use the same tests. The
optional static auditor reads pinned PE/resource data only. No vendor DLL,
GUI or installer executes. Runtime uses committed portable models.

```powershell
python tools/LegacyOrdinaryGap.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res E:/BlurMask-build-artifacts-20261014/gap-controls.json
dotnet run --project tools/LegacyOrdinaryGap.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-processing-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/gap-output.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/BlurMask-build-artifacts-20261014/gap-controls.json
python tools/LegacyOrdinaryGap.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res E:/BlurMask-build-artifacts-20261014/gap-output.json
python tools/handwriting/audit_fresh_main_gap_smoothing.py --output E:/BlurMask-build-artifacts-20261014/gap-static.json
```

The first fixture retains an explicit diagnostic all-enabled linguistic mask.
Automatic first mask policy, completed native winner orchestration and actual
empty-first24A fallback are separate remaining boundaries. Full raw handwriting
accuracy and live application font sizing are not inferred from these geometry
checks.
