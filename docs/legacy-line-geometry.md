# Prepared line geometry and scale selection

`LegacyLineGeometry` and `LegacyLineScale` expose two checked scalar portions
of the Text class `219` line-geometry provider. Both accept prepared inputs;
neither substitutes a whole-word bounding box for the native estimator.

The concrete provider implements interface `218`. Its line getter `180168b90`
returns a 40-byte line record whose first fields are float32 baseline and
height. Its scale getter `180169050` converts the stored double primary scale
to float32. The materializer `1801629c0` reads a prepared 24-byte cluster:

* For a positive-height cluster, baseline is `row * pitch + baseline`, and
  height is the cluster height. A one-row cluster uses row zero.
* Without a positive-height cluster, a prepared 16-byte fallback record gives
  `baseline = y + .75 * height` and `line height = .25 * height`.

The API requires the caller to provide a valid row index. Native row inference
from item extrema, item kinds, cluster assignment and subsequent adjustments
are outside this component. The fallback record is not proven to be the full
ink bounding box; applying the formula to arbitrary word bounds would be an
additional heuristic.

The scale selector `18015af40` sorts positive float32 prepared samples. It
selects primary scale at the rounded `(sampleCount - 1) * quantile` index.
The secondary quantile blends two explicit profile quantiles using a ratio
derived from sample count, associated-record count, record weight and blend
numerator; its index instead uses `sampleCount`. Float32 and double arithmetic
stages, integer truncation of each associated-record weight, half-away-from-zero
index rounding and the upper index cap are retained. Input resolution divided
by `25.4` provides the primary scale floor and proportionally adjusts secondary
scale. With no samples, both scales are resolution divided by `25.4`, or one
when resolution is unavailable.

The implementation rejects nonfinite inputs, unsupported counts and invalid
profiles, copies samples before sorting, and accepts cancellation. It supplies
no default profile coefficients, automatic sample producer or language tuning.

The enclosing commit `1801680d0` also calls item grouping, scale-sample
production, cluster selection and geometry adjustments. Its helper `18015a1c0`
selects between classes `2e2` and `2e3`, whose estimator operations are invoked
by `18015b260` and `18015f050`. These upstream operations are not yet ported.
The observed scalar materialization does not establish a separate neural model
that selects the replacement font size or position. Likewise, the English
20-input/two-output model is consumed by segmentation costs (`180192ef0` /
`1801941b0`), rather than being a baseline or font-size regressor. Absence of
such a model throughout the complete SDK has not been established.

The independent Python controls use only generated numerical data. Managed
and Windows Native AOT executions pass 280 scale selections and 400 line-pair
comparisons, including sparse/dense blending, fractional record weights,
resolution floors, negative positions and multiple row offsets. Own checks
also cover input immutability, distinct primary/secondary indices, unavailable
cluster fallback, rejected records and cancellation. These are scalar/static
transcription checks, not native SDK execution, automatic-line accuracy or
complete handwriting-recognition parity.

```text
python tools/LegacyLineGeometry.Probe/reference_controls.py .artifacts/legacy-line-geometry-controls.json
dotnet run --project tools/LegacyLineGeometry.Probe -c Release -- .artifacts/legacy-line-geometry-controls.json
```
