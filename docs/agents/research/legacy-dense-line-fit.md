# Prepared-line dense Gaussian geometry

`LegacyLineExtrema` and `LegacyGaussianLineFit` reproduce the bounded default-profile
geometry stage in Text `18000b3d0`, configured by `18000b300`. This stage receives
an already selected line of prepared strokes. It does not find line membership.
It outputs four global guideline intercepts and a shared slope. It is geometric
Gaussian fitting, rather than evidence of a learned font-size neural network.

The source inspected was the static x64 Text binary, SHA-256
`18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f`.
The original binary is not loaded or executed by these APIs or controls.

## Exact bounded stage

The native extremum producer separates points immediately before strict Y falls
and rises into two populations. Flat segments retain the preceding direction.
Endpoint inclusion changes the first-transition and last-point treatment; an
empty result duplicates the first point into both populations. The API's explicit
stroke input starts at the selected line, so the native first-flat-buffer-point
fallback does not involve an excluded prefix of another line.

The fit uses four fixed likelihood variances and four prior means. Initial
Y variance includes a `1e-6` accumulator seed and is divided by the combined
extremum count. Default standardized means are promoted float `2.6f`, double
`0.5`, promoted float `-0.65f` and double `-3`. Mean-prior variance factors are
`[2,4,4,2]`; likelihood factors are all `0.25`. Slope-prior variance is observation
count times promoted float `0.1f`. Normalization uses native double
`6.283185005187988`, rather than substituting the mathematical double `2π`.

Each of at most 20 iterations assigns the first population to component 0 or 1,
and the second to component 2 or 3. Ties select the first component. Exponential
arguments preserve the native cast to float before multiplication by `-0.5f`.
Low-likelihood points are rejected; the first iteration has a quarter-strength
rejection threshold. A MAP step then solves for the shared slope and component
means, with the original mean priors retained throughout. Outer-component prior
variances double after each step. Native stopping compares RMS mean movement
strictly to zero. At completion, centered-X means become global Y intercepts.

This API supports the actual default profile only, with its flags value 5 and
unit class weights. It does not advertise support for arbitrary native profiles
or the flags-1 optional postconstraint. Public preparation caps total input at one
million points and the fit at 16384 extrema; cancellation and finite-coordinate
checks bound malformed input. Numerical results use managed double arithmetic and
`Math.Exp`; the independent controls verify tolerances, rather than claiming
bit-identical CRT transcendental implementations on every platform.

## Remaining native stages

Class `24a` `18016e0c0` wraps this stage with resource-dependent slope clamping and,
for fewer than 20 vertical runs, blends its central guidelines with anchored Y
statistics. Those wrapper adjustments are not part of `LegacyGaussianLineFit`.
The existing sparse estimator covers the low-run branch separately. Automatic
candidate grouping, dense wrapper integration, callback adjustment passes and
recognition-result typography are separate unfinished boundaries. These APIs do
not establish full native-engine equivalence or a complete automatic-baseline
pipeline.

## Independent controls

`tools/LegacyGaussianLineFit.Probe/reference_controls.py` creates original synthetic
stroke and extremum populations. Its fitting oracle independently solves a full
5×5 normal-equation system with NumPy, rather than copying the port's scalar
Schur elimination. Its extrema oracle uses non-flat transition neighbours.

```powershell
python tools/LegacyGaussianLineFit.Probe/reference_controls.py .artifacts/gaussian-line-controls.json
dotnet build tools/LegacyGaussianLineFit.Probe/LegacyGaussianLineFit.Probe.csproj -c Release --artifacts-path D:/BlurMask-build-artifacts-20261011/gaussian-line-build
dotnet D:/BlurMask-build-artifacts-20261011/gaussian-line-build/bin/LegacyGaussianLineFit.Probe/release/LegacyGaussianLineFit.Probe.dll .artifacts/gaussian-line-controls.json
```

Managed and Windows x64 NativeAOT both pass 190 independent fitted populations
and 130 original stroke-to-extrema controls, containing 1713 extrema. The probe
also verifies empty/nonfinite/oversized input rejection and cancellation. These
are independently generated mathematical controls backed by static disassembly,
not comparisons obtained by executing the original vendor fitter.
