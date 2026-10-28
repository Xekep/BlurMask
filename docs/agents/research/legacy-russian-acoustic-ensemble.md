# Legacy calibrated acoustic ensemble

`LegacyAcousticEnsemble` restores the inspected single-profile mode1 aggregate
branch of `1801bcbd0`, with per-family/per-class logistic calibration from
`18013e8e0`. It requires seven explicit family slots, null for inactive families,
and caller-owned matching center/slope arrays for each active output. It does
not select families, create masks, infer coefficients or distribute assets.

Each quantized EXP probability becomes float `probability * (1f/32768)`. The
helper widens that float and the stored center/slope to double, computes
`exp(-(probability-center)*slope)`, returns zero when the exponent result reaches
float maximum, otherwise narrows `1/(1+exp)` to float. Individual calibrated
confidences are added in the native order: slot1+slot0, then slots2..6. The
single-profile result divides by the count of active families and is capped
above at1. This is **not the mean of raw EXP probabilities**; per-class
calibration can materially change scores and their ordering.

The supported mode matches the inspected RU configuration's main-only
`[null,EXP,null]` network triples. Reader `1801c25b0` stores those three network
pointers at offsets+0x10/+0x18/+0x20; constructor `1801b0760` sets global271/272
flags from the first/third pointers, which are absent here. With valid alphabet
special-output metadata, nonraw-output mode and mode1 calibration, `1801bcbd0`
uses float probability getter slot0xd8 and the class/model coefficient table.
Other branches select log-score getters, raw separated outputs, generic
13.9/0.575 calibration or signed mode2 transforms. They are not substituted
for this checked mode and are not implemented by this component.

The private inspected table has192 classes×4 active families×2 floats, with
center then slope in model-major order. For example, acoustic Russian м107
has center0.1315020025 and slope27.6284008. Callers must supply the correct
coefficient rows when evaluating a subset, rather than relabeling family IDs
or guessing universal defaults. The restored3f and4f pair is only a subset of
the four active families. The later [word diagnostic](legacy-russian-directional-words.md)
also evaluates the checked256-input1f and238-input2f baseline profiles, with
explicit caller context; native lexical/case costs and joined-stroke decoding
still remain unfinished.

The bounded API permits1..4096 equal output classes and up to seven active
families, quantized EXP values0..32767, finite centers and finite positive
slopes. It rejects an empty ensemble, mismatched dimensions and invalid inputs,
and checks cancellation. Managed and Windows Native AOT pass640 independent
own-probability/coefficient controls with absolute tolerance1e−7, plus analytic
center equality, active-family counting, extreme exponent fallback, saturated
confidence, malformed and cancellation tests. No native SDK execution is used.

The output is a per-class calibrated confidence array, not a normalized
probability distribution or decoded text. Any subsequent confidence
quantization/dictionary alias mapping must be identified as a separate caller
policy unless its native path has also been restored. Word and representative
joined Russian cursive accuracy have not been established by these tests.

```powershell
python tools/LegacyAcousticEnsemble.Probe/reference_controls.py .artifacts/ru-ensemble.json
dotnet run --project tools/LegacyAcousticEnsemble.Probe -c Release -- .artifacts/ru-ensemble.json
```
