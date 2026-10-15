# Active29B mixture from actual recognized source geometry

`LegacyFreshMainActiveGapPreparation.Prepare` closes the fresh unknown-source
branch of Text `175350`, including the formerly refused active Gaussian mixture.
It accepts the actual main24D owner, reads the pinned29B profile and produces
qualifying gaps from real source720 boxes/current main guides. It does not
accept a replacement gap, mixture threshold, hardcoded letter label or
first-child frame substituted for the main source.

## Qualifying observation ownership

Within each ordinary descriptor, `175260` center-sorts the complete(center,rowID)
records, including its native tie swaps. A forward maximum-right frontier
produces float normalized gaps clamped to0..4. All observations feed the
mean/variance estimate. Only descriptors with more than10 gaps feed the actual
mixture sample vector and count blend. The blend is
`clamp((qualifyingGapCount−10)/(30−10),0,1)` for the actual pinned profile.

The owning fresh720 producer `156950` publishes item+10 as−1. Native `175350`
therefore leaves its recognized-input exclusion predicate false and includes
these qualifying observations. The prepared full producer explicitly checks
that field. Pre-existing recognized-input720 items require the separately
owned physical flag1000/source nibbles predicate and are refused here; their
category is not guessed from original point keys or Unicode labels.

## Actual fixed-variance EM

`LegacyNativeOrdinaryGapMixture.Fit` restores the native fixed-variance two-
component fit. Actual profile seeds are means0.15/1.5, variances0.1/0.1 and
priors0.8/0.2. Each retry divides the original variances by2^retry, with at most
five attempts. Variances are not estimated from samples.

Each iteration starts with the profile-normalized prior pseudo-counts and
their weights multiplied by the **current** component means. It evaluates
`exp(−0.5*d*d/variance)*currentPrior`, without a square-root density normalizer
in this EM loop. A native epsilon guard skips negligible total density.
Posterior weights update the mean sums/counts and priors; means and priors
are rounded tofloat on every native field write. Convergence compares the
float mean changes, promoted todouble, against double1e−6. Previous-mean slots
are preserved between variance retries.

After convergence, the intersection calculation does use
`prior/sqrt(2πvariance)` and the log density ratio. It handles equal quadratic
coefficients, two real roots and a negative-discriminant float midpoint. With
two roots it chooses the one having greater first-component density. An
intersection outside the ordered means, or effectively equal means, retries
with smaller variances. The library retains the native five-attempt outcome
when it is finite; it does not silently replace it with a midpoint/default.
An intermediate infinite/NaN intersection is retained in the attempt trace and
reaches the native placement/convergence tests before a narrower-variance retry.
Assembly175FDB..176117 confirms there is no finite-value rejection between the
equal-quadratic division and those retry tests. Nonfinite state, a nonfinite
intersection after the final retry and excessive portable iteration counts
remain explicit portable failures.

The final publication blends the ordinary mean/stddev estimate with the
mixture intersection, preserving native float rounding on `1−blend` and
`float(intersection)*blend`, then applies the serialized maximum. No-active-
mixture cases reuse the already verified bounded29B implementation, including
incoming9C preservation and the genuinely zero-gap1.5 branch.

## Complete fresh continuation through265

`LegacyFreshMainActiveGapPreparation.Smooth` carries the exact owning29B result
through265. It consumes each descriptor's actual current guide height and
the original15ABE0 source metrics, then composes factor200 fractional keys
through the prior physical-reordering map with native uint32 arithmetic.
The produced source gap and source720 publication remain unchanged.

The real integration fixture uses fourteen separate raw trajectories. Both
embedded2E3 networks, main split/reorder, first acoustic/cache/pruned graph/
protocol/native275 publication and source720 materialization execute. Fourteen
actual recognized items produce thirteen qualifying gaps, blend0.15, three EM
attempts and source gap1.6429254. Subsequent265 produces434 points. An
independent NumPy/dense spline oracle verifies the actual observations,
every EM attempt, publication, smoothing coordinates and exact original
point-key composition. No expected OCR labels are supplied or corrected.
The first fixture still uses an explicit diagnostic all-enabled linguistic
mask; this check does not establish complete automatic language policy,
native winner orchestration, general cursive accuracy or application font fit.

## Reproduction

The independent suite includes192 component-fit cases:181 finite results with
286 retry attempts, including all five retries, and11 final-nonfinite-intersection
refusals. Another64 source720 fixtures verify the actual qualifying-gap
producer, tie sorting, mean/variance and blended publication. Asymmetric and
reversed component profiles test paths beyond the actual equal-variance seed.

The optional final probe argument15 reproduces an additional real counterexample
with fifteen equally spaced raw M-shaped trajectories. Fourteen qualifying gaps
produce20/1564/6 iterations in three attempts. Attempt1's means become the same
binary32 value, so its equal-variance linear denominator is zero and its root is
negative infinity. Native retry2 recovers a finite intersection1.10086338156;
the final gap is1.61128354 and265 produces465 points. The earlier per-attempt
finite guard incorrectly rejected this recoverable state. It now runs only
after the retry loop. Finite14-stroke regression output remains unchanged.
The NumPy audit reconstructs exported single-precision input coordinates before
recomputing gaps, and compares the nonfinite intermediate symbolically as well
as every finite state and final publication. Nonfinite trace values are encoded
as explicit JSON strings, not invalid JSON numeric literals.

```powershell
python tools/LegacyOrdinaryGapMixture.Probe/reference_controls.py models/ru-RU/ru_RU-ak-cur.lite.res E:/BlurMask-build-artifacts-20261014/gap-mixture-controls.json
dotnet run --project tools/LegacyOrdinaryGapMixture.Probe -c Release --artifacts-path E:/BlurMask-build-artifacts-20261014/main-processing-build -- models/ru-RU/ru_RU-ak-cur.lite.res models/ru-RU/legacy-engine-profile.json models/ru-RU/legacy-separation-profile.json models/line-geometry/line-2e3-30.res models/line-geometry/line-2e3-50.res E:/BlurMask-build-artifacts-20261014/gap-mixture-output.json models/text-unicode/legacy-case-data.json models/text-unicode/legacy-text-publication-data.json E:/BlurMask-build-artifacts-20261014/gap-mixture-controls.json
python tools/LegacyOrdinaryGapMixture.Probe/audit_actual.py models/ru-RU/ru_RU-ak-cur.lite.res E:/BlurMask-build-artifacts-20261014/gap-mixture-output.json
python tools/handwriting/audit_fresh_main_gap_mixture.py --output E:/BlurMask-build-artifacts-20261014/gap-mixture-static.json
```

Managed and fresh Windows NativeAOT execute the same suite. Static proof reads
only pinned PE/resource bytes; runtime uses portable data and never executes a
vendor DLL, GUI or installer. The recognized-input physical exclusion gate,
prior-source/model state and actual empty-first24A fallback remain separate
boundaries.
