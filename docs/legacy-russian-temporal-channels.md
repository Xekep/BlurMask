# Legacy temporal directional channels

`LegacyDirectionalWindowStatistics` restores the checked `0b7f60` sixteen raw
statistics per window: center and bbox fractions, physical stroke endpoint
counts, dynamically scaled signed covariance, direction reversals and the three
intersection classes. Point memberships and both flag arrays are caller data.
Empty windows return zero raw statistics. Interior-class fractions use the
whole-glyph point count. For negative coordinate scale, the native local bbox
unit becomes zero; the whole prepared geometry separately uses unit 1.

`LegacyDirectionalFeatureAssembly` restores the checked `0b9720` temporal
channel transformations and order. The checked masks select all17 global
statistics and all16 local statistics in each of16 temporal windows. The
input layout is:

| Columns | Contents |
| --- | --- |
| 0 | Bias1024 |
| 1–17 | Global shape/covariance/reversal/intersection statistics |
| 18–273 | Sixteen local windows, sixteen statistics each |
| 274–282 | Nine caller-supplied raw baseline statistics, transformed |
| 283–310 | Twenty-eight caller-supplied context values |
| 311–312 | Caller-supplied start/end boundary values |

The first274 geometry columns are not directly the network input: the checked
native 274-column profile calibrates columns 1–273 using distinct min/max ranges
before appending baseline. `TransformGeometry` requires explicit caller-supplied
274-element signed-short calibration arrays. Equal ranges retain the raw
channel, bias remains1024, signed integer truncation and short overflow are
preserved, and output is not clamped. No trained calibration ranges are bundled
in source. `AppendInput` requires all nine raw baseline and twenty-eight context
values instead of guessing unavailable inputs or filling them with zeros.

The APIs implement the checked3f temporal layout. They do not claim that the
4f alternative local-window layout is equivalent, infer baselines or context,
segment cursive characters, or recognize words by themselves.

Managed and Windows Native AOT controls pass against separate Python references:
640 own local-statistic cases and512 own complete313-column arithmetic/layout
cases, including overflow, signed covariance and nontrivial synthetic min/max
ranges. Analytic line/empty-window/singleton/negative-scale controls and malformed
input/cancellation also pass. The references execute no vendor engine and
include no trained assets or vendor ink. These numerical tests are not a
handwriting-accuracy benchmark or proof of full SDK behavior.

```powershell
python tools/LegacyDirectionalWindow.Probe/reference_controls.py .artifacts/ru-temporal.json
dotnet run --project tools/LegacyDirectionalWindow.Probe -c Release -- .artifacts/ru-temporal.json
```

Actual model recognition still requires verified caller baseline/context
profiles and representative joined-word ink controls. AI-pen UI does not enable
these experimental components.
