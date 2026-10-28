# Automatic geometry callbacks: sparse baseline branch

The recovered class-219 pipeline distinguishes user guidelines from computed line geometry. Its secondary `+0x68` array of 24-byte records is filled by the user-guideline setter `1801670b0`. Computed line records live in secondary `+0x78`, use 40 bytes each, and expose baseline/height through the existing `180168b90` interface.

## Concrete automatic callbacks

Static descriptor and vtable inspection identifies these class-248 geometry callbacks:

| Concrete type | Descriptor | Callback | Geometry function |
|---|---|---|---|
| 264 | `1802df4c0` | `18016c3a0` | `18016b220` |
| 29f | `1802df620` | `18016d9e0` | `18016cb60` |
| 24a | `1802df700` | `18016e850` | `18016e0c0` |

The callbacks operate on already assigned line stroke ranges. For type 24a, `18016e850` calls `18016e0c0`, stores its middle guideline as the baseline, and stores middle minus upper guideline as the line height. With the native explicit-scale flag, it replaces the computed height with the supplied scale. It then invokes the class-219 geometry-record adjustment interface and a second preprocessing pass. Those later steps are not part of the sparse estimator API.

`LegacySparseLineEstimator.TryEstimate` implements the exact **sparse branch of type 24a**. It accepts an explicit list of strokes belonging to one line, retains stroke boundaries when counting vertical runs, and returns false if the native branch would invoke the unported dense fit. It never substitutes a different dense algorithm.

The native count includes each strict up/down direction run plus a terminal observation for every stroke whose Y changes. Equal consecutive Y values preserve the current direction. The default sparse threshold is 10, established by `18016df90`; callers can supply the threshold from a resource profile.

Below that threshold, `18016b050` computes Y mean and population standard deviation using differences from the first Y. Differences and their squares use float; the accumulators and variance division use double. The native sparse branch then applies its original bbox aspect multiplier, width-dependent minimum spread, and tiny-band fallback. Its baseline is `mean + halfBand`; its upper guideline is `mean - halfBand`. The callback's height is the difference of these two float guidelines, computed in double and stored back as float. No neural weights participate in this branch.

This preserves occasionally surprising native output: a single point produces a ten-unit line height and a baseline five units below that point. It does not replace that behavior with a visual typography heuristic.

Static x64 checks cover:

- `18016e134..18016e1d9`: per-stroke vertical run count;
- `18016e1de..18016e1e1`: sparse/dense branch predicate;
- `18016b050..18016b1c7`: float-relative mean and standard deviation helper;
- `18016e384..18016e521`: sparse geometry, including a **double** `0.0001` comparison on the float half-band;
- `18016e850`: baseline/height materialization and optional supplied-height replacement.

## Dense branch evidence and limits

Type 24a's dense branch calls `18000b3d0` after `18000b300` initializes scalar fitting parameters. Inspection shows four Gaussian components over Y-extrema, a shared slope, capped iteration count of 20, rejection thresholds and per-component sufficient statistics. This is a geometric statistical fit in the inspected callback; it is not an embedded learned neural font-placement model. Its full parameter update and convergence branches are not yet ported. The older 264 / 29f dense fit variants also remain unported.

The inspected Text DLL is pinned to SHA-256 `18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`. The result does not establish that every other SDK rendering or recognition path is non-neural.

## Resource evidence

The type-24a v102 loader `18016eb90` reads twenty scalar bytes; it does not load tensor arrays. The ordered fields are first int32, second float, endpoint byte, min/max slope floats, sparse/dense threshold bytes, and one reserved byte. Slopes are already serialized as tangent values; the configure function `18016e020` separately converts supplied angles to tangent slopes.

`tools/inspect-legacy-baseline-profiles.py` inventories these original nodes without executing the engine. Current checked originals contain:

| Resource | Node offsets | Sparse/dense thresholds | Slope bounds |
|---|---|---|---|
| `models/en-US/en_US-ak-cur.lite.res` | 8486, 16842, 182230 | 10/10, 10/50, 10/50 | 0/0, approximately ±0.57735, approximately ±0.57735 |
| `models/ru-RU/ru_RU-ak-cur.lite.res` | 5789, 11421, 187976 | 10/10 for all three | 0/0, approximately ±1.633124e16 for the latter two |

Those values describe geometry profiles. They do not improve Russian character recognition or supply a missing Russian acoustic model.

## Verification and unresolved integration

The probe checks 232 independent own-ink controls: **159 estimated sparse lines and 73 explicit dense-branch declines**, including horizontal/vertical strokes, plateaus, multiple strokes, single points, aspect limits and the float-band/double-threshold boundary. Bounds, malformed input, overflow and cancellation are checked. Managed and Windows Native AOT runs pass.

```powershell
python tools/LegacySparseLineEstimator.Probe/reference_controls.py .artifacts/sparse-line-controls.json
dotnet run --project tools/LegacySparseLineEstimator.Probe -c Release -- .artifacts/sparse-line-controls.json
python tools/inspect-legacy-baseline-profiles.py models/ru-RU/ru_RU-ak-cur.lite.res models/en-US/en_US-ak-cur.lite.res
```

These are independent comparisons against the statically recovered branch, not comparisons with a running vendor engine. Automatic stroke-to-line grouping, the dense Gaussian fit, the native geometry-record adjustment and full font placement remain incomplete. This API should not be presented as complete automatic baseline parity.
