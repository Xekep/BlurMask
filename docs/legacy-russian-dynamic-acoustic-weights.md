# Dynamic acoustic mixture: type257 and stored type21d configuration

`LegacyDynamicAcousticWeights` restores the bounded float geometry route
`ce610 → cece0`, exposed by type257. This is parallel-segment overlap geometry,
not a learned baseline estimator. Parameters remain explicit unless obtained
from `LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix`.

## Native evidence and arithmetic

MyScriptText descriptor1802e5180 has method table1802e51c8. Direct getter
instructions independently establish:

- `cebc0`: float object+24, the stored overlap aggregate;
- `cebd0`: float1 minus object+20, the mode1 acoustic weight;
- `cebe0`: float object+20, the mode2 acoustic weight.

`cf990` calls `cece0` over the inclusive full physical-stroke interval. `ce610`
resamples physical arc length at `totalLength/(SampleCount−1)`. It carries a
float residual across pen-up, explicitly emits each subsequent stroke's first
point and emits the first stroke only when a sample is reached. Zero total arc
copies the first point of each physical stroke. Interpolation instructions use
float multiply/add without fusion; square root widens an already float-rounded
squared distance to double and narrows the result. Repeated endpoints remain.

`cece0` compares segment midpoint distances and absolute normalized cross
products. Adjacent same-stroke segments within the exclusion radius are skipped.
Nearby sufficiently parallel segments compete for a maximum per-segment overlap,
with a separate same-stroke factor. Pen-up and zero-length segments contribute
zero, but the aggregate denominator remains all flattened segment slots.
Double accumulation of float overlaps feeds the configured logistic transform
before the aggregate itself is stored as float. Equal configured lower/upper
endpoints are reset to0/1 by constructor`ce4b0`.

`1b7440` copies the three getters to recognizer1b8,1bc,1c0. Native integer
acoustic scorer`1b9e20` resolves mode0 from its stored family float, mode1 from
recognizer1b8, mode2 from1bc. Seven NN slots are not seven lexical categories.

The reusable evaluator imposes bounded finite geometry, sample/parameter ranges,
ordered weight endpoints in0..1,4096 source/resampled points and cancellation.
It uses platform `Math.Exp`; the engine's proprietary exp implementation has
not been asserted bit-identical. Independent controls compare float outputs
within2e−6. This does not infer native instance configuration overrides.

## Actual Russian resource configuration

`1c2940` is the serialized type21d reader, and`1c25b0` reads each family record.
The committed Russian resource's type21d at189734 has version110. Its initial
network occupies189755..195746. After its context fields, enabled byte195770
is1; sample count at195771 is30. The following fields, mapped by runtime
initializer`1b0760` to the40-byte type257 constructor, are:

| Parameter | Stored value |
| --- | --- |
| Radius fraction | .15 |
| Exclusion fraction | .30 |
| Same-stroke factor | .40 |
| Sine limit | .50 |
| Logistic center | .95 |
| Logistic slope |25 |
| Lower weight | .20 |
| Upper weight |1 |

The ninth float.10 is recognizer property1c4 and is not a type257 parameter.
Version110 initializes the source cost multiplier to1 in`1c2940`; application
of that multiplier is an independent runtime flag, left explicit downstream.

The single stored profile's seven family records begin195889,283691,367893,
467095,566297,566308,566319. Slots0..3 have mode2, both flags set, and only the
second NN of each record present. Slots4..6 have mode0, stored weight0, neither
flag and no NN. Prefix consumption ends566330; remaining type21d properties
are not claimed parsed. Every NN contribution still undergoes its own integer
conversion; identical dynamic weights must not be algebraically cancelled.

The loader supports the checked version110 prefix with one no-grid profile,
initial network, enabled dynamic weights, bounded short lists and no nested
a0 conversion. It skips version106 network payloads using checked declared
lengths; the existing NN loader handles matrix contents separately. Unknown
versions or unsupported branches fail closed. `ResolveWeights` selects only
the proven mode0/1/2 values from explicitly supplied runtime geometry results.
Runtime enabled-family selection and native full word search remain separate.

## Checks

```powershell
python tools/LegacyDynamicWeights.Probe/reference_controls.py .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-dynamic-weights-controls.json
dotnet run --project tools/LegacyDynamicWeights.Probe -c Release -- --controls .artifacts/myscript-analysis/output/RussianAssetsSearch/legacy-dynamic-weights-controls.json
dotnet run --project tools/LegacyDynamicWeights.Probe -c Release -- --resource models/ru-RU/ru_RU-ak-cur.lite.res
```

The own fixture covers zero arc, close parallel traces, translation, configured
logistic midpoint, equal endpoint reset, cancellation and invalid parameters.
512 independent Python float32 controls cover resampling and overlap. Own
serialized bytes test mode0/1/2 resolution, exact prefix consumption, every
truncation and unsupported version. Actual bytes independently check all
parameters, seven modes/flags/network masks and prefix endpoint. These controls
are arithmetic/resource validation, not Russian handwriting accuracy or GUI
integration. Float prepared-point to normalized-short geometry conversion and
native variable-cut word search remain separate unresolved upstream stages.
