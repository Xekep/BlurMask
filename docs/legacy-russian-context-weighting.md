# Legacy prior context weighting

`LegacyContextWeighting` restores the checked `1801ca5f0` arithmetic after
prior-group selection. Caller supplies target mean X, line height, left gap
offset, left/right variances and selected prior observations containing mean
X and28 neural probabilities. The bounded API accepts at most1024 observations.

For nonnegative normalized X distance it uses the right variance. For negative
distance it adds the left gap offset, uses the left variance while the adjusted
distance is nonpositive, and otherwise gives full influence. The exponent is
`−distance² / (2×variance)` with float32 operation order. Exponents below −75 are
skipped; `exp` receives a double converted from the float exponent, then narrows
to float before multiplication. Each channel retains the maximum weighted
probability. The final context channel is `2p−1`, snapped to −1 when below −.99.
An empty accepted context therefore produces28 channels of −1. Duplicate
observations do not accumulate probability.

Static path evidence for the inspected native generation:

- `1801b0760` creates type227 and allocates its float context buffer at parent
  +268, with channel count at +260.
- Type227 virtual table at `1802e4d60` has setup `1801ca530`, channel count
  `1801ca5d0` and aggregation `1801ca5f0`.
- `1801c9870` precomputes candidate windows of previous physical strokes, calls
  type226 extraction, evaluates the initial network and stores accepted neural
  probabilities and group centers.
- `1801b7f70` obtains the selected context; `1801b75b0` appends it to enabled
  classifier families. `1801b8320` also accepts an explicit caller context.
- Type226's `1801c91e0` serializes geometry statistics into the30-float initial
  network input. This is a separate feature profile from the restored313
  directional vector and the older30-sample trajectory geometry.

This API restores only the aggregation arithmetic. It does not generate the30
geometry inputs or their neural probabilities, select matching line/group
observations, infer source baselines or recognize cursive words. Caller-selected
observations must already exclude incompatible lines and overlapping current
ink as required by the full selection pass. The float channels also require the
verified native float-to-fixed conversion before being appended to an integer
classifier; this API does not guess that conversion. Use the separate checked
[`LegacyFloatInputQuantization`](legacy-russian-float-input.md) component for it,
[`LegacyPriorContextSelection`](legacy-russian-prior-context.md) for accepted-group
filtering, and [`LegacyContextGeometry`](legacy-russian-context-geometry.md) for
the explicit 30-channel geometry profile.

Managed and Windows Native AOT tests pass512 own independent Python float32
reference cases (absolute tolerance2e−7), plus analytic empty-context,
max-aggregation, offset plateau, translation, exponent cutoff, near−1 snap,
bounded work, invalid geometry/probability and cancellation controls. They
execute no vendor engine and contain no trained assets. They do not establish
handwriting accuracy or full SDK behavior.

```powershell
python tools/LegacyContextWeighting.Probe/reference_controls.py .artifacts/ru-context.json
dotnet run --project tools/LegacyContextWeighting.Probe -c Release -- .artifacts/ru-context.json
```
